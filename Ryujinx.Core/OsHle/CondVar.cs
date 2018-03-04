using Ryujinx.Core.OsHle.Handles;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle
{
    public class CondVar
    {
        private Process Process;

        private long CondVarAddress;
        private long Timeout;

        private bool OwnsCondVarValue;

        private List<HThread> WaitingThreads;

        public CondVar(Process Process, long CondVarAddress, long Timeout)
        {
            this.Process        = Process;
            this.CondVarAddress = CondVarAddress;
            this.Timeout        = Timeout;

            WaitingThreads = new List<HThread>();
        }

        public bool WaitForSignal(HThread Thread)
        {
            int Count = Process.Memory.ReadInt32(CondVarAddress);

            if (Count <= 0)
            {
                lock (WaitingThreads)
                {
                    WaitingThreads.Add(Thread);
                }

                if (Timeout == -1)
                {
                    Process.Scheduler.WaitForSignal(Thread);
                }
                else
                {
                    bool Result = Process.Scheduler.WaitForSignal(Thread, (int)(Timeout / 1000000));

                    lock (WaitingThreads)
                    {
                        WaitingThreads.Remove(Thread);
                    }

                    return Result;
                }
            }

            AcquireCondVarValue();

            Count = Process.Memory.ReadInt32(CondVarAddress);

            if (Count > 0)
            {
                Process.Memory.WriteInt32(CondVarAddress, Count - 1);
            }

            ReleaseCondVarValue();

            return true;
        }

        public void SetSignal(HThread Thread, int Count)
        {
            lock (WaitingThreads)
            {
                if (Count == -1)
                {
                    Process.Scheduler.Signal(WaitingThreads.ToArray());

                    AcquireCondVarValue();

                    Process.Memory.WriteInt32(CondVarAddress, WaitingThreads.Count);

                    ReleaseCondVarValue();

                    WaitingThreads.Clear();
                }
                else
                {
                    if (WaitingThreads.Count > 0)
                    {
                        int HighestPriority  = WaitingThreads[0].Priority;
                        int HighestPrioIndex = 0;

                        for (int Index = 1; Index < WaitingThreads.Count; Index++)
                        {
                            if (HighestPriority > WaitingThreads[Index].Priority)
                            {
                                HighestPriority = WaitingThreads[Index].Priority;

                                HighestPrioIndex = Index;
                            }
                        }

                        Process.Scheduler.Signal(WaitingThreads[HighestPrioIndex]);

                        WaitingThreads.RemoveAt(HighestPrioIndex);
                    }

                    AcquireCondVarValue();

                    Process.Memory.WriteInt32(CondVarAddress, Count);

                    ReleaseCondVarValue();
                }
            }

            Process.Scheduler.Suspend(Thread.ProcessorId);
            Process.Scheduler.Resume(Thread);
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