using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    [StructLayout(LayoutKind.Sequential)]
    struct TimeSpanType
    {
        private const long NanoSecondsPerSecond = 1000000000;

        public static readonly TimeSpanType Zero = new(0);

        public long NanoSeconds;

        public TimeSpanType(long nanoSeconds)
        {
            NanoSeconds = nanoSeconds;
        }

        public readonly long ToSeconds()
        {
            return NanoSeconds / NanoSecondsPerSecond;
        }

        public readonly TimeSpanType AddSeconds(long seconds)
        {
            return new TimeSpanType(NanoSeconds + (seconds * NanoSecondsPerSecond));
        }

        public readonly bool IsDaylightSavingTime()
        {
            return DateTime.UnixEpoch.AddSeconds(ToSeconds()).ToLocalTime().IsDaylightSavingTime();
        }

        public static TimeSpanType FromSeconds(long seconds)
        {
            return new TimeSpanType(seconds * NanoSecondsPerSecond);
        }

        public static TimeSpanType FromTimeSpan(TimeSpan timeSpan)
        {
            return new TimeSpanType((long)(timeSpan.TotalMilliseconds * 1000000));
        }

        public static TimeSpanType FromTicks(ulong ticks, ulong frequency)
        {
            return FromSeconds((long)ticks / (long)frequency);
        }
    }
}
