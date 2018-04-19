using Ryujinx.Core.OsHle.Handles;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle.Kernel
{
    class MutualExclusion
    {
        private const int MutexHasListenersMask = 0x40000000;

        private Process Process;

        private long MutexAddress;

        private List<(KThread Thread, AutoResetEvent WaitEvent)> WaitingThreads;

        public MutualExclusion(Process Process, long MutexAddress)
        {
            this.Process      = Process;
            this.MutexAddress = MutexAddress;

            WaitingThreads = new List<(KThread, AutoResetEvent)>();
        }

        public void WaitForLock(KThread RequestingThread)
        {
            int OwnerThreadHandle = Process.Memory.ReadInt32(MutexAddress) & ~MutexHasListenersMask;

            WaitForLock(RequestingThread, OwnerThreadHandle);
        }

        public void WaitForLock(KThread RequestingThread, int OwnerThreadHandle)
        {
            if (OwnerThreadHandle == RequestingThread.Handle ||
                OwnerThreadHandle == 0)
            {
                return;
            }

            using (AutoResetEvent WaitEvent = new AutoResetEvent(false))
            {
                lock (WaitingThreads)
                {
                    WaitingThreads.Add((RequestingThread, WaitEvent));
                }

                Process.Scheduler.Suspend(RequestingThread.ProcessorId);

                WaitEvent.WaitOne();

                Process.Scheduler.Resume(RequestingThread);
            }
        }

        public void Unlock()
        {
            lock (WaitingThreads)
            {
                int HasListeners = WaitingThreads.Count > 1 ? MutexHasListenersMask : 0;

                if (WaitingThreads.Count > 0)
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

                    int Handle = WaitingThreads[HighestPrioIndex].Thread.Handle;

                    WaitingThreads[HighestPrioIndex].WaitEvent.Set();

                    WaitingThreads.RemoveAt(HighestPrioIndex);

                    Process.Memory.WriteInt32(MutexAddress, HasListeners | Handle);
                }
                else
                {
                    Process.Memory.WriteInt32(MutexAddress, 0);
                }
            }
        }
    }
}