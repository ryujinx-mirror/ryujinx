using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct CommandBuffer
    {
        public int MemoryId;
        public int Offset;
        public int WordsCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SubmitArguments
    {
        public int CmdBufsCount;
        public int RelocsCount;
        public int SyncptIncrsCount;
        public int WaitchecksCount;
    }
}
