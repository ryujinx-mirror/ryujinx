using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    [StructLayout(LayoutKind.Sequential, Size = 0xD0)]
    struct ClockSnapshot
    {
        public SystemClockContext     UserContext;
        public SystemClockContext     NetworkContext;
        public long                   UserTime;
        public long                   NetworkTime;
        public CalendarTime           UserCalendarTime;
        public CalendarTime           NetworkCalendarTime;
        public CalendarAdditionalInfo UserCalendarAdditionalTime;
        public CalendarAdditionalInfo NetworkCalendarAdditionalTime;
        public SteadyClockTimePoint   SteadyClockTimePoint;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24)]
        public char[] LocationName;

        [MarshalAs(UnmanagedType.I1)]
        public bool   IsAutomaticCorrectionEnabled;
        public byte   Type;
        public ushort Unknown;

        public static ResultCode GetCurrentTime(out long currentTime, SteadyClockTimePoint steadyClockTimePoint, SystemClockContext context)
        {
            currentTime = 0;

            if (steadyClockTimePoint.ClockSourceId == context.SteadyTimePoint.ClockSourceId)
            {
                currentTime = steadyClockTimePoint.TimePoint + context.Offset;

                return ResultCode.Success;
            }

            return ResultCode.TimeMismatch;
        }
    }
}