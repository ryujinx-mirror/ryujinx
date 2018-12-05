using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    class HleCoreManager
    {
        private class PausableThread
        {
            public ManualResetEvent Event { get; private set; }

            public bool IsExiting { get; set; }

            public PausableThread()
            {
                Event = new ManualResetEvent(false);
            }
        }

        private ConcurrentDictionary<Thread, PausableThread> Threads;

        public HleCoreManager()
        {
            Threads = new ConcurrentDictionary<Thread, PausableThread>();
        }

        public void Set(Thread Thread)
        {
            GetThread(Thread).Event.Set();
        }

        public void Reset(Thread Thread)
        {
            GetThread(Thread).Event.Reset();
        }

        public void Wait(Thread Thread)
        {
            PausableThread PausableThread = GetThread(Thread);

            if (!PausableThread.IsExiting)
            {
                PausableThread.Event.WaitOne();
            }
        }

        public void Exit(Thread Thread)
        {
            GetThread(Thread).IsExiting = true;
        }

        private PausableThread GetThread(Thread Thread)
        {
            return Threads.GetOrAdd(Thread, (Key) => new PausableThread());
        }

        public void RemoveThread(Thread Thread)
        {
            if (Threads.TryRemove(Thread, out PausableThread PausableThread))
            {
                PausableThread.Event.Set();
                PausableThread.Event.Dispose();
            }
        }
    }
}