using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KCriticalSection
    {
        private readonly KernelContext _context;

        public object LockObj { get; private set; }

        private int _recursionCount;

        public KCriticalSection(KernelContext context)
        {
            _context = context;

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
                if (_context.Scheduler.ThreadReselectionRequested)
                {
                    _context.Scheduler.SelectThreads();
                }

                Monitor.Exit(LockObj);

                if (_context.Scheduler.MultiCoreScheduling)
                {
                    lock (_context.Scheduler.CoreContexts)
                    {
                        for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                        {
                            KCoreContext coreContext = _context.Scheduler.CoreContexts[core];

                            if (coreContext.ContextSwitchNeeded)
                            {
                                KThread currentThread = coreContext.CurrentThread;

                                if (currentThread == null)
                                {
                                    // Nothing is running, we can perform the context switch immediately.
                                    coreContext.ContextSwitch();
                                }
                                else if (currentThread.IsCurrentHostThread())
                                {
                                    // Thread running on the current core, context switch will block.
                                    doContextSwitch = true;
                                }
                                else
                                {
                                    // Thread running on another core, request a interrupt.
                                    currentThread.Context.RequestInterrupt();
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
                _context.Scheduler.ContextSwitch();
            }
        }
    }
}