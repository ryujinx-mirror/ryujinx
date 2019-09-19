using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 4)]
    struct TimeTypeInfo
    {
        public int GmtOffset;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsDaySavingTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public char[] Padding1;

        public int AbbreviationListIndex;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsStandardTimeDaylight;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsGMT;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public char[] Padding2;
    }
}