using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KCoreContext
    {
        private KScheduler Scheduler;

        private HleCoreManager CoreManager;

        public bool ContextSwitchNeeded { get; private set; }

        public long LastContextSwitchTime { get; private set; }

        public long TotalIdleTimeTicks { get; private set; } //TODO

        public KThread CurrentThread  { get; private set; }
        public KThread SelectedThread { get; private set; }

        public KCoreContext(KScheduler Scheduler, HleCoreManager CoreManager)
        {
            this.Scheduler   = Scheduler;
            this.CoreManager = CoreManager;
        }

        public void SelectThread(KThread Thread)
        {
            SelectedThread = Thread;

            if (SelectedThread != CurrentThread)
            {
                ContextSwitchNeeded = true;
            }
        }

        public void UpdateCurrentThread()
        {
            ContextSwitchNeeded = false;

            LastContextSwitchTime = PerformanceCounter.ElapsedMilliseconds;

            CurrentThread = SelectedThread;

            if (CurrentThread != null)
            {
                long CurrentTime = PerformanceCounter.ElapsedMilliseconds;

                CurrentThread.TotalTimeRunning += CurrentTime - CurrentThread.LastScheduledTime;
                CurrentThread.LastScheduledTime = CurrentTime;
            }
        }

        public void ContextSwitch()
        {
            ContextSwitchNeeded = false;

            LastContextSwitchTime = PerformanceCounter.ElapsedMilliseconds;

            if (CurrentThread != null)
            {
                CoreManager.Reset(CurrentThread.Context.Work);
            }

            CurrentThread = SelectedThread;

            if (CurrentThread != null)
            {
                long CurrentTime = PerformanceCounter.ElapsedMilliseconds;

                CurrentThread.TotalTimeRunning += CurrentTime - CurrentThread.LastScheduledTime;
                CurrentThread.LastScheduledTime = CurrentTime;

                CurrentThread.ClearExclusive();

                CoreManager.Set(CurrentThread.Context.Work);

                CurrentThread.Context.Execute();
            }
        }
    }
}