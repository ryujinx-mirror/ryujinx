using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct FreeSpaceArguments
    {
        public ulong Offset;
        public uint  Pages;
        public uint  PageSize;
    }
}
