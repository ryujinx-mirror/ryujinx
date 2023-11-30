using System;
using System.Threading;

namespace Ryujinx.Common.PreciseSleep
{
    /// <summary>
    /// A cross-platform precise sleep event that has millisecond granularity.
    /// </summary>
    internal class SleepEvent : IPreciseSleepEvent
    {
        private readonly AutoResetEvent _waitEvent = new(false);

        public long AdjustTimePoint(long timePoint, long timeoutNs)
        {
            // No adjustment
            return timePoint;
        }

        public bool SleepUntil(long timePoint)
        {
            long now = PerformanceCounter.ElapsedTicks;
            long ms = Math.Min((timePoint - now) / PerformanceCounter.TicksPerMillisecond, int.MaxValue);

            if (ms > 0)
            {
                _waitEvent.WaitOne((int)ms);

                return true;
            }

            return false;
        }

        public void Sleep()
        {
            _waitEvent.WaitOne();
        }

        public void Signal()
        {
            _waitEvent.Set();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _waitEvent.Dispose();
        }
    }
}
