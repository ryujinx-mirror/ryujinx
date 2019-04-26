using ChocolArm64.Events;
using ChocolArm64.Translation;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace ChocolArm64.State
{
    public class CpuThreadState
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

        public bool Aarch32;

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

        public int Fpcr { get; set; }
        public int Fpsr { get; set; }

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

        public event EventHandler<EventArgs>              Interrupt;
        public event EventHandler<InstExceptionEventArgs> Break;
        public event EventHandler<InstExceptionEventArgs> SvcCall;
        public event EventHandler<InstUndefinedEventArgs> Undefined;

        private static Stopwatch _tickCounter;

        private static double _hostTickFreq;

        internal Translator CurrentTranslator;

        private ulong _exclusiveAddress;

        internal ulong ExclusiveValueLow  { get; set; }
        internal ulong ExclusiveValueHigh { get; set; }

        public CpuThreadState()
        {
            ClearExclusiveAddress();
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
            //Firing a interrupt frequently is expensive, so we only
            //do it after a given number of instructions has executed.
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

        internal void RequestInterrupt()
        {
            _interrupted = true;
        }

        internal void OnBreak(long position, int imm)
        {
            Break?.Invoke(this, new InstExceptionEventArgs(position, imm));
        }

        internal void OnSvcCall(long position, int imm)
        {
            SvcCall?.Invoke(this, new InstExceptionEventArgs(position, imm));
        }

        internal void OnUndefined(long position, int rawOpCode)
        {
            Undefined?.Invoke(this, new InstUndefinedEventArgs(position, rawOpCode));
        }

        internal ExecutionMode GetExecutionMode()
        {
            if (!Aarch32)
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
            return (Fpcr & (1 << (int)flag)) != 0;
        }

        internal void SetFpsrFlag(Fpsr flag)
        {
            Fpsr |= 1 << (int)flag;
        }

        internal RoundMode FPRoundingMode()
        {
            return (RoundMode)((Fpcr >> (int)State.Fpcr.RMode) & 3);
        }
    }
}
