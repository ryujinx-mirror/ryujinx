using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel
{
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 4)]
    struct NvHostChannelGetParamArg
    {
        public int Param;
        public int Value;
    }
}