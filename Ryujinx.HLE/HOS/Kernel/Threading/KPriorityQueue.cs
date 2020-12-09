using System.Collections.Generic;
using System.Numerics;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KPriorityQueue
    {
        private LinkedList<KThread>[][] _scheduledThreadsPerPrioPerCore;
        private LinkedList<KThread>[][] _suggestedThreadsPerPrioPerCore;

        private long[] _scheduledPrioritiesPerCore;
        private long[] _suggestedPrioritiesPerCore;

        public KPriorityQueue()
        {
            _suggestedThreadsPerPrioPerCore = new LinkedList<KThread>[KScheduler.PrioritiesCount][];
            _scheduledThreadsPerPrioPerCore = new LinkedList<KThread>[KScheduler.PrioritiesCount][];

            for (int prio = 0; prio < KScheduler.PrioritiesCount; prio++)
            {
                _suggestedThreadsPerPrioPerCore[prio] = new LinkedList<KThread>[KScheduler.CpuCoresCount];
                _scheduledThreadsPerPrioPerCore[prio] = new LinkedList<KThread>[KScheduler.CpuCoresCount];

                for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                {
                    _suggestedThreadsPerPrioPerCore[prio][core] = new LinkedList<KThread>();
                    _scheduledThreadsPerPrioPerCore[prio][core] = new LinkedList<KThread>();
                }
            }

            _scheduledPrioritiesPerCore = new long[KScheduler.CpuCoresCount];
            _suggestedPrioritiesPerCore = new long[KScheduler.CpuCoresCount];
        }

        public IEnumerable<KThread> SuggestedThreads(int core)
        {
            return Iterate(_suggestedThreadsPerPrioPerCore, _suggestedPrioritiesPerCore, core);
        }

        public IEnumerable<KThread> ScheduledThreads(int core)
        {
            return Iterate(_scheduledThreadsPerPrioPerCore, _scheduledPrioritiesPerCore, core);
        }

        private IEnumerable<KThread> Iterate(LinkedList<KThread>[][] listPerPrioPerCore, long[] prios, int core)
        {
            long prioMask = prios[core];

            int prio = BitOperations.TrailingZeroCount(prioMask);

            prioMask &= ~(1L << prio);

            while (prio < KScheduler.PrioritiesCount)
            {
                LinkedList<KThread> list = listPerPrioPerCore[prio][core];

                LinkedListNode<KThread> node = list.First;

                while (node != null)
                {
                    yield return node.Value;

                    node = node.Next;
                }

                prio = BitOperations.TrailingZeroCount(prioMask);

                prioMask &= ~(1L << prio);
            }
        }

        public void TransferToCore(int prio, int dstCore, KThread thread)
        {
            int srcCore = thread.ActiveCore;
            if (srcCore == dstCore)
            {
                return;
            }

            thread.ActiveCore = dstCore;

            if (srcCore >= 0)
            {
                Unschedule(prio, srcCore, thread);
            }

            if (dstCore >= 0)
            {
                Unsuggest(prio, dstCore, thread);
                Schedule(prio, dstCore, thread);
            }

            if (srcCore >= 0)
            {
                Suggest(prio, srcCore, thread);
            }
        }

        public void Suggest(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            thread.SiblingsPerCore[core] = SuggestedQueue(prio, core).AddFirst(thread);

            _suggestedPrioritiesPerCore[core] |= 1L << prio;
        }

        public void Unsuggest(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            LinkedList<KThread> queue = SuggestedQueue(prio, core);

            queue.Remove(thread.SiblingsPerCore[core]);

            if (queue.First == null)
            {
                _suggestedPrioritiesPerCore[core] &= ~(1L << prio);
            }
        }

        public void Schedule(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            thread.SiblingsPerCore[core] = ScheduledQueue(prio, core).AddLast(thread);

            _scheduledPrioritiesPerCore[core] |= 1L << prio;
        }

        public void SchedulePrepend(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            thread.SiblingsPerCore[core] = ScheduledQueue(prio, core).AddFirst(thread);

            _scheduledPrioritiesPerCore[core] |= 1L << prio;
        }

        public KThread Reschedule(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return null;
            }

            LinkedList<KThread> queue = ScheduledQueue(prio, core);

            queue.Remove(thread.SiblingsPerCore[core]);

            thread.SiblingsPerCore[core] = queue.AddLast(thread);

            return queue.First.Value;
        }

        public void Unschedule(int prio, int core, KThread thread)
        {
            if (prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            LinkedList<KThread> queue = ScheduledQueue(prio, core);

            queue.Remove(thread.SiblingsPerCore[core]);

            if (queue.First == null)
            {
                _scheduledPrioritiesPerCore[core] &= ~(1L << prio);
            }
        }

        private LinkedList<KThread> SuggestedQueue(int prio, int core)
        {
            return _suggestedThreadsPerPrioPerCore[prio][core];
        }

        private LinkedList<KThread> ScheduledQueue(int prio, int core)
        {
            return _scheduledThreadsPerPrioPerCore[prio][core];
        }
    }
}