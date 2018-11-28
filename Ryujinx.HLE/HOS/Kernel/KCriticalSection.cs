using ChocolArm64;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KCriticalSection
    {
        private Horizon System;

        public object LockObj { get; private set; }

        private int RecursionCount;

        public KCriticalSection(Horizon System)
        {
            this.System = System;

            LockObj = new object();
        }

        public void Enter()
        {
            Monitor.Enter(LockObj);

            RecursionCount++;
        }

        public void Leave()
        {
            if (RecursionCount == 0)
            {
                return;
            }

            bool DoContextSwitch = false;

            if (--RecursionCount == 0)
            {
                if (System.Scheduler.ThreadReselectionRequested)
                {
                    System.Scheduler.SelectThreads();
                }

                Monitor.Exit(LockObj);

                if (System.Scheduler.MultiCoreScheduling)
                {
                    lock (System.Scheduler.CoreContexts)
                    {
                        for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
                        {
                            KCoreContext CoreContext = System.Scheduler.CoreContexts[Core];

                            if (CoreContext.ContextSwitchNeeded)
                            {
                                CpuThread CurrentHleThread = CoreContext.CurrentThread?.Context;

                                if (CurrentHleThread == null)
                                {
                                    //Nothing is running, we can perform the context switch immediately.
                                    CoreContext.ContextSwitch();
                                }
                                else if (CurrentHleThread.IsCurrentThread())
                                {
                                    //Thread running on the current core, context switch will block.
                                    DoContextSwitch = true;
                                }
                                else
                                {
                                    //Thread running on another core, request a interrupt.
                                    CurrentHleThread.RequestInterrupt();
                                }
                            }
                        }
                    }
                }
                else
                {
                    DoContextSwitch = true;
                }
            }
            else
            {
                Monitor.Exit(LockObj);
            }

            if (DoContextSwitch)
            {
                System.Scheduler.ContextSwitch();
            }
        }
    }
}