using System;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    partial class KScheduler
    {
        private const int RoundRobinTimeQuantumMs = 10;

        private int _currentCore;

        public bool MultiCoreScheduling { get; set; }

        public HleCoreManager CoreManager { get; private set; }

        private bool _keepPreempting;

        public void StartAutoPreemptionThread()
        {
            Thread preemptionThread = new Thread(PreemptCurrentThread)
            {
                Name = "HLE.PreemptionThread"
            };

            _keepPreempting = true;

            preemptionThread.Start();
        }

        public void ContextSwitch()
        {
            lock (CoreContexts)
            {
                if (MultiCoreScheduling)
                {
                    int selectedCount = 0;

                    for (int core = 0; core < CpuCoresCount; core++)
                    {
                        KCoreContext coreContext = CoreContexts[core];

                        if (coreContext.ContextSwitchNeeded && (coreContext.CurrentThread?.IsCurrentHostThread() ?? false))
                        {
                            coreContext.ContextSwitch();
                        }

                        if (coreContext.CurrentThread?.IsCurrentHostThread() ?? false)
                        {
                            selectedCount++;
                        }
                    }

                    if (selectedCount == 0)
                    {
                        CoreManager.Reset(Thread.CurrentThread);
                    }
                    else if (selectedCount == 1)
                    {
                        CoreManager.Set(Thread.CurrentThread);
                    }
                    else
                    {
                        throw new InvalidOperationException("Thread scheduled in more than one core!");
                    }
                }
                else
                {
                    KThread currentThread = CoreContexts[_currentCore].CurrentThread;

                    bool hasThreadExecuting = currentThread != null;

                    if (hasThreadExecuting)
                    {
                        // If this is not the thread that is currently executing, we need
                        // to request an interrupt to allow safely starting another thread.
                        if (!currentThread.IsCurrentHostThread())
                        {
                            currentThread.Context.RequestInterrupt();

                            return;
                        }

                        CoreManager.Reset(currentThread.HostThread);
                    }

                    // Advance current core and try picking a thread,
                    // keep advancing if it is null.
                    for (int core = 0; core < 4; core++)
                    {
                        _currentCore = (_currentCore + 1) % CpuCoresCount;

                        KCoreContext coreContext = CoreContexts[_currentCore];

                        coreContext.UpdateCurrentThread();

                        if (coreContext.CurrentThread != null)
                        {
                            CoreManager.Set(coreContext.CurrentThread.HostThread);

                            coreContext.CurrentThread.Execute();

                            break;
                        }
                    }

                    // If nothing was running before, then we are on a "external"
                    // HLE thread, we don't need to wait.
                    if (!hasThreadExecuting)
                    {
                        return;
                    }
                }
            }

            CoreManager.Wait(Thread.CurrentThread);
        }

        private void PreemptCurrentThread()
        {
            // Preempts current thread every 10 milliseconds on a round-robin fashion,
            // when multi core scheduling is disabled, to try ensuring that all threads
            // gets a chance to run.
            while (_keepPreempting)
            {
                lock (CoreContexts)
                {
                    KThread currentThread = CoreContexts[_currentCore].CurrentThread;

                    currentThread?.Context.RequestInterrupt();
                }

                PreemptThreads();

                Thread.Sleep(RoundRobinTimeQuantumMs);
            }
        }

        public void ExitThread(KThread thread)
        {
            thread.Context.Running = false;

            CoreManager.Exit(thread.HostThread);
        }

        public void RemoveThread(KThread thread)
        {
            CoreManager.RemoveThread(thread.HostThread);
        }
    }
}