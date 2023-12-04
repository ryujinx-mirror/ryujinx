using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct ZcullGetCtxSizeArguments
    {
        public int Size;
    }
}
