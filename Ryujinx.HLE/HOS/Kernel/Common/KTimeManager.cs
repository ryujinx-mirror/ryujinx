using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    class KTimeManager : IDisposable
    {
        private class WaitingObject
        {
            public IKFutureSchedulerObject Object { get; }
            public long TimePoint { get; }

            public WaitingObject(IKFutureSchedulerObject schedulerObj, long timePoint)
            {
                Object    = schedulerObj;
                TimePoint = timePoint;
            }
        }

        private readonly KernelContext _context;
        private readonly List<WaitingObject> _waitingObjects;
        private AutoResetEvent _waitEvent;
        private bool _keepRunning;

        public KTimeManager(KernelContext context)
        {
            _context = context;
            _waitingObjects = new List<WaitingObject>();
            _keepRunning = true;

            Thread work = new Thread(WaitAndCheckScheduledObjects)
            {
                Name = "HLE.TimeManager"
            };

            work.Start();
        }

        public void ScheduleFutureInvocation(IKFutureSchedulerObject schedulerObj, long timeout)
        {
            long timePoint = PerformanceCounter.ElapsedMilliseconds + ConvertNanosecondsToMilliseconds(timeout);

            lock (_context.CriticalSection.Lock)
            {
                _waitingObjects.Add(new WaitingObject(schedulerObj, timePoint));
            }

            _waitEvent.Set();
        }

        public void UnscheduleFutureInvocation(IKFutureSchedulerObject schedulerObj)
        {
            lock (_context.CriticalSection.Lock)
            {
                _waitingObjects.RemoveAll(x => x.Object == schedulerObj);
            }
        }

        private void WaitAndCheckScheduledObjects()
        {
            using (_waitEvent = new AutoResetEvent(false))
            {
                while (_keepRunning)
                {
                    WaitingObject next;

                    lock (_context.CriticalSection.Lock)
                    {
                        next = _waitingObjects.OrderBy(x => x.TimePoint).FirstOrDefault();
                    }

                    if (next != null)
                    {
                        long timePoint = PerformanceCounter.ElapsedMilliseconds;

                        if (next.TimePoint > timePoint)
                        {
                            _waitEvent.WaitOne((int)(next.TimePoint - timePoint));
                        }

                        bool timeUp = PerformanceCounter.ElapsedMilliseconds >= next.TimePoint;

                        if (timeUp)
                        {
                            lock (_context.CriticalSection.Lock)
                            {
                                if (_waitingObjects.Remove(next))
                                {
                                    next.Object.TimeUp();
                                }
                            }
                        }
                    }
                    else
                    {
                        _waitEvent.WaitOne();
                    }
                }
            }
        }

        public static long ConvertNanosecondsToMilliseconds(long time)
        {
            time /= 1000000;

            if ((ulong)time > int.MaxValue)
            {
                return int.MaxValue;
            }

            return time;
        }

        public static long ConvertMillisecondsToNanoseconds(long time)
        {
            return time * 1000000;
        }

        public static long ConvertHostTicksToTicks(long time)
        {
            return (long)((time / (double)PerformanceCounter.TicksPerSecond) * 19200000.0);
        }

        public void Dispose()
        {
            _keepRunning = false;
            _waitEvent?.Set();
        }
    }
}