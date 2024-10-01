using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Fatal.Types
{
    public struct CpuContext32
    {
        public Array11<uint> X;
        public uint FP;
        public uint IP;
        public uint SP;
        public uint LR;
        public uint PC;

        public uint PState;
        public uint Afsr0;
        public uint Afsr1;
        public uint Esr;
        public uint Far;

        public Array32<uint> StackTrace;
        public uint StackTraceSize;
        public uint StartAddress;
        public uint RegisterSetFlags;
    }
}
