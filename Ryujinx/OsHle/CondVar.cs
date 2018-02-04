using ChocolArm64.Memory;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.OsHle
{
    class CondVar
    {
        private AMemory Memory;

        private long CondVarAddress;
        private long Timeout;

        private class WaitingThread
        {
            public int Handle;

            public ManualResetEvent Event;

            public WaitingThread(int Handle, ManualResetEvent Event)
            {
                this.Handle = Handle;
                this.Event  = Event;
            }
        }

        private ConcurrentQueue<WaitingThread> WaitingThreads;

        public CondVar(AMemory Memory, long CondVarAddress, long Timeout)
        {
            this.Memory         = Memory;
            this.CondVarAddress = CondVarAddress;
            this.Timeout        = Timeout;

            WaitingThreads = new ConcurrentQueue<WaitingThread>();
        }

        public void WaitForSignal(int ThreadHandle)
        {
            int Count = Memory.ReadInt32(CondVarAddress);

            if (Count <= 0)
            {
                return;
            }

            Memory.WriteInt32(CondVarAddress, Count - 1);

            ManualResetEvent Event = new ManualResetEvent(false);

            WaitingThreads.Enqueue(new WaitingThread(ThreadHandle, Event));

            if (Timeout != -1)
            {
                Event.WaitOne((int)(Timeout / 1000000));
            }
            else
            {
                Event.WaitOne();
            }
        }

        public void SetSignal(int Count)
        {
            if (Count == -1)
            {
                while (WaitingThreads.TryDequeue(out WaitingThread Thread))
                {
                    Thread.Event.Set();
                }

                Memory.WriteInt32(CondVarAddress, WaitingThreads.Count);
            }
            else
            {
                //TODO: Threads with the highest priority needs to be signaled first.
                if (WaitingThreads.TryDequeue(out WaitingThread Thread))
                {
                    Thread.Event.Set();
                }

                Memory.WriteInt32(CondVarAddress, Count);
            }
        }
    }
}