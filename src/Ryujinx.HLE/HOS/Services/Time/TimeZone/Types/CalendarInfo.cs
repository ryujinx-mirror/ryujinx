using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x20, CharSet = CharSet.Ansi)]
    struct CalendarInfo
    {
        public CalendarTime Time;
        public CalendarAdditionalInfo AdditionalInfo;
    }
}
