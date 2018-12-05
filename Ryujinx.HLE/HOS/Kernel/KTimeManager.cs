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
            public IKFutureSchedulerObject Object { get; private set; }

            public long TimePoint { get; private set; }

            public WaitingObject(IKFutureSchedulerObject Object, long TimePoint)
            {
                this.Object    = Object;
                this.TimePoint = TimePoint;
            }
        }

        private List<WaitingObject> WaitingObjects;

        private AutoResetEvent WaitEvent;

        private bool KeepRunning;

        public KTimeManager()
        {
            WaitingObjects = new List<WaitingObject>();

            KeepRunning = true;

            Thread Work = new Thread(WaitAndCheckScheduledObjects);

            Work.Start();
        }

        public void ScheduleFutureInvocation(IKFutureSchedulerObject Object, long Timeout)
        {
            long TimePoint = PerformanceCounter.ElapsedMilliseconds + ConvertNanosecondsToMilliseconds(Timeout);

            lock (WaitingObjects)
            {
                WaitingObjects.Add(new WaitingObject(Object, TimePoint));
            }

            WaitEvent.Set();
        }

        public static long ConvertNanosecondsToMilliseconds(long Time)
        {
            Time /= 1000000;

            if ((ulong)Time > int.MaxValue)
            {
                return int.MaxValue;
            }

            return Time;
        }

        public static long ConvertMillisecondsToNanoseconds(long Time)
        {
            return Time * 1000000;
        }

        public static long ConvertMillisecondsToTicks(long Time)
        {
            return Time * 19200;
        }

        public void UnscheduleFutureInvocation(IKFutureSchedulerObject Object)
        {
            lock (WaitingObjects)
            {
                WaitingObjects.RemoveAll(x => x.Object == Object);
            }
        }

        private void WaitAndCheckScheduledObjects()
        {
            using (WaitEvent = new AutoResetEvent(false))
            {
                while (KeepRunning)
                {
                    WaitingObject Next;

                    lock (WaitingObjects)
                    {
                        Next = WaitingObjects.OrderBy(x => x.TimePoint).FirstOrDefault();
                    }

                    if (Next != null)
                    {
                        long TimePoint = PerformanceCounter.ElapsedMilliseconds;

                        if (Next.TimePoint > TimePoint)
                        {
                            WaitEvent.WaitOne((int)(Next.TimePoint - TimePoint));
                        }

                        bool TimeUp = PerformanceCounter.ElapsedMilliseconds >= Next.TimePoint;

                        if (TimeUp)
                        {
                            lock (WaitingObjects)
                            {
                                TimeUp = WaitingObjects.Remove(Next);
                            }
                        }

                        if (TimeUp)
                        {
                            Next.Object.TimeUp();
                        }
                    }
                    else
                    {
                        WaitEvent.WaitOne();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                KeepRunning = false;

                WaitEvent?.Set();
            }
        }
    }
}