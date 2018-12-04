using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
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

        private List<WaitingObject> _waitingObjects;

        private AutoResetEvent _waitEvent;

        private bool _keepRunning;

        public KTimeManager()
        {
            _waitingObjects = new List<WaitingObject>();

            _keepRunning = true;

            Thread work = new Thread(WaitAndCheckScheduledObjects);

            work.Start();
        }

        public void ScheduleFutureInvocation(IKFutureSchedulerObject schedulerObj, long timeout)
        {
            long timePoint = PerformanceCounter.ElapsedMilliseconds + ConvertNanosecondsToMilliseconds(timeout);

            lock (_waitingObjects)
            {
                _waitingObjects.Add(new WaitingObject(schedulerObj, timePoint));
            }

            _waitEvent.Set();
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

        public static long ConvertMillisecondsToTicks(long time)
        {
            return time * 19200;
        }

        public void UnscheduleFutureInvocation(IKFutureSchedulerObject Object)
        {
            lock (_waitingObjects)
            {
                _waitingObjects.RemoveAll(x => x.Object == Object);
            }
        }

        private void WaitAndCheckScheduledObjects()
        {
            using (_waitEvent = new AutoResetEvent(false))
            {
                while (_keepRunning)
                {
                    WaitingObject next;

                    lock (_waitingObjects)
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
                            lock (_waitingObjects)
                            {
                                timeUp = _waitingObjects.Remove(next);
                            }
                        }

                        if (timeUp)
                        {
                            next.Object.TimeUp();
                        }
                    }
                    else
                    {
                        _waitEvent.WaitOne();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keepRunning = false;

                _waitEvent?.Set();
            }
        }
    }
}