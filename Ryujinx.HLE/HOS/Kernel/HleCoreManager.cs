using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    class HleCoreManager
    {
        private ConcurrentDictionary<Thread, ManualResetEvent> Threads;

        public HleCoreManager()
        {
            Threads = new ConcurrentDictionary<Thread, ManualResetEvent>();
        }

        public ManualResetEvent GetThread(Thread Thread)
        {
            return Threads.GetOrAdd(Thread, (Key) => new ManualResetEvent(false));
        }

        public void RemoveThread(Thread Thread)
        {
            if (Threads.TryRemove(Thread, out ManualResetEvent Event))
            {
                Event.Set();
                Event.Dispose();
            }
        }
    }
}