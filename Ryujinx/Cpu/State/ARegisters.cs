using System;

namespace ChocolArm64.State
{
    public class ARegisters
    {
        internal const int LRIndex = 30;
        internal const int ZRIndex = 31;

        internal const int ErgSizeLog2 = 4;
        internal const int DczSizeLog2 = 4;

        private const long TicksPerS  = 19_200_000;
        private const long TicksPerMS = TicksPerS / 1_000;

        public ulong X0,  X1,  X2,  X3,  X4,  X5,  X6,  X7,
                     X8,  X9,  X10, X11, X12, X13, X14, X15,
                     X16, X17, X18, X19, X20, X21, X22, X23,
                     X24, X25, X26, X27, X28, X29, X30, X31;

        public AVec V0,  V1,  V2,  V3,  V4,  V5,  V6,  V7,
                    V8,  V9,  V10, V11, V12, V13, V14, V15,
                    V16, V17, V18, V19, V20, V21, V22, V23,
                    V24, V25, V26, V27, V28, V29, V30, V31;

        public bool Overflow;
        public bool Carry;
        public bool Zero;
        public bool Negative;

        public int ProcessId;
        public int ThreadId;

        public long TpidrEl0 { get; set; }
        public long Tpidr    { get; set; }

        public int Fpcr { get; set; }
        public int Fpsr { get; set; }

        public uint CtrEl0   => 0x8444c004;
        public uint DczidEl0 => 0x00000004;

        public long CntpctEl0 => Environment.TickCount * TicksPerMS;

        public event EventHandler<SvcEventArgs> SvcCall;
        public event EventHandler<EventArgs>    Undefined;

        public void OnSvcCall(int Imm)
        {
            SvcCall?.Invoke(this, new SvcEventArgs(Imm));
        }

        public void OnUndefined()
        {
            Undefined?.Invoke(this, EventArgs.Empty);
        }
    }
}