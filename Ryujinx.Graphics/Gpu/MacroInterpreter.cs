using ChocolArm64.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu
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

        private NvGpuFifo    PFifo;
        private INvGpuEngine Engine;

        public Queue<int> Fifo { get; private set; }

        private int[] Gprs;

        private int MethAddr;
        private int MethIncr;

        private bool Carry;

        private int OpCode;

        private int PipeOp;

        private long Pc;

        public MacroInterpreter(NvGpuFifo PFifo, INvGpuEngine Engine)
        {
            this.PFifo  = PFifo;
            this.Engine = Engine;

            Fifo = new Queue<int>();

            Gprs = new int[8];
        }

        public void Execute(AMemory Memory, long Position, int Param)
        {
            Reset();

            Gprs[1] = Param;

            Pc = Position;

            FetchOpCode(Memory);

            while (Step(Memory));

            //Due to the delay slot, we still need to execute
            //one more instruction before we actually exit.
            Step(Memory);
        }

        private void Reset()
        {
            for (int Index = 0; Index < Gprs.Length; Index++)
            {
                Gprs[Index] = 0;
            }

            MethAddr = 0;
            MethIncr = 0;

            Carry = false;
        }

        private bool Step(AMemory Memory)
        {
            long BaseAddr = Pc - 4;

            FetchOpCode(Memory);

            if ((OpCode & 7) < 7)
            {
                //Operation produces a value.
                AssignmentOperation AsgOp = (AssignmentOperation)((OpCode >> 4) & 7);

                int Result = GetAluResult();

                switch (AsgOp)
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
                        SetDstGpr(Result);

                        break;
                    }

                    //Move result and use as Method Address.
                    case AssignmentOperation.MoveAndSetMaddr:
                    {
                        SetDstGpr(Result);

                        SetMethAddr(Result);

                        break;
                    }

                    //Fetch parameter and send result.
                    case AssignmentOperation.FetchAndSend:
                    {
                        SetDstGpr(FetchParam());

                        Send(Memory, Result);

                        break;
                    }

                    //Move and send result.
                    case AssignmentOperation.MoveAndSend:
                    {
                        SetDstGpr(Result);

                        Send(Memory, Result);

                        break;
                    }

                    //Fetch parameter and use result as Method Address.
                    case AssignmentOperation.FetchAndSetMaddr:
                    {
                        SetDstGpr(FetchParam());

                        SetMethAddr(Result);

                        break;
                    }

                    //Move result and use as Method Address, then fetch and send paramter.
                    case AssignmentOperation.MoveAndSetMaddrThenFetchAndSend:
                    {
                        SetDstGpr(Result);

                        SetMethAddr(Result);

                        Send(Memory, FetchParam());

                        break;
                    }

                    //Move result and use as Method Address, then send bits 17:12 of result.
                    case AssignmentOperation.MoveAndSetMaddrThenSendHigh:
                    {
                        SetDstGpr(Result);

                        SetMethAddr(Result);

                        Send(Memory, (Result >> 12) & 0x3f);

                        break;
                    }
                }
            }
            else
            {
                //Branch.
                bool OnNotZero = ((OpCode >> 4) & 1) != 0;

                bool Taken = OnNotZero
                    ? GetGprA() != 0
                    : GetGprA() == 0;

                if (Taken)
                {
                    Pc = BaseAddr + (GetImm() << 2);

                    bool NoDelays = (OpCode & 0x20) != 0;

                    if (NoDelays)
                    {
                        FetchOpCode(Memory);
                    }

                    return true;
                }
            }

            bool Exit = (OpCode & 0x80) != 0;

            return !Exit;
        }

        private void FetchOpCode(AMemory Memory)
        {
            OpCode = PipeOp;

            PipeOp = Memory.ReadInt32(Pc);

            Pc += 4;
        }

        private int GetAluResult()
        {
            AluOperation Op = (AluOperation)(OpCode & 7);

            switch (Op)
            {
                case AluOperation.AluReg:
                {
                    AluRegOperation AluOp = (AluRegOperation)((OpCode >> 17) & 0x1f);

                    return GetAluResult(AluOp, GetGprA(), GetGprB());
                }

                case AluOperation.AddImmediate:
                {
                    return GetGprA() + GetImm();
                }

                case AluOperation.BitfieldReplace:
                case AluOperation.BitfieldExtractLslImm:
                case AluOperation.BitfieldExtractLslReg:
                {
                    int BfSrcBit = (OpCode >> 17) & 0x1f;
                    int BfSize   = (OpCode >> 22) & 0x1f;
                    int BfDstBit = (OpCode >> 27) & 0x1f;

                    int BfMask = (1 << BfSize) - 1;

                    int Dst = GetGprA();
                    int Src = GetGprB();

                    switch (Op)
                    {
                        case AluOperation.BitfieldReplace:
                        {
                            Src = (int)((uint)Src >> BfSrcBit) & BfMask;

                            Dst &= ~(BfMask << BfDstBit);

                            Dst |= Src << BfDstBit;

                            return Dst;
                        }

                        case AluOperation.BitfieldExtractLslImm:
                        {
                            Src = (int)((uint)Src >> Dst) & BfMask;

                            return Src << BfDstBit;
                        }

                        case AluOperation.BitfieldExtractLslReg:
                        {
                            Src = (int)((uint)Src >> BfSrcBit) & BfMask;

                            return Src << Dst;
                        }
                    }

                    break;
                }

                case AluOperation.ReadImmediate:
                {
                    return Read(GetGprA() + GetImm());
                }
            }

            throw new ArgumentException(nameof(OpCode));
        }

        private int GetAluResult(AluRegOperation AluOp, int A, int B)
        {
            switch (AluOp)
            {
                case AluRegOperation.Add:
                {
                    ulong Result = (ulong)A + (ulong)B;

                    Carry = Result > 0xffffffff;

                    return (int)Result;
                }

                case AluRegOperation.AddWithCarry:
                {
                    ulong Result = (ulong)A + (ulong)B + (Carry ? 1UL : 0UL);

                    Carry = Result > 0xffffffff;

                    return (int)Result;
                }

                case AluRegOperation.Subtract:
                {
                    ulong Result = (ulong)A - (ulong)B;

                    Carry = Result < 0x100000000;

                    return (int)Result;
                }

                case AluRegOperation.SubtractWithBorrow:
                {
                    ulong Result = (ulong)A - (ulong)B - (Carry ? 0UL : 1UL);

                    Carry = Result < 0x100000000;

                    return (int)Result;
                }

                case AluRegOperation.BitwiseExclusiveOr: return   A ^  B;
                case AluRegOperation.BitwiseOr:          return   A |  B;
                case AluRegOperation.BitwiseAnd:         return   A &  B;
                case AluRegOperation.BitwiseAndNot:      return   A & ~B;
                case AluRegOperation.BitwiseNotAnd:      return ~(A &  B);
            }

            throw new ArgumentOutOfRangeException(nameof(AluOp));
        }

        private int GetImm()
        {
            //Note: The immediate is signed, the sign-extension is intended here.
            return OpCode >> 14;
        }

        private void SetMethAddr(int Value)
        {
            MethAddr = (Value >>  0) & 0xfff;
            MethIncr = (Value >> 12) & 0x3f;
        }

        private void SetDstGpr(int Value)
        {
            Gprs[(OpCode >> 8) & 7] = Value;
        }

        private int GetGprA()
        {
            return GetGprValue((OpCode >> 11) & 7);
        }

        private int GetGprB()
        {
            return GetGprValue((OpCode >> 14) & 7);
        }

        private int GetGprValue(int Index)
        {
            return Index != 0 ? Gprs[Index] : 0;
        }

        private int FetchParam()
        {
            int Value;

            //If we don't have any parameters in the FIFO,
            //keep running the PFIFO engine until it writes the parameters.
            while (!Fifo.TryDequeue(out Value))
            {
                if (!PFifo.Step())
                {
                    return 0;
                }
            }

            return Value;
        }

        private int Read(int Reg)
        {
            return Engine.Registers[Reg];
        }

        private void Send(AMemory Memory, int Value)
        {
            NsGpuPBEntry PBEntry = new NsGpuPBEntry(MethAddr, 0, Value);

            Engine.CallMethod(Memory, PBEntry);

            MethAddr += MethIncr;
        }
    }
}