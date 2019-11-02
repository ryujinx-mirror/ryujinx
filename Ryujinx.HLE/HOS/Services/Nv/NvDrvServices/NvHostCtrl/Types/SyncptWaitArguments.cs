using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct SyncptWaitArguments
    {
        public uint Id;
        public int  Thresh;
        public int  Timeout;
    }
}
