using Ryujinx.Core.OsHle.Handles;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Core.OsHle
{
    public class Mutex
    {
        private const int MutexHasListenersMask = 0x40000000;

        private Process Process;

        private long MutexAddress;

        private bool OwnsMutexValue;

        private object EnterWaitLock;

        private ConcurrentQueue<HThread> WaitingThreads;

        public Mutex(Process Process, long MutexAddress, int OwnerThreadHandle)
        {
            this.Process      = Process;
            this.MutexAddress = MutexAddress;

            //Process.Memory.WriteInt32(MutexAddress, OwnerThreadHandle);

            EnterWaitLock = new object();

            WaitingThreads = new ConcurrentQueue<HThread>();
        }

        public void WaitForLock(HThread RequestingThread, int RequestingThreadHandle)
        {
            AcquireMutexValue();

            lock (EnterWaitLock)
            {
                int CurrentThreadHandle = Process.Memory.ReadInt32(MutexAddress) & ~MutexHasListenersMask;

                if (CurrentThreadHandle == RequestingThreadHandle ||
                    CurrentThreadHandle == 0)
                {
                    return;
                }

                Process.Memory.WriteInt32(MutexAddress, CurrentThreadHandle | MutexHasListenersMask);

                ReleaseMutexValue();

                WaitingThreads.Enqueue(RequestingThread);
            }

            Process.Scheduler.WaitForSignal(RequestingThread);
        }

        public void GiveUpLock(int ThreadHandle)
        {
            AcquireMutexValue();

            lock (EnterWaitLock)
            {
                int CurrentThread = Process.Memory.ReadInt32(MutexAddress) & ~MutexHasListenersMask;

                if (CurrentThread == ThreadHandle)
                {
                    Unlock();
                }
            }

            ReleaseMutexValue();
        }

        public void Unlock()
        {
            AcquireMutexValue();

            lock (EnterWaitLock)
            {
                int HasListeners = WaitingThreads.Count > 1 ? MutexHasListenersMask : 0;

                Process.Memory.WriteInt32(MutexAddress, HasListeners);

                ReleaseMutexValue();

                HThread[] UnlockedThreads = new HThread[WaitingThreads.Count];

                int Index = 0;

                while (WaitingThreads.TryDequeue(out HThread Thread))
                {
                    UnlockedThreads[Index++] = Thread;
                }

                Process.Scheduler.Signal(UnlockedThreads);
            }
        }

        private void AcquireMutexValue()
        {
            if (!OwnsMutexValue)
            {
                while (!Process.Memory.AcquireAddress(MutexAddress))
                {
                    Thread.Yield();
                }

                OwnsMutexValue = true;
            }
        }

        private void ReleaseMutexValue()
        {
            if (OwnsMutexValue)
            {
                OwnsMutexValue = false;

                Process.Memory.ReleaseAddress(MutexAddress);
            }
        }
    }
}