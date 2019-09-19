using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
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
}