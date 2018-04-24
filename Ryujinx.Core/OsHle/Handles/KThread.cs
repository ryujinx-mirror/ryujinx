using ChocolArm64;
using System;

namespace Ryujinx.Core.OsHle.Handles
{
    class KThread : KSynchronizationObject
    {
        public AThread Thread { get; private set; }

        public long MutexAddress   { get; set; }
        public long CondVarAddress { get; set; }

        public KThread NextMutexThread   { get; set; }
        public KThread NextCondVarThread { get; set; }

        public KThread MutexOwner { get; set; }

        public int ActualPriority { get; private set; }
        public int WantedPriority { get; private set; }

        public int ProcessorId  { get; private set; }

        public int WaitHandle { get; set; }

        public int ThreadId => Thread.ThreadId;

        public KThread(AThread Thread, int ProcessorId, int Priority)
        {
            this.Thread      = Thread;
            this.ProcessorId = ProcessorId;

            ActualPriority = WantedPriority = Priority;
        }

        public void SetPriority(int Priority)
        {
            WantedPriority = Priority;

            UpdatePriority();
        }

        public void UpdatePriority()
        {
            int OldPriority = ActualPriority;

            int CurrPriority = WantedPriority;

            if (NextMutexThread != null && CurrPriority > NextMutexThread.WantedPriority)
            {
                CurrPriority = NextMutexThread.WantedPriority;
            }

            if (CurrPriority != OldPriority)
            {
                ActualPriority = CurrPriority;

                UpdateWaitList();

                MutexOwner?.UpdatePriority();
            }
        }

        private void UpdateWaitList()
        {
            KThread OwnerThread = MutexOwner;

            if (OwnerThread != null)
            {
                //The MutexOwner field should only be non null when the thread is
                //waiting for the lock, and the lock belongs to another thread.
                if (OwnerThread == this)
                {
                    throw new InvalidOperationException();
                }

                lock (OwnerThread)
                {
                    //Remove itself from the list.
                    KThread CurrThread = OwnerThread;

                    while (CurrThread.NextMutexThread != null)
                    {
                        if (CurrThread.NextMutexThread == this)
                        {
                            CurrThread.NextMutexThread = NextMutexThread;

                            break;
                        }

                        CurrThread = CurrThread.NextMutexThread;
                    }

                    //Re-add taking new priority into account.
                    CurrThread = OwnerThread;

                    while (CurrThread.NextMutexThread != null)
                    {
                        if (CurrThread.NextMutexThread.ActualPriority < ActualPriority)
                        {
                            break;
                        }

                        CurrThread = CurrThread.NextMutexThread;
                    }

                    NextMutexThread = CurrThread.NextMutexThread;

                    CurrThread.NextMutexThread = this;
                }
            }
        }
    }
}