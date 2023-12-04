using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct AllocObjCtxArguments
    {
        public uint ClassNumber;
        public uint Flags;
        public ulong ObjectId;
    }
}
