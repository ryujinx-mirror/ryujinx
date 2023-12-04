using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.Error
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErrorCommonArg
    {
        public uint Module;
        public uint Description;
        public uint ResultCode;
    }
}
