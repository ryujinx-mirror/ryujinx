using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvHostChannel
{
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 4)]
    struct NvHostChannelSubmit
    {
        public int CmdBufsCount;
        public int RelocsCount;
        public int SyncptIncrsCount;
        public int WaitchecksCount;
    }
}