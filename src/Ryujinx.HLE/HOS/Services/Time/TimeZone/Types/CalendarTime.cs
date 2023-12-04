using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
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
}
