using ChocolArm64.Translation;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using static ChocolArm64.Instructions.VectorHelper;

namespace ChocolArm64.State
{
    public class CpuThreadState : ARMeilleure.State.IExecutionContext
    {
        private const int MinCountForCheck = 40000;

        internal const int ErgSizeLog2 = 4;
        internal const int DczSizeLog2 = 4;

        public ulong X0,  X1,  X2,  X3,  X4,  X5,  X6,  X7,
                     X8,  X9,  X10, X11, X12, X13, X14, X15,
                     X16, X17, X18, X19, X20, X21, X22, X23,
                     X24, X25, X26, X27, X28, X29, X30, X31;

        public Vector128<float> V0,  V1,  V2,  V3,  V4,  V5,  V6,  V7,
                                V8,  V9,  V10, V11, V12, V13, V14, V15,
                                V16, V17, V18, V19, V20, V21, V22, V23,
                                V24, V25, V26, V27, V28, V29, V30, V31;

        public bool IsAarch32 { get; set; }

        public bool Thumb;
        public bool BigEndian;

        public bool Overflow;
        public bool Carry;
        public bool Zero;
        public bool Negative;

        public int ElrHyp;

        public bool Running { get; set; }

        private bool _interrupted;

        private int _syncCount;

        public long TpidrEl0 { get; set; }
        public long Tpidr    { get; set; }

        public int CFpcr { get; set; }
        public int CFpsr { get; set; }

        public ARMeilleure.State.FPCR Fpcr
        {
            get => (ARMeilleure.State.FPCR)CFpcr;
            set => CFpcr = (int)value;
        }

        public ARMeilleure.State.FPSR Fpsr
        {
            get => (ARMeilleure.State.FPSR)CFpsr;
            set => CFpsr = (int)value;
        }

        public int Psr
        {
            get
            {
                return (Negative ? (int)PState.NMask : 0) |
                       (Zero     ? (int)PState.ZMask : 0) |
                       (Carry    ? (int)PState.CMask : 0) |
                       (Overflow ? (int)PState.VMask : 0);
            }
        }

        public uint CtrEl0   => 0x8444c004;
        public uint DczidEl0 => 0x00000004;

        public ulong CntfrqEl0 { get; set; }
        public ulong CntpctEl0
        {
            get
            {
                double ticks = _tickCounter.ElapsedTicks * _hostTickFreq;

                return (ulong)(ticks * CntfrqEl0);
            }
        }

        public event EventHandler<EventArgs>                                Interrupt;
        public event EventHandler<ARMeilleure.State.InstExceptionEventArgs> Break;
        public event EventHandler<ARMeilleure.State.InstExceptionEventArgs> SupervisorCall;
        public event EventHandler<ARMeilleure.State.InstUndefinedEventArgs> Undefined;

        private static Stopwatch _tickCounter;

        private static double _hostTickFreq;

        internal Translator CurrentTranslator;

        private ulong _exclusiveAddress;

        internal ulong ExclusiveValueLow  { get; set; }
        internal ulong ExclusiveValueHigh { get; set; }

        public CpuThreadState()
        {
            ClearExclusiveAddress();

            Running = true;
        }

        static CpuThreadState()
        {
            _hostTickFreq = 1.0 / Stopwatch.Frequency;

            _tickCounter = new Stopwatch();

            _tickCounter.Start();
        }

        internal void SetExclusiveAddress(ulong address)
        {
            _exclusiveAddress = GetMaskedExclusiveAddress(address);
        }

        internal bool CheckExclusiveAddress(ulong address)
        {
            return GetMaskedExclusiveAddress(address) == _exclusiveAddress;
        }

        internal void ClearExclusiveAddress()
        {
            _exclusiveAddress = ulong.MaxValue;
        }

        private ulong GetMaskedExclusiveAddress(ulong address)
        {
            return address & ~((4UL << ErgSizeLog2) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Synchronize()
        {
            // Firing a interrupt frequently is expensive, so we only
            // do it after a given number of instructions has executed.
            _syncCount++;

            if (_syncCount >= MinCountForCheck)
            {
                CheckInterrupt();
            }

            return Running;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckInterrupt()
        {
            _syncCount = 0;

            if (_interrupted)
            {
                _interrupted = false;

                Interrupt?.Invoke(this, EventArgs.Empty);
            }
        }

        public ulong GetX(int index)
        {
            switch (index)
            {
                case 0:  return X0;
                case 1:  return X1;
                case 2:  return X2;
                case 3:  return X3;
                case 4:  return X4;
                case 5:  return X5;
                case 6:  return X6;
                case 7:  return X7;
                case 8:  return X8;
                case 9:  return X9;
                case 10: return X10;
                case 11: return X11;
                case 12: return X12;
                case 13: return X13;
                case 14: return X14;
                case 15: return X15;
                case 16: return X16;
                case 17: return X17;
                case 18: return X18;
                case 19: return X19;
                case 20: return X20;
                case 21: return X21;
                case 22: return X22;
                case 23: return X23;
                case 24: return X24;
                case 25: return X25;
                case 26: return X26;
                case 27: return X27;
                case 28: return X28;
                case 29: return X29;
                case 30: return X30;
                case 31: return X31;

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void SetX(int index, ulong value)
        {
            switch (index)
            {
                case 0:  X0  = value; break;
                case 1:  X1  = value; break;
                case 2:  X2  = value; break;
                case 3:  X3  = value; break;
                case 4:  X4  = value; break;
                case 5:  X5  = value; break;
                case 6:  X6  = value; break;
                case 7:  X7  = value; break;
                case 8:  X8  = value; break;
                case 9:  X9  = value; break;
                case 10: X10 = value; break;
                case 11: X11 = value; break;
                case 12: X12 = value; break;
                case 13: X13 = value; break;
                case 14: X14 = value; break;
                case 15: X15 = value; break;
                case 16: X16 = value; break;
                case 17: X17 = value; break;
                case 18: X18 = value; break;
                case 19: X19 = value; break;
                case 20: X20 = value; break;
                case 21: X21 = value; break;
                case 22: X22 = value; break;
                case 23: X23 = value; break;
                case 24: X24 = value; break;
                case 25: X25 = value; break;
                case 26: X26 = value; break;
                case 27: X27 = value; break;
                case 28: X28 = value; break;
                case 29: X29 = value; break;
                case 30: X30 = value; break;
                case 31: X31 = value; break;

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public ARMeilleure.State.V128 GetV(int index)
        {
            switch (index)
            {
                case 0:  return new ARMeilleure.State.V128(VectorExtractIntZx(V0,  0, 3), VectorExtractIntZx(V0,  1, 3));
                case 1:  return new ARMeilleure.State.V128(VectorExtractIntZx(V1,  0, 3), VectorExtractIntZx(V1,  1, 3));
                case 2:  return new ARMeilleure.State.V128(VectorExtractIntZx(V2,  0, 3), VectorExtractIntZx(V2,  1, 3));
                case 3:  return new ARMeilleure.State.V128(VectorExtractIntZx(V3,  0, 3), VectorExtractIntZx(V3,  1, 3));
                case 4:  return new ARMeilleure.State.V128(VectorExtractIntZx(V4,  0, 3), VectorExtractIntZx(V4,  1, 3));
                case 5:  return new ARMeilleure.State.V128(VectorExtractIntZx(V5,  0, 3), VectorExtractIntZx(V5,  1, 3));
                case 6:  return new ARMeilleure.State.V128(VectorExtractIntZx(V6,  0, 3), VectorExtractIntZx(V6,  1, 3));
                case 7:  return new ARMeilleure.State.V128(VectorExtractIntZx(V7,  0, 3), VectorExtractIntZx(V7,  1, 3));
                case 8:  return new ARMeilleure.State.V128(VectorExtractIntZx(V8,  0, 3), VectorExtractIntZx(V8,  1, 3));
                case 9:  return new ARMeilleure.State.V128(VectorExtractIntZx(V9,  0, 3), VectorExtractIntZx(V9,  1, 3));
                case 10: return new ARMeilleure.State.V128(VectorExtractIntZx(V10, 0, 3), VectorExtractIntZx(V10, 1, 3));
                case 11: return new ARMeilleure.State.V128(VectorExtractIntZx(V11, 0, 3), VectorExtractIntZx(V11, 1, 3));
                case 12: return new ARMeilleure.State.V128(VectorExtractIntZx(V12, 0, 3), VectorExtractIntZx(V12, 1, 3));
                case 13: return new ARMeilleure.State.V128(VectorExtractIntZx(V13, 0, 3), VectorExtractIntZx(V13, 1, 3));
                case 14: return new ARMeilleure.State.V128(VectorExtractIntZx(V14, 0, 3), VectorExtractIntZx(V14, 1, 3));
                case 15: return new ARMeilleure.State.V128(VectorExtractIntZx(V15, 0, 3), VectorExtractIntZx(V15, 1, 3));
                case 16: return new ARMeilleure.State.V128(VectorExtractIntZx(V16, 0, 3), VectorExtractIntZx(V16, 1, 3));
                case 17: return new ARMeilleure.State.V128(VectorExtractIntZx(V17, 0, 3), VectorExtractIntZx(V17, 1, 3));
                case 18: return new ARMeilleure.State.V128(VectorExtractIntZx(V18, 0, 3), VectorExtractIntZx(V18, 1, 3));
                case 19: return new ARMeilleure.State.V128(VectorExtractIntZx(V19, 0, 3), VectorExtractIntZx(V19, 1, 3));
                case 20: return new ARMeilleure.State.V128(VectorExtractIntZx(V20, 0, 3), VectorExtractIntZx(V20, 1, 3));
                case 21: return new ARMeilleure.State.V128(VectorExtractIntZx(V21, 0, 3), VectorExtractIntZx(V21, 1, 3));
                case 22: return new ARMeilleure.State.V128(VectorExtractIntZx(V22, 0, 3), VectorExtractIntZx(V22, 1, 3));
                case 23: return new ARMeilleure.State.V128(VectorExtractIntZx(V23, 0, 3), VectorExtractIntZx(V23, 1, 3));
                case 24: return new ARMeilleure.State.V128(VectorExtractIntZx(V24, 0, 3), VectorExtractIntZx(V24, 1, 3));
                case 25: return new ARMeilleure.State.V128(VectorExtractIntZx(V25, 0, 3), VectorExtractIntZx(V25, 1, 3));
                case 26: return new ARMeilleure.State.V128(VectorExtractIntZx(V26, 0, 3), VectorExtractIntZx(V26, 1, 3));
                case 27: return new ARMeilleure.State.V128(VectorExtractIntZx(V27, 0, 3), VectorExtractIntZx(V27, 1, 3));
                case 28: return new ARMeilleure.State.V128(VectorExtractIntZx(V28, 0, 3), VectorExtractIntZx(V28, 1, 3));
                case 29: return new ARMeilleure.State.V128(VectorExtractIntZx(V29, 0, 3), VectorExtractIntZx(V29, 1, 3));
                case 30: return new ARMeilleure.State.V128(VectorExtractIntZx(V30, 0, 3), VectorExtractIntZx(V30, 1, 3));
                case 31: return new ARMeilleure.State.V128(VectorExtractIntZx(V31, 0, 3), VectorExtractIntZx(V31, 1, 3));

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public bool GetPstateFlag(ARMeilleure.State.PState flag)
        {
            switch (flag)
            {
                case ARMeilleure.State.PState.NFlag: return Negative;
                case ARMeilleure.State.PState.ZFlag: return Zero;
                case ARMeilleure.State.PState.CFlag: return Carry;
                case ARMeilleure.State.PState.VFlag: return Overflow;

                default: throw new ArgumentOutOfRangeException(nameof(flag));
            }
        }

        public void RequestInterrupt()
        {
            _interrupted = true;
        }

        internal void OnBreak(long position, int imm)
        {
            Break?.Invoke(this, new ARMeilleure.State.InstExceptionEventArgs((ulong)position, imm));
        }

        internal void OnSvcCall(long position, int imm)
        {
            SupervisorCall?.Invoke(this, new ARMeilleure.State.InstExceptionEventArgs((ulong)position, imm));
        }

        internal void OnUndefined(long position, int rawOpCode)
        {
            Undefined?.Invoke(this, new ARMeilleure.State.InstUndefinedEventArgs((ulong)position, rawOpCode));
        }

        internal ExecutionMode GetExecutionMode()
        {
            if (!IsAarch32)
            {
                return ExecutionMode.Aarch64;
            }
            else
            {
                return Thumb ? ExecutionMode.Aarch32Thumb : ExecutionMode.Aarch32Arm;
            }
        }

        internal bool GetFpcrFlag(Fpcr flag)
        {
            return (CFpcr & (1 << (int)flag)) != 0;
        }

        internal void SetFpsrFlag(Fpsr flag)
        {
            CFpsr |= 1 << (int)flag;
        }

        internal RoundMode FPRoundingMode()
        {
            return (RoundMode)((CFpcr >> (int)State.Fpcr.RMode) & 3);
        }

        public void Dispose() { }
    }
}
