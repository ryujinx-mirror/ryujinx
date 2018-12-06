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

        private ConcurrentDictionary<Thread, PausableThread> _threads;

        public HleCoreManager()
        {
            _threads = new ConcurrentDictionary<Thread, PausableThread>();
        }

        public void Set(Thread thread)
        {
            GetThread(thread).Event.Set();
        }

        public void Reset(Thread thread)
        {
            GetThread(thread).Event.Reset();
        }

        public void Wait(Thread thread)
        {
            PausableThread pausableThread = GetThread(thread);

            if (!pausableThread.IsExiting)
            {
                pausableThread.Event.WaitOne();
            }
        }

        public void Exit(Thread thread)
        {
            GetThread(thread).IsExiting = true;
        }

        private PausableThread GetThread(Thread thread)
        {
            return _threads.GetOrAdd(thread, (key) => new PausableThread());
        }

        public void RemoveThread(Thread thread)
        {
            if (_threads.TryRemove(thread, out PausableThread pausableThread))
            {
                pausableThread.Event.Set();
                pausableThread.Event.Dispose();
            }
        }
    }
}