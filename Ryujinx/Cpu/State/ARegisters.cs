using System;

namespace ChocolArm64.State
{
    public class ARegisters
    {
        internal const int LRIndex = 30;
        internal const int ZRIndex = 31;

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

        public int  ProcessId;
        public int  ThreadId;
        public long TlsAddrEl0;
        public long TlsAddr;

        private int FPCR;
        private int FPSR;

        public ACoreType CoreType;

        private const ulong A53DczidEl0 = 4;
        private const ulong A53CtrEl0  = 0x84448004;
        private const ulong A57CtrEl0  = 0x8444c004;

        private const ulong TicksPerS  = 19_200_000;
        private const ulong TicksPerMS = TicksPerS / 1_000;

        public event EventHandler<SvcEventArgs> SvcCall;
        public event EventHandler<EventArgs>    Undefined;

        public ulong GetSystemReg(int Op0, int Op1, int CRn, int CRm, int Op2)
        {
            switch (PackRegId(Op0, Op1, CRn, CRm, Op2))
            {
                case 0b11_011_0000_0000_001: return GetCtrEl0();
                case 0b11_011_0000_0000_111: return GetDczidEl0();
                case 0b11_011_0100_0100_000: return (ulong)PackFPCR();
                case 0b11_011_0100_0100_001: return (ulong)PackFPSR();
                case 0b11_011_1101_0000_010: return (ulong)TlsAddrEl0;
                case 0b11_011_1101_0000_011: return (ulong)TlsAddr;
                case 0b11_011_1110_0000_001: return (ulong)Environment.TickCount * TicksPerMS;

                default: throw new ArgumentException();
            }
        }

        public void SetSystemReg(int Op0, int Op1, int CRn, int CRm, int Op2, ulong Value)
        {
            switch (PackRegId(Op0, Op1, CRn, CRm, Op2))
            {
                case 0b11_011_0100_0100_000: UnpackFPCR((int)Value);   break;
                case 0b11_011_0100_0100_001: UnpackFPSR((int)Value);   break;
                case 0b11_011_1101_0000_010: TlsAddrEl0 = (long)Value; break;

                default: throw new ArgumentException();
            }
        }

        private int PackRegId(int Op0, int Op1, int CRn, int CRm, int Op2)
        {
            int Id;

            Id  = Op2 << 0;
            Id |= CRm << 3;
            Id |= CRn << 7;
            Id |= Op1 << 11;
            Id |= Op0 << 14;

            return Id;
        }

        public ulong GetCtrEl0()
        {
            return CoreType == ACoreType.CortexA53 ? A53CtrEl0 : A57CtrEl0;
        }

        public ulong GetDczidEl0()
        {
            return A53DczidEl0;
        }

        public int PackFPCR()
        {
            return FPCR; //TODO
        }

        public int PackFPSR()
        {
            return FPSR; //TODO
        }

        public void UnpackFPCR(int Value)
        {
            FPCR = Value;
        }

        public void UnpackFPSR(int Value)
        {
            FPSR = Value;
        }

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