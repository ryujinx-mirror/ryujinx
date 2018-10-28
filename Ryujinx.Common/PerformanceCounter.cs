using System.Diagnostics;

namespace Ryujinx.Common
{
    public static class PerformanceCounter
    {
        /// <summary>
        /// Represents the number of ticks in 1 day.
        /// </summary>
        public static long TicksPerDay { get; }

        /// <summary>
        /// Represents the number of ticks in 1 hour.
        /// </summary>
        public static long TicksPerHour { get; }

        /// <summary>
        /// Represents the number of ticks in 1 minute.
        /// </summary>
        public static long TicksPerMinute { get; }

        /// <summary>
        /// Represents the number of ticks in 1 second.
        /// </summary>
        public static long TicksPerSecond { get; }

        /// <summary>
        /// Represents the number of ticks in 1 millisecond.
        /// </summary>
        public static long TicksPerMillisecond { get; }

        /// <summary>
        /// Gets the number of milliseconds elapsed since the system started.
        /// </summary>
        public static long ElapsedTicks
        {
            get
            {
                return Stopwatch.GetTimestamp();
            }
        }

        /// <summary>
        /// Gets the number of milliseconds elapsed since the system started.
        /// </summary>
        public static long ElapsedMilliseconds
        {
            get
            {
                long timestamp = Stopwatch.GetTimestamp();

                return timestamp / TicksPerMillisecond;
            }
        }

        static PerformanceCounter()
        {
            TicksPerMillisecond = Stopwatch.Frequency / 1000;
            TicksPerSecond      = Stopwatch.Frequency;
            TicksPerMinute      = TicksPerSecond * 60;
            TicksPerHour        = TicksPerMinute * 60;
            TicksPerDay         = TicksPerHour * 24;
        }
    }
}
