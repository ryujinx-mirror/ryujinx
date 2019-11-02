using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct EventWaitArguments
    {
        public int Id;
        public int Thresh;
        public int Timeout;
        public int Value;
    }
}
