using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSynchronizationObject
    {
        public LinkedList<KThread> WaitingThreads;

        protected Horizon System;

        public KSynchronizationObject(Horizon System)
        {
            this.System = System;

            WaitingThreads = new LinkedList<KThread>();
        }

        public LinkedListNode<KThread> AddWaitingThread(KThread Thread)
        {
            return WaitingThreads.AddLast(Thread);
        }

        public void RemoveWaitingThread(LinkedListNode<KThread> Node)
        {
            WaitingThreads.Remove(Node);
        }

        public virtual void Signal()
        {
            System.Synchronization.SignalObject(this);
        }

        public virtual bool IsSignaled()
        {
            return false;
        }
    }
}