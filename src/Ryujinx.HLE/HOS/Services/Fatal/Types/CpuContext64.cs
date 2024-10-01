using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Fatal.Types
{
    public struct CpuContext64
    {
        public Array29<ulong> X;
        public ulong FP;
        public ulong LR;
        public ulong SP;
        public ulong PC;

        public ulong PState;
        public ulong Afsr0;
        public ulong Afsr1;
        public ulong Esr;
        public ulong Far;

        public Array32<ulong> StackTrace;
        public ulong StartAddress;
        public ulong RegisterSetFlags;
        public uint StackTraceSize;
    }
}
