using Ryujinx.HLE.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    [StructLayout(LayoutKind.Sequential)]
    struct TimeSpanType
    {
        private const long NanoSecondsPerSecond = 1000000000;

        public long NanoSeconds;

        public TimeSpanType(long nanoSeconds)
        {
            NanoSeconds = nanoSeconds;
        }

        public long ToSeconds()
        {
            return NanoSeconds / NanoSecondsPerSecond;
        }

        public static TimeSpanType FromSeconds(long seconds)
        {
            return new TimeSpanType(seconds * NanoSecondsPerSecond);
        }

        public static TimeSpanType FromTicks(ulong ticks, ulong frequency)
        {
            return FromSeconds((long)ticks / (long)frequency);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SteadyClockTimePoint
    {
        public long    TimePoint;
        public UInt128 ClockSourceId;

        public ResultCode GetSpanBetween(SteadyClockTimePoint other, out long outSpan)
        {
            outSpan = 0;

            if (ClockSourceId == other.ClockSourceId)
            {
                try
                {
                    outSpan = checked(other.TimePoint - TimePoint);

                    return ResultCode.Success;
                }
                catch (OverflowException)
                {
                    return ResultCode.Overflow;
                }
            }

            return ResultCode.Overflow;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SystemClockContext
    {
        public long                 Offset;
        public SteadyClockTimePoint SteadyTimePoint;
    }

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
