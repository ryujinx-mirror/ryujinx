using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 4)]
    struct TimeTypeInfo
    {
        public int GmtOffset;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsDaySavingTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        char[] Padding1;

        public int AbbreviationListIndex;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsStandardTimeDaylight;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsGMT;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        char[] Padding2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x4000, CharSet = CharSet.Ansi)]
    struct TimeZoneRule
    {
        public const int TzMaxTypes        = 128;
        public const int TzMaxChars        = 50;
        public const int TzMaxLeaps        = 50;
        public const int TzMaxTimes        = 1000;
        public const int TzNameMax         = 255;
        public const int TzCharsArraySize  = 2 * (TzNameMax + 1);

        public int TimeCount;
        public int TypeCount;
        public int CharCount;

        [MarshalAs(UnmanagedType.I1)]
        public bool GoBack;

        [MarshalAs(UnmanagedType.I1)]
        public bool GoAhead;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TzMaxTimes)]
        public long[] Ats;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TzMaxTimes)]
        public byte[] Types;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TzMaxTypes)]
        public TimeTypeInfo[] Ttis;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TzCharsArraySize)]
        public char[] Chars;

        public int DefaultType;
    }

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

    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x8)]
    struct CalendarTime
    {
        public short Year;
        public sbyte Month;
        public sbyte Day;
        public sbyte Hour;
        public sbyte Minute;
        public sbyte Second;
    }

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
        char[] Padding;

        public int GmtOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x20, CharSet = CharSet.Ansi)]
    struct CalendarInfo
    {
        public CalendarTime           Time;
        public CalendarAdditionalInfo AdditionalInfo;
    }
}