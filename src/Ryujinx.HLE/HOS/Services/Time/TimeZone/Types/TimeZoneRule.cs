using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x4000, CharSet = CharSet.Ansi)]
    public struct TimeZoneRule
    {
        public const int TzMaxTypes = 128;
        public const int TzMaxChars = 50;
        public const int TzMaxLeaps = 50;
        public const int TzMaxTimes = 1000;
        public const int TzNameMax = 255;
        public const int TzCharsArraySize = 2 * (TzNameMax + 1);

        public int TimeCount;
        public int TypeCount;
        public int CharCount;

        [MarshalAs(UnmanagedType.I1)]
        public bool GoBack;

        [MarshalAs(UnmanagedType.I1)]
        public bool GoAhead;

        [StructLayout(LayoutKind.Sequential, Size = sizeof(long) * TzMaxTimes)]
        private struct AtsStorageStruct { }

        private AtsStorageStruct _ats;

        public Span<long> Ats => SpanHelpers.AsSpan<AtsStorageStruct, long>(ref _ats);

        [StructLayout(LayoutKind.Sequential, Size = sizeof(byte) * TzMaxTimes)]
        private struct TypesStorageStruct { }

        private TypesStorageStruct _types;

        public Span<byte> Types => SpanHelpers.AsByteSpan(ref _types);

        [StructLayout(LayoutKind.Sequential, Size = TimeTypeInfo.Size * TzMaxTypes)]
        private struct TimeTypeInfoStorageStruct { }

        private TimeTypeInfoStorageStruct _ttis;

        public Span<TimeTypeInfo> Ttis => SpanHelpers.AsSpan<TimeTypeInfoStorageStruct, TimeTypeInfo>(ref _ttis);

        [StructLayout(LayoutKind.Sequential, Size = sizeof(byte) * TzCharsArraySize)]
        private struct CharsStorageStruct { }

        private CharsStorageStruct _chars;
        public Span<byte> Chars => SpanHelpers.AsByteSpan(ref _chars);

        public int DefaultType;
    }
}
