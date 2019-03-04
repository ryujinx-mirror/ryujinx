using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Graphics3d
{
    class MacroInterpreter
    {
        private enum AssignmentOperation
        {
            IgnoreAndFetch                  = 0,
            Move                            = 1,
            MoveAndSetMaddr                 = 2,
            FetchAndSend                    = 3,
            MoveAndSend                     = 4,
            FetchAndSetMaddr                = 5,
            MoveAndSetMaddrThenFetchAndSend = 6,
            MoveAndSetMaddrThenSendHigh     = 7
        }

        private enum AluOperation
        {
            AluReg                = 0,
            AddImmediate          = 1,
            BitfieldReplace       = 2,
            BitfieldExtractLslImm = 3,
            BitfieldExtractLslReg = 4,
            ReadImmediate         = 5
        }

        private enum AluRegOperation
        {
            Add                = 0,
            AddWithCarry       = 1,
            Subtract           = 2,
            SubtractWithBorrow = 3,
            BitwiseExclusiveOr = 8,
            BitwiseOr          = 9,
            BitwiseAnd         = 10,
            BitwiseAndNot      = 11,
            BitwiseNotAnd      = 12
        }

        private NvGpuFifo    _pFifo;
        private INvGpuEngine _engine;

        public Queue<int> Fifo { get; private set; }

        private int[] _gprs;

        private int _methAddr;
        private int _methIncr;

        private bool _carry;

        private int _opCode;

        private int _pipeOp;

        private int _pc;

        public MacroInterpreter(NvGpuFifo pFifo, INvGpuEngine engine)
        {
            _pFifo  = pFifo;
            _engine = engine;

            Fifo = new Queue<int>();

            _gprs = new int[8];
        }

        public void Execute(NvGpuVmm vmm, int[] mme, int position, int param)
        {
            Reset();

            _gprs[1] = param;

            _pc = position;

            FetchOpCode(mme);

            while (Step(vmm, mme));

            //Due to the delay slot, we still need to execute
            //one more instruction before we actually exit.
            Step(vmm, mme);
        }

        private void Reset()
        {
            for (int index = 0; index < _gprs.Length; index++)
            {
                _gprs[index] = 0;
            }

            _methAddr = 0;
            _methIncr = 0;

            _carry = false;
        }

        private bool Step(NvGpuVmm vmm, int[] mme)
        {
            int baseAddr = _pc - 1;

            FetchOpCode(mme);

            if ((_opCode & 7) < 7)
            {
                //Operation produces a value.
                AssignmentOperation asgOp = (AssignmentOperation)((_opCode >> 4) & 7);

                int result = GetAluResult();

                switch (asgOp)
                {
                    //Fetch parameter and ignore result.
                    case AssignmentOperation.IgnoreAndFetch:
                    {
                        SetDstGpr(FetchParam());

                        break;
                    }

                    //Move result.
                    case AssignmentOperation.Move:
                    {
                        SetDstGpr(result);

                        break;
                    }

                    //Move result and use as Method Address.
                    case AssignmentOperation.MoveAndSetMaddr:
                    {
                        SetDstGpr(result);

                        SetMethAddr(result);

                        break;
                    }

                    //Fetch parameter and send result.
                    case AssignmentOperation.FetchAndSend:
                    {
                        SetDstGpr(FetchParam());

                        Send(vmm, result);

                        break;
                    }

                    //Move and send result.
                    case AssignmentOperation.MoveAndSend:
                    {
                        SetDstGpr(result);

                        Send(vmm, result);

                        break;
                    }

                    //Fetch parameter and use result as Method Address.
                    case AssignmentOperation.FetchAndSetMaddr:
                    {
                        SetDstGpr(FetchParam());

                        SetMethAddr(result);

                        break;
                    }

                    //Move result and use as Method Address, then fetch and send paramter.
                    case AssignmentOperation.MoveAndSetMaddrThenFetchAndSend:
                    {
                        SetDstGpr(result);

                        SetMethAddr(result);

                        Send(vmm, FetchParam());

                        break;
                    }

                    //Move result and use as Method Address, then send bits 17:12 of result.
                    case AssignmentOperation.MoveAndSetMaddrThenSendHigh:
                    {
                        SetDstGpr(result);

                        SetMethAddr(result);

                        Send(vmm, (result >> 12) & 0x3f);

                        break;
                    }
                }
            }
            else
            {
                //Branch.
                bool onNotZero = ((_opCode >> 4) & 1) != 0;

                bool taken = onNotZero
                    ? GetGprA() != 0
                    : GetGprA() == 0;

                if (taken)
                {
                    _pc = baseAddr + GetImm();

                    bool noDelays = (_opCode & 0x20) != 0;

                    if (noDelays)
                    {
                        FetchOpCode(mme);
                    }

                    return true;
                }
            }

            bool exit = (_opCode & 0x80) != 0;

            return !exit;
        }

        private void FetchOpCode(int[] mme)
        {
            _opCode = _pipeOp;

            _pipeOp = mme[_pc++];
        }

        private int GetAluResult()
        {
            AluOperation op = (AluOperation)(_opCode & 7);

            switch (op)
            {
                case AluOperation.AluReg:
                {
                    AluRegOperation aluOp = (AluRegOperation)((_opCode >> 17) & 0x1f);

                    return GetAluResult(aluOp, GetGprA(), GetGprB());
                }

                case AluOperation.AddImmediate:
                {
                    return GetGprA() + GetImm();
                }

                case AluOperation.BitfieldReplace:
                case AluOperation.BitfieldExtractLslImm:
                case AluOperation.BitfieldExtractLslReg:
                {
                    int bfSrcBit = (_opCode >> 17) & 0x1f;
                    int bfSize   = (_opCode >> 22) & 0x1f;
                    int bfDstBit = (_opCode >> 27) & 0x1f;

                    int bfMask = (1 << bfSize) - 1;

                    int dst = GetGprA();
                    int src = GetGprB();

                    switch (op)
                    {
                        case AluOperation.BitfieldReplace:
                        {
                            src = (int)((uint)src >> bfSrcBit) & bfMask;

                            dst &= ~(bfMask << bfDstBit);

                            dst |= src << bfDstBit;

                            return dst;
                        }

                        case AluOperation.BitfieldExtractLslImm:
                        {
                            src = (int)((uint)src >> dst) & bfMask;

                            return src << bfDstBit;
                        }

                        case AluOperation.BitfieldExtractLslReg:
                        {
                            src = (int)((uint)src >> bfSrcBit) & bfMask;

                            return src << dst;
                        }
                    }

                    break;
                }

                case AluOperation.ReadImmediate:
                {
                    return Read(GetGprA() + GetImm());
                }
            }

            throw new ArgumentException(nameof(_opCode));
        }

        private int GetAluResult(AluRegOperation aluOp, int a, int b)
        {
            switch (aluOp)
            {
                case AluRegOperation.Add:
                {
                    ulong result = (ulong)a + (ulong)b;

                    _carry = result > 0xffffffff;

                    return (int)result;
                }

                case AluRegOperation.AddWithCarry:
                {
                    ulong result = (ulong)a + (ulong)b + (_carry ? 1UL : 0UL);

                    _carry = result > 0xffffffff;

                    return (int)result;
                }

                case AluRegOperation.Subtract:
                {
                    ulong result = (ulong)a - (ulong)b;

                    _carry = result < 0x100000000;

                    return (int)result;
                }

                case AluRegOperation.SubtractWithBorrow:
                {
                    ulong result = (ulong)a - (ulong)b - (_carry ? 0UL : 1UL);

                    _carry = result < 0x100000000;

                    return (int)result;
                }

                case AluRegOperation.BitwiseExclusiveOr: return   a ^  b;
                case AluRegOperation.BitwiseOr:          return   a |  b;
                case AluRegOperation.BitwiseAnd:         return   a &  b;
                case AluRegOperation.BitwiseAndNot:      return   a & ~b;
                case AluRegOperation.BitwiseNotAnd:      return ~(a &  b);
            }

            throw new ArgumentOutOfRangeException(nameof(aluOp));
        }

        private int GetImm()
        {
            //Note: The immediate is signed, the sign-extension is intended here.
            return _opCode >> 14;
        }

        private void SetMethAddr(int value)
        {
            _methAddr = (value >>  0) & 0xfff;
            _methIncr = (value >> 12) & 0x3f;
        }

        private void SetDstGpr(int value)
        {
            _gprs[(_opCode >> 8) & 7] = value;
        }

        private int GetGprA()
        {
            return GetGprValue((_opCode >> 11) & 7);
        }

        private int GetGprB()
        {
            return GetGprValue((_opCode >> 14) & 7);
        }

        private int GetGprValue(int index)
        {
            return index != 0 ? _gprs[index] : 0;
        }

        private int FetchParam()
        {
            int value;

            if (!Fifo.TryDequeue(out value))
            {
                Logger.PrintWarning(LogClass.Gpu, "Macro attempted to fetch an inexistent argument.");

                return 0;
            }

            return value;
        }

        private int Read(int reg)
        {
            return _engine.Registers[reg];
        }

        private void Send(NvGpuVmm vmm, int value)
        {
            GpuMethodCall methCall = new GpuMethodCall(_methAddr, value);

            _engine.CallMethod(vmm, methCall);

            _methAddr += _methIncr;
        }
    }
}