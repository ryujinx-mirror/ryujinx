using System;
using System.Threading;

namespace Ryujinx.HLE.OsHle.Handles
{
    class SchedulerThread : IDisposable
    {
        public KThread Thread { get; private set; }

        public SchedulerThread Next { get; set; }

        public bool IsActive { get; set; }

        public AutoResetEvent   WaitSync     { get; private set; }
        public ManualResetEvent WaitActivity { get; private set; }
        public AutoResetEvent   WaitSched    { get; private set; }

        public SchedulerThread(KThread Thread)
        {
            this.Thread = Thread;

            IsActive = true;

            WaitSync  = new AutoResetEvent(false);

            WaitActivity = new ManualResetEvent(true);

            WaitSched = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                WaitSync.Dispose();

                WaitActivity.Dispose();

                WaitSched.Dispose();
            }
        }
    }
}