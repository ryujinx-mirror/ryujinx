using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 4)]
    struct NvHostChannelCmdBuf
    {
        public int MemoryId;
        public int Offset;
        public int WordsCount;
    }
}