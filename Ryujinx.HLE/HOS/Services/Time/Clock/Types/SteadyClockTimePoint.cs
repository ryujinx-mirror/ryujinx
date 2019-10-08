using Ryujinx.HLE.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
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

        public static SteadyClockTimePoint GetRandom()
        {
            return new SteadyClockTimePoint
            {
                TimePoint     = 0,
                ClockSourceId = new UInt128(Guid.NewGuid().ToByteArray())
            };
        }
    }
}