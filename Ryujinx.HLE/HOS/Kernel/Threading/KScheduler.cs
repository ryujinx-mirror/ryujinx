using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    partial class KScheduler : IDisposable
    {
        public const int PrioritiesCount = 64;
        public const int CpuCoresCount   = 4;

        private const int PreemptionPriorityCores012 = 59;
        private const int PreemptionPriorityCore3    = 63;

        private Horizon _system;

        public KSchedulingData SchedulingData { get; private set; }

        public KCoreContext[] CoreContexts { get; private set; }

        public bool ThreadReselectionRequested { get; set; }

        public KScheduler(Horizon system)
        {
            _system = system;

            SchedulingData = new KSchedulingData();

            CoreManager = new HleCoreManager();

            CoreContexts = new KCoreContext[CpuCoresCount];

            for (int core = 0; core < CpuCoresCount; core++)
            {
                CoreContexts[core] = new KCoreContext(this, CoreManager);
            }
        }

        private void PreemptThreads()
        {
            _system.CriticalSection.Enter();

            PreemptThread(PreemptionPriorityCores012, 0);
            PreemptThread(PreemptionPriorityCores012, 1);
            PreemptThread(PreemptionPriorityCores012, 2);
            PreemptThread(PreemptionPriorityCore3,    3);

            _system.CriticalSection.Leave();
        }

        private void PreemptThread(int prio, int core)
        {
            IEnumerable<KThread> scheduledThreads = SchedulingData.ScheduledThreads(core);

            KThread selectedThread = scheduledThreads.FirstOrDefault(x => x.DynamicPriority == prio);

            // Yield priority queue.
            if (selectedThread != null)
            {
                SchedulingData.Reschedule(prio, core, selectedThread);
            }

            IEnumerable<KThread> SuitableCandidates()
            {
                foreach (KThread thread in SchedulingData.SuggestedThreads(core))
                {
                    int srcCore = thread.CurrentCore;

                    if (srcCore >= 0)
                    {
                        KThread highestPrioSrcCore = SchedulingData.ScheduledThreads(srcCore).FirstOrDefault();

                        if (highestPrioSrcCore != null && highestPrioSrcCore.DynamicPriority < 2)
                        {
                            break;
                        }

                        if (highestPrioSrcCore == thread)
                        {
                            continue;
                        }
                    }

                    // If the candidate was scheduled after the current thread, then it's not worth it.
                    if (selectedThread == null || selectedThread.LastScheduledTime >= thread.LastScheduledTime)
                    {
                        yield return thread;
                    }
                }
            }

            // Select candidate threads that could run on this core.
            // Only take into account threads that are not yet selected.
            KThread dst = SuitableCandidates().FirstOrDefault(x => x.DynamicPriority == prio);

            if (dst != null)
            {
                SchedulingData.TransferToCore(prio, core, dst);

                selectedThread = dst;
            }

            // If the priority of the currently selected thread is lower than preemption priority,
            // then allow threads with lower priorities to be selected aswell.
            if (selectedThread != null && selectedThread.DynamicPriority > prio)
            {
                Func<KThread, bool> predicate = x => x.DynamicPriority >= selectedThread.DynamicPriority;

                dst = SuitableCandidates().FirstOrDefault(predicate);

                if (dst != null)
                {
                    SchedulingData.TransferToCore(dst.DynamicPriority, core, dst);
                }
            }

            ThreadReselectionRequested = true;
        }

        public void SelectThreads()
        {
            ThreadReselectionRequested = false;

            for (int core = 0; core < CpuCoresCount; core++)
            {
                KThread thread = SchedulingData.ScheduledThreads(core).FirstOrDefault();

                CoreContexts[core].SelectThread(thread);
            }

            for (int core = 0; core < CpuCoresCount; core++)
            {
                // If the core is not idle (there's already a thread running on it),
                // then we don't need to attempt load balancing.
                if (SchedulingData.ScheduledThreads(core).Any())
                {
                    continue;
                }

                int[] srcCoresHighestPrioThreads = new int[CpuCoresCount];

                int srcCoresHighestPrioThreadsCount = 0;

                KThread dst = null;

                // Select candidate threads that could run on this core.
                // Give preference to threads that are not yet selected.
                foreach (KThread thread in SchedulingData.SuggestedThreads(core))
                {
                    if (thread.CurrentCore < 0 || thread != CoreContexts[thread.CurrentCore].SelectedThread)
                    {
                        dst = thread;

                        break;
                    }

                    srcCoresHighestPrioThreads[srcCoresHighestPrioThreadsCount++] = thread.CurrentCore;
                }

                // Not yet selected candidate found.
                if (dst != null)
                {
                    // Priorities < 2 are used for the kernel message dispatching
                    // threads, we should skip load balancing entirely.
                    if (dst.DynamicPriority >= 2)
                    {
                        SchedulingData.TransferToCore(dst.DynamicPriority, core, dst);

                        CoreContexts[core].SelectThread(dst);
                    }

                    continue;
                }

                // All candidates are already selected, choose the best one
                // (the first one that doesn't make the source core idle if moved).
                for (int index = 0; index < srcCoresHighestPrioThreadsCount; index++)
                {
                    int srcCore = srcCoresHighestPrioThreads[index];

                    KThread src = SchedulingData.ScheduledThreads(srcCore).ElementAtOrDefault(1);

                    if (src != null)
                    {
                        // Run the second thread on the queue on the source core,
                        // move the first one to the current core.
                        KThread origSelectedCoreSrc = CoreContexts[srcCore].SelectedThread;

                        CoreContexts[srcCore].SelectThread(src);

                        SchedulingData.TransferToCore(origSelectedCoreSrc.DynamicPriority, core, origSelectedCoreSrc);

                        CoreContexts[core].SelectThread(origSelectedCoreSrc);
                    }
                }
            }
        }

        public KThread GetCurrentThread()
        {
            lock (CoreContexts)
            {
                for (int core = 0; core < CpuCoresCount; core++)
                {
                    if (CoreContexts[core].CurrentThread?.IsCurrentHostThread() ?? false)
                    {
                        return CoreContexts[core].CurrentThread;
                    }
                }
            }

            return GetDummyThread();

            throw new InvalidOperationException("Current thread is not scheduled!");
        }

        private KThread _dummyThread;

        private KThread GetDummyThread()
        {
            if (_dummyThread != null)
            {
                return _dummyThread;
            }

            KProcess dummyProcess = new KProcess(_system);

            KThread dummyThread = new KThread(_system);

            dummyThread.Initialize(0, 0, 0, 44, 0, dummyProcess, ThreadType.Dummy);

            return _dummyThread = dummyThread;
        }

        public KProcess GetCurrentProcess()
        {
            return GetCurrentThread().Owner;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keepPreempting = false;
            }
        }
    }
}