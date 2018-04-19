using Ryujinx.Core.OsHle.Handles;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle.Kernel
{
    class ConditionVariable
    {
        private Process Process;

        private long CondVarAddress;

        private bool OwnsCondVarValue;

        private List<(KThread Thread, AutoResetEvent WaitEvent)> WaitingThreads;

        public ConditionVariable(Process Process, long CondVarAddress)
        {
            this.Process        = Process;
            this.CondVarAddress = CondVarAddress;

            WaitingThreads = new List<(KThread, AutoResetEvent)>();
        }

        public bool WaitForSignal(KThread Thread, long Timeout)
        {
            bool Result = true;

            int Count = Process.Memory.ReadInt32(CondVarAddress);

            if (Count <= 0)
            {
                using (AutoResetEvent WaitEvent = new AutoResetEvent(false))
                {
                    lock (WaitingThreads)
                    {
                        WaitingThreads.Add((Thread, WaitEvent));
                    }

                    Process.Scheduler.Suspend(Thread.ProcessorId);

                    if (Timeout < 0)
                    {
                        Result = WaitEvent.WaitOne();
                    }
                    else
                    {
                        Result = WaitEvent.WaitOne((int)(Timeout / 1000000));

                        lock (WaitingThreads)
                        {
                            WaitingThreads.Remove((Thread, WaitEvent));
                        }
                    }

                    Process.Scheduler.Resume(Thread);
                }
            }

            AcquireCondVarValue();

            Count = Process.Memory.ReadInt32(CondVarAddress);

            if (Count > 0)
            {
                Process.Memory.WriteInt32(CondVarAddress, Count - 1);
            }

            ReleaseCondVarValue();

            return Result;
        }

        public void SetSignal(KThread Thread, int Count)
        {
            lock (WaitingThreads)
            {
                if (Count < 0)
                {
                    Process.Memory.WriteInt32(CondVarAddress, WaitingThreads.Count);

                    foreach ((_, AutoResetEvent WaitEvent) in WaitingThreads)
                    {
                        WaitEvent.Set();
                    }

                    WaitingThreads.Clear();
                }
                else
                {
                    Process.Memory.WriteInt32(CondVarAddress, Count);

                    while (WaitingThreads.Count > 0 && Count-- > 0)
                    {
                        int HighestPriority  = WaitingThreads[0].Thread.Priority;
                        int HighestPrioIndex = 0;

                        for (int Index = 1; Index < WaitingThreads.Count; Index++)
                        {
                            if (HighestPriority > WaitingThreads[Index].Thread.Priority)
                            {
                                HighestPriority = WaitingThreads[Index].Thread.Priority;

                                HighestPrioIndex = Index;
                            }
                        }

                        WaitingThreads[HighestPrioIndex].WaitEvent.Set();

                        WaitingThreads.RemoveAt(HighestPrioIndex);
                    }
                }
            }
        }

        private void AcquireCondVarValue()
        {
            if (!OwnsCondVarValue)
            {
                while (!Process.Memory.AcquireAddress(CondVarAddress))
                {
                    Thread.Yield();
                }

                OwnsCondVarValue = true;
            }
        }

        private void ReleaseCondVarValue()
        {
            if (OwnsCondVarValue)
            {
                OwnsCondVarValue = false;

                Process.Memory.ReleaseAddress(CondVarAddress);
            }
        }
    }
}