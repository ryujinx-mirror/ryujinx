using Ryujinx.HLE.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    [StructLayout(LayoutKind.Sequential)]
    struct TimeSpanType
    {
        public long NanoSeconds;

        public TimeSpanType(long nanoSeconds)
        {
            NanoSeconds = nanoSeconds;
        }

        public long ToSeconds()
        {
            return NanoSeconds / 1000000000;
        }

        public static TimeSpanType FromTicks(ulong ticks, ulong frequency)
        {
            return new TimeSpanType((long)ticks * 1000000000 / (long)frequency);
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
}
