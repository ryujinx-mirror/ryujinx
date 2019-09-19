using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x2C)]
    struct TzifHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Magic;

        public char Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] TtisGMTCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] TtisSTDCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] LeapCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] TimeCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] TypeCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CharCount;
    }
}