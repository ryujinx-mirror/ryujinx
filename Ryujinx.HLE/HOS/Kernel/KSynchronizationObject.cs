using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSynchronizationObject : KAutoObject
    {
        public LinkedList<KThread> WaitingThreads;

        public KSynchronizationObject(Horizon System) : base(System)
        {
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