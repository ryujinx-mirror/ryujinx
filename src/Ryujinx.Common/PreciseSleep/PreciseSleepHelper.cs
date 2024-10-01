using Ryujinx.Common.SystemInterop;
using System;
using System.Threading;

namespace Ryujinx.Common.PreciseSleep
{
    public static class PreciseSleepHelper
    {
        /// <summary>
        /// Create a precise sleep event for the current platform.
        /// </summary>
        /// <returns>A precise sleep event</returns>
        public static IPreciseSleepEvent CreateEvent()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsAndroid())
            {
                return new NanosleepEvent();
            }
            else if (OperatingSystem.IsWindows())
            {
                return new WindowsSleepEvent();
            }
            else
            {
                return new SleepEvent();
            }
        }

        /// <summary>
        /// Sleeps up to the closest point to the timepoint that the OS reasonably allows.
        /// The provided event is used by the timer to wake the current thread, and should not be signalled from any other source.
        /// </summary>
        /// <param name="evt">Event used to wake this thread</param>
        /// <param name="timePoint">Target timepoint in host ticks</param>
        public static void SleepUntilTimePoint(EventWaitHandle evt, long timePoint)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsGranularTimer.Instance.SleepUntilTimePointWithoutExternalSignal(evt, timePoint);
            }
            else
            {
                // Events might oversleep by a little, depending on OS.
                // We don't want to miss the timepoint, so bias the wait to be lower.
                // Nanosleep can possibly handle it better, too.
                long accuracyBias = PerformanceCounter.TicksPerMillisecond / 2;
                long now = PerformanceCounter.ElapsedTicks + accuracyBias;
                long ms = Math.Min((timePoint - now) / PerformanceCounter.TicksPerMillisecond, int.MaxValue);

                if (ms > 0)
                {
                    evt.WaitOne((int)ms);
                }

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsAndroid())
                {
                    // Do a nanosleep.
                    now = PerformanceCounter.ElapsedTicks;
                    long ns = ((timePoint - now) * 1_000_000) / PerformanceCounter.TicksPerMillisecond;

                    Nanosleep.SleepAtMost(ns);
                }
            }
        }

        /// <summary>
        /// Spinwait until the given timepoint. If wakeSignal is or becomes 1, return early.
        /// Thread is allowed to yield.
        /// </summary>
        /// <param name="timePoint">Target timepoint in host ticks</param>
        /// <param name="wakeSignal">Returns early if this is set to 1</param>
        public static void SpinWaitUntilTimePoint(long timePoint, ref long wakeSignal)
        {
            SpinWait spinWait = new();

            while (Interlocked.Read(ref wakeSignal) != 1 && PerformanceCounter.ElapsedTicks < timePoint)
            {
                // Our time is close - don't let SpinWait go off and potentially Thread.Sleep().
                if (spinWait.NextSpinWillYield)
                {
                    Thread.Yield();

                    spinWait.Reset();
                }
                else
                {
                    spinWait.SpinOnce();
                }
            }
        }

        /// <summary>
        /// Spinwait until the given timepoint, with no opportunity to wake early.
        /// </summary>
        /// <param name="timePoint">Target timepoint in host ticks</param>
        public static void SpinWaitUntilTimePoint(long timePoint)
        {
            while (PerformanceCounter.ElapsedTicks < timePoint)
            {
                Thread.SpinWait(5);
            }
        }
    }
}
