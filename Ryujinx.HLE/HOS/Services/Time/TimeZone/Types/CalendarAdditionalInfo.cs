using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x18, CharSet = CharSet.Ansi)]
    struct CalendarAdditionalInfo
    {
        public uint DayOfWeek;
        public uint DayOfYear;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] TimezoneName;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsDaySavingTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public char[] Padding;

        public int GmtOffset;
    }
}