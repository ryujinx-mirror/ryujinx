using ChocolArm64;
using System;

namespace Ryujinx.Core.OsHle.Handles
{
    class KThread : KSynchronizationObject
    {
        public AThread Thread { get; private set; }

        public int CoreMask { get; set; }

        public long MutexAddress   { get; set; }
        public long CondVarAddress { get; set; }

        private Process Process;

        public KThread NextMutexThread   { get; set; }
        public KThread NextCondVarThread { get; set; }

        public KThread MutexOwner { get; set; }

        public int ActualPriority { get; private set; }
        public int WantedPriority { get; private set; }

        public int IdealCore  { get; set; }
        public int ActualCore { get; set; }

        public int WaitHandle { get; set; }

        public int ThreadId => Thread.ThreadId;

        public KThread(
            AThread Thread,
            Process Process,
            int     IdealCore,
            int     Priority)
        {
            this.Thread    = Thread;
            this.Process   = Process;
            this.IdealCore = IdealCore;

            CoreMask = 1 << IdealCore;

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

                UpdateWaitLists();

                MutexOwner?.UpdatePriority();
            }
        }

        private void UpdateWaitLists()
        {
            UpdateMutexList();
            UpdateCondVarList();

            Process.Scheduler.Resort(this);
        }

        private void UpdateMutexList()
        {
            KThread OwnerThread = MutexOwner;

            if (OwnerThread == null)
            {
                return;
            }

            //The MutexOwner field should only be non-null when the thread is
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
                    if (CurrThread.NextMutexThread.ActualPriority > ActualPriority)
                    {
                        break;
                    }

                    CurrThread = CurrThread.NextMutexThread;
                }

                NextMutexThread = CurrThread.NextMutexThread;

                CurrThread.NextMutexThread = this;
            }
        }

        private void UpdateCondVarList()
        {
            lock (Process.ThreadArbiterListLock)
            {
                if (Process.ThreadArbiterListHead == null)
                {
                    return;
                }

                //Remove itself from the list.
                bool Found;

                KThread CurrThread = Process.ThreadArbiterListHead;

                if (Found = (Process.ThreadArbiterListHead == this))
                {
                    Process.ThreadArbiterListHead = Process.ThreadArbiterListHead.NextCondVarThread;
                }
                else
                {
                    while (CurrThread.NextCondVarThread != null)
                    {
                        if (CurrThread.NextCondVarThread == this)
                        {
                            CurrThread.NextCondVarThread = NextCondVarThread;

                            Found = true;

                            break;
                        }

                        CurrThread = CurrThread.NextCondVarThread;
                    }
                }

                if (!Found)
                {
                    return;
                }

                //Re-add taking new priority into account.
                if (Process.ThreadArbiterListHead == null ||
                    Process.ThreadArbiterListHead.ActualPriority > ActualPriority)
                {
                    NextCondVarThread = Process.ThreadArbiterListHead;

                    Process.ThreadArbiterListHead = this;

                    return;
                }

                CurrThread = Process.ThreadArbiterListHead;

                while (CurrThread.NextCondVarThread != null)
                {
                    if (CurrThread.NextCondVarThread.ActualPriority > ActualPriority)
                    {
                        break;
                    }

                    CurrThread = CurrThread.NextCondVarThread;
                }

                NextCondVarThread = CurrThread.NextCondVarThread;

                CurrThread.NextCondVarThread = this;
            }
        }
    }
}