using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Stopwatch Counter;

        private bool KeepRunning;

        public KTimeManager()
        {
            WaitingObjects = new List<WaitingObject>();

            Counter = new Stopwatch();

            Counter.Start();

            KeepRunning = true;

            Thread Work = new Thread(WaitAndCheckScheduledObjects);

            Work.Start();
        }

        public void ScheduleFutureInvocation(IKFutureSchedulerObject Object, long Timeout)
        {
            lock (WaitingObjects)
            {
                long TimePoint = Counter.ElapsedMilliseconds + ConvertNanosecondsToMilliseconds(Timeout);

                WaitingObjects.Add(new WaitingObject(Object, TimePoint));
            }

            WaitEvent.Set();
        }

        private long ConvertNanosecondsToMilliseconds(long Timeout)
        {
            Timeout /= 1000000;

            if ((ulong)Timeout > int.MaxValue)
            {
                return int.MaxValue;
            }

            return Timeout;
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
                    Monitor.Enter(WaitingObjects);

                    WaitingObject Next = WaitingObjects.OrderBy(x => x.TimePoint).FirstOrDefault();

                    Monitor.Exit(WaitingObjects);

                    if (Next != null)
                    {
                        long TimePoint = Counter.ElapsedMilliseconds;

                        if (Next.TimePoint > TimePoint)
                        {
                            WaitEvent.WaitOne((int)(Next.TimePoint - TimePoint));
                        }

                        Monitor.Enter(WaitingObjects);

                        bool TimeUp = Counter.ElapsedMilliseconds >= Next.TimePoint && WaitingObjects.Remove(Next);

                        Monitor.Exit(WaitingObjects);

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