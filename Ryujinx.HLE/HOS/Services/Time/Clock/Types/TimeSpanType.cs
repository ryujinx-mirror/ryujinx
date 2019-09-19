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
}