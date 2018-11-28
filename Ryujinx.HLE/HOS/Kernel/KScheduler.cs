using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class KScheduler : IDisposable
    {
        public const int PrioritiesCount = 64;
        public const int CpuCoresCount   = 4;

        private const int PreemptionPriorityCores012 = 59;
        private const int PreemptionPriorityCore3    = 63;

        private Horizon System;

        public KSchedulingData SchedulingData { get; private set; }

        public KCoreContext[] CoreContexts { get; private set; }

        public bool ThreadReselectionRequested { get; set; }

        public KScheduler(Horizon System)
        {
            this.System = System;

            SchedulingData = new KSchedulingData();

            CoreManager = new HleCoreManager();

            CoreContexts = new KCoreContext[CpuCoresCount];

            for (int Core = 0; Core < CpuCoresCount; Core++)
            {
                CoreContexts[Core] = new KCoreContext(this, CoreManager);
            }
        }

        private void PreemptThreads()
        {
            System.CriticalSection.Enter();

            PreemptThread(PreemptionPriorityCores012, 0);
            PreemptThread(PreemptionPriorityCores012, 1);
            PreemptThread(PreemptionPriorityCores012, 2);
            PreemptThread(PreemptionPriorityCore3,    3);

            System.CriticalSection.Leave();
        }

        private void PreemptThread(int Prio, int Core)
        {
            IEnumerable<KThread> ScheduledThreads = SchedulingData.ScheduledThreads(Core);

            KThread SelectedThread = ScheduledThreads.FirstOrDefault(x => x.DynamicPriority == Prio);

            //Yield priority queue.
            if (SelectedThread != null)
            {
                SchedulingData.Reschedule(Prio, Core, SelectedThread);
            }

            IEnumerable<KThread> SuitableCandidates()
            {
                foreach (KThread Thread in SchedulingData.SuggestedThreads(Core))
                {
                    int SrcCore = Thread.CurrentCore;

                    if (SrcCore >= 0)
                    {
                        KThread HighestPrioSrcCore = SchedulingData.ScheduledThreads(SrcCore).FirstOrDefault();

                        if (HighestPrioSrcCore != null && HighestPrioSrcCore.DynamicPriority < 2)
                        {
                            break;
                        }

                        if (HighestPrioSrcCore == Thread)
                        {
                            continue;
                        }
                    }

                    //If the candidate was scheduled after the current thread, then it's not worth it.
                    if (SelectedThread == null || SelectedThread.LastScheduledTime >= Thread.LastScheduledTime)
                    {
                        yield return Thread;
                    }
                }
            }

            //Select candidate threads that could run on this core.
            //Only take into account threads that are not yet selected.
            KThread Dst = SuitableCandidates().FirstOrDefault(x => x.DynamicPriority == Prio);

            if (Dst != null)
            {
                SchedulingData.TransferToCore(Prio, Core, Dst);

                SelectedThread = Dst;
            }

            //If the priority of the currently selected thread is lower than preemption priority,
            //then allow threads with lower priorities to be selected aswell.
            if (SelectedThread != null && SelectedThread.DynamicPriority > Prio)
            {
                Func<KThread, bool> Predicate = x => x.DynamicPriority >= SelectedThread.DynamicPriority;

                Dst = SuitableCandidates().FirstOrDefault(Predicate);

                if (Dst != null)
                {
                    SchedulingData.TransferToCore(Dst.DynamicPriority, Core, Dst);
                }
            }

            ThreadReselectionRequested = true;
        }

        public void SelectThreads()
        {
            ThreadReselectionRequested = false;

            for (int Core = 0; Core < CpuCoresCount; Core++)
            {
                KThread Thread = SchedulingData.ScheduledThreads(Core).FirstOrDefault();

                CoreContexts[Core].SelectThread(Thread);
            }

            for (int Core = 0; Core < CpuCoresCount; Core++)
            {
                //If the core is not idle (there's already a thread running on it),
                //then we don't need to attempt load balancing.
                if (SchedulingData.ScheduledThreads(Core).Any())
                {
                    continue;
                }

                int[] SrcCoresHighestPrioThreads = new int[CpuCoresCount];

                int SrcCoresHighestPrioThreadsCount = 0;

                KThread Dst = null;

                //Select candidate threads that could run on this core.
                //Give preference to threads that are not yet selected.
                foreach (KThread Thread in SchedulingData.SuggestedThreads(Core))
                {
                    if (Thread.CurrentCore < 0 || Thread != CoreContexts[Thread.CurrentCore].SelectedThread)
                    {
                        Dst = Thread;

                        break;
                    }

                    SrcCoresHighestPrioThreads[SrcCoresHighestPrioThreadsCount++] = Thread.CurrentCore;
                }

                //Not yet selected candidate found.
                if (Dst != null)
                {
                    //Priorities < 2 are used for the kernel message dispatching
                    //threads, we should skip load balancing entirely.
                    if (Dst.DynamicPriority >= 2)
                    {
                        SchedulingData.TransferToCore(Dst.DynamicPriority, Core, Dst);

                        CoreContexts[Core].SelectThread(Dst);
                    }

                    continue;
                }

                //All candiates are already selected, choose the best one
                //(the first one that doesn't make the source core idle if moved).
                for (int Index = 0; Index < SrcCoresHighestPrioThreadsCount; Index++)
                {
                    int SrcCore = SrcCoresHighestPrioThreads[Index];

                    KThread Src = SchedulingData.ScheduledThreads(SrcCore).ElementAtOrDefault(1);

                    if (Src != null)
                    {
                        //Run the second thread on the queue on the source core,
                        //move the first one to the current core.
                        KThread OrigSelectedCoreSrc = CoreContexts[SrcCore].SelectedThread;

                        CoreContexts[SrcCore].SelectThread(Src);

                        SchedulingData.TransferToCore(OrigSelectedCoreSrc.DynamicPriority, Core, OrigSelectedCoreSrc);

                        CoreContexts[Core].SelectThread(OrigSelectedCoreSrc);
                    }
                }
            }
        }

        public KThread GetCurrentThread()
        {
            lock (CoreContexts)
            {
                for (int Core = 0; Core < CpuCoresCount; Core++)
                {
                    if (CoreContexts[Core].CurrentThread?.Context.IsCurrentThread() ?? false)
                    {
                        return CoreContexts[Core].CurrentThread;
                    }
                }
            }

            throw new InvalidOperationException("Current thread is not scheduled!");
        }

        public KProcess GetCurrentProcess()
        {
            return GetCurrentThread().Owner;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                KeepPreempting = false;
            }
        }
    }
}