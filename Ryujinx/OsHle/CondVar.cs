using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using System.Collections.Generic;

namespace Ryujinx.OsHle
{
    class CondVar
    {
        private Process Process;

        private long CondVarAddress;
        private long Timeout;

        private List<HThread> WaitingThreads;

        public CondVar(Process Process, long CondVarAddress, long Timeout)
        {
            this.Process        = Process;
            this.CondVarAddress = CondVarAddress;
            this.Timeout        = Timeout;

            WaitingThreads = new List<HThread>();
        }

        public void WaitForSignal(HThread Thread)
        {
            int Count = ReadCondVarValue();

            if (Count <= 0)
            {
                //FIXME: We shouldn't need to do that?
                Process.Scheduler.Yield(Thread);

                return;
            }

            WriteCondVarValue(Count - 1);

            lock (WaitingThreads)
            {
                WaitingThreads.Add(Thread);
            }

            if (Timeout != -1)
            {
                Process.Scheduler.WaitForSignal(Thread, (int)(Timeout / 1000000));
            }
            else
            {
                Process.Scheduler.WaitForSignal(Thread);
            }
        }

        public void SetSignal(int Count)
        {
            lock (WaitingThreads)
            {
                if (Count == -1)
                {
                    Process.Scheduler.Signal(WaitingThreads.ToArray());

                    WriteCondVarValue(WaitingThreads.Count);

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

                    WriteCondVarValue(Count);
                }
            }
        }

        private int ReadCondVarValue()
        {
            return AMemoryHelper.ReadInt32Exclusive(Process.Memory, CondVarAddress);
        }

        private void WriteCondVarValue(int Value)
        {
            AMemoryHelper.WriteInt32Exclusive(Process.Memory, CondVarAddress, Value);
        }
    }
}