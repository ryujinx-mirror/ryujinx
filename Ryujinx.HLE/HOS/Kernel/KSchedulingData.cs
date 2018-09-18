using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSchedulingData
    {
        private LinkedList<KThread>[][] ScheduledThreadsPerPrioPerCore;
        private LinkedList<KThread>[][] SuggestedThreadsPerPrioPerCore;

        private long[] ScheduledPrioritiesPerCore;
        private long[] SuggestedPrioritiesPerCore;

        public KSchedulingData()
        {
            SuggestedThreadsPerPrioPerCore = new LinkedList<KThread>[KScheduler.PrioritiesCount][];
            ScheduledThreadsPerPrioPerCore = new LinkedList<KThread>[KScheduler.PrioritiesCount][];

            for (int Prio = 0; Prio < KScheduler.PrioritiesCount; Prio++)
            {
                SuggestedThreadsPerPrioPerCore[Prio] = new LinkedList<KThread>[KScheduler.CpuCoresCount];
                ScheduledThreadsPerPrioPerCore[Prio] = new LinkedList<KThread>[KScheduler.CpuCoresCount];

                for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
                {
                    SuggestedThreadsPerPrioPerCore[Prio][Core] = new LinkedList<KThread>();
                    ScheduledThreadsPerPrioPerCore[Prio][Core] = new LinkedList<KThread>();
                }
            }

            ScheduledPrioritiesPerCore = new long[KScheduler.CpuCoresCount];
            SuggestedPrioritiesPerCore = new long[KScheduler.CpuCoresCount];
        }

        public IEnumerable<KThread> SuggestedThreads(int Core)
        {
            return Iterate(SuggestedThreadsPerPrioPerCore, SuggestedPrioritiesPerCore, Core);
        }

        public IEnumerable<KThread> ScheduledThreads(int Core)
        {
            return Iterate(ScheduledThreadsPerPrioPerCore, ScheduledPrioritiesPerCore, Core);
        }

        private IEnumerable<KThread> Iterate(LinkedList<KThread>[][] ListPerPrioPerCore, long[] Prios, int Core)
        {
            long PrioMask = Prios[Core];

            int Prio = CountTrailingZeros(PrioMask);

            PrioMask &= ~(1L << Prio);

            while (Prio < KScheduler.PrioritiesCount)
            {
                LinkedList<KThread> List = ListPerPrioPerCore[Prio][Core];

                LinkedListNode<KThread> Node = List.First;

                while (Node != null)
                {
                    yield return Node.Value;

                    Node = Node.Next;
                }

                Prio = CountTrailingZeros(PrioMask);

                PrioMask &= ~(1L << Prio);
            }
        }

        private int CountTrailingZeros(long Value)
        {
            int Count = 0;

            while (((Value >> Count) & 0xf) == 0 && Count < 64)
            {
                Count += 4;
            }

            while (((Value >> Count) & 1) == 0 && Count < 64)
            {
                Count++;
            }

            return Count;
        }

        public void TransferToCore(int Prio, int DstCore, KThread Thread)
        {
            bool Schedulable = Thread.DynamicPriority < KScheduler.PrioritiesCount;

            int SrcCore = Thread.CurrentCore;

            Thread.CurrentCore = DstCore;

            if (SrcCore == DstCore || !Schedulable)
            {
                return;
            }

            if (SrcCore >= 0)
            {
                Unschedule(Prio, SrcCore, Thread);
            }

            if (DstCore >= 0)
            {
                Unsuggest(Prio, DstCore, Thread);
                Schedule(Prio, DstCore, Thread);
            }

            if (SrcCore >= 0)
            {
                Suggest(Prio, SrcCore, Thread);
            }
        }

        public void Suggest(int Prio, int Core, KThread Thread)
        {
            if (Prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            Thread.SiblingsPerCore[Core] = SuggestedQueue(Prio, Core).AddFirst(Thread);

            SuggestedPrioritiesPerCore[Core] |= 1L << Prio;
        }

        public void Unsuggest(int Prio, int Core, KThread Thread)
        {
            if (Prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            LinkedList<KThread> Queue = SuggestedQueue(Prio, Core);

            Queue.Remove(Thread.SiblingsPerCore[Core]);

            if (Queue.First == null)
            {
                SuggestedPrioritiesPerCore[Core] &= ~(1L << Prio);
            }
        }

        public void Schedule(int Prio, int Core, KThread Thread)
        {
            if (Prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            Thread.SiblingsPerCore[Core] = ScheduledQueue(Prio, Core).AddLast(Thread);

            ScheduledPrioritiesPerCore[Core] |= 1L << Prio;
        }

        public void SchedulePrepend(int Prio, int Core, KThread Thread)
        {
            if (Prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            Thread.SiblingsPerCore[Core] = ScheduledQueue(Prio, Core).AddFirst(Thread);

            ScheduledPrioritiesPerCore[Core] |= 1L << Prio;
        }

        public void Reschedule(int Prio, int Core, KThread Thread)
        {
            LinkedList<KThread> Queue = ScheduledQueue(Prio, Core);

            Queue.Remove(Thread.SiblingsPerCore[Core]);

            Thread.SiblingsPerCore[Core] = Queue.AddLast(Thread);
        }

        public void Unschedule(int Prio, int Core, KThread Thread)
        {
            if (Prio >= KScheduler.PrioritiesCount)
            {
                return;
            }

            LinkedList<KThread> Queue = ScheduledQueue(Prio, Core);

            Queue.Remove(Thread.SiblingsPerCore[Core]);

            if (Queue.First == null)
            {
                ScheduledPrioritiesPerCore[Core] &= ~(1L << Prio);
            }
        }

        private LinkedList<KThread> SuggestedQueue(int Prio, int Core)
        {
            return SuggestedThreadsPerPrioPerCore[Prio][Core];
        }

        private LinkedList<KThread> ScheduledQueue(int Prio, int Core)
        {
            return ScheduledThreadsPerPrioPerCore[Prio][Core];
        }
    }
}