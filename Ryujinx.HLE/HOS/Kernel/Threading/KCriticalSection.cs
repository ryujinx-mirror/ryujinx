using ChocolArm64;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KCriticalSection
    {
        private Horizon _system;

        public object LockObj { get; private set; }

        private int _recursionCount;

        public KCriticalSection(Horizon system)
        {
            _system = system;

            LockObj = new object();
        }

        public void Enter()
        {
            Monitor.Enter(LockObj);

            _recursionCount++;
        }

        public void Leave()
        {
            if (_recursionCount == 0)
            {
                return;
            }

            bool doContextSwitch = false;

            if (--_recursionCount == 0)
            {
                if (_system.Scheduler.ThreadReselectionRequested)
                {
                    _system.Scheduler.SelectThreads();
                }

                Monitor.Exit(LockObj);

                if (_system.Scheduler.MultiCoreScheduling)
                {
                    lock (_system.Scheduler.CoreContexts)
                    {
                        for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                        {
                            KCoreContext coreContext = _system.Scheduler.CoreContexts[core];

                            if (coreContext.ContextSwitchNeeded)
                            {
                                CpuThread currentHleThread = coreContext.CurrentThread?.Context;

                                if (currentHleThread == null)
                                {
                                    //Nothing is running, we can perform the context switch immediately.
                                    coreContext.ContextSwitch();
                                }
                                else if (currentHleThread.IsCurrentThread())
                                {
                                    //Thread running on the current core, context switch will block.
                                    doContextSwitch = true;
                                }
                                else
                                {
                                    //Thread running on another core, request a interrupt.
                                    currentHleThread.RequestInterrupt();
                                }
                            }
                        }
                    }
                }
                else
                {
                    doContextSwitch = true;
                }
            }
            else
            {
                Monitor.Exit(LockObj);
            }

            if (doContextSwitch)
            {
                _system.Scheduler.ContextSwitch();
            }
        }
    }
}