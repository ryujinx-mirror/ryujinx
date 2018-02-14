using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.OsHle
{
    class Mutex
    {
        private const int MutexHasListenersMask = 0x40000000;

        private Process Process;

        private long MutexAddress;

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
            lock (EnterWaitLock)
            {
                int CurrentThreadHandle = ReadMutexValue() & ~MutexHasListenersMask;

                if (CurrentThreadHandle == RequestingThreadHandle ||
                    CurrentThreadHandle == 0)
                {
                    return;
                }

                WriteMutexValue(CurrentThreadHandle | MutexHasListenersMask);

                WaitingThreads.Enqueue(RequestingThread);
            }

            Process.Scheduler.WaitForSignal(RequestingThread);
        }

        public void GiveUpLock(int ThreadHandle)
        {
            lock (EnterWaitLock)
            {
                int CurrentThread = ReadMutexValue() & ~MutexHasListenersMask;

                if (CurrentThread == ThreadHandle)
                {
                    Unlock();
                }
            }
        }

        public void Unlock()
        {
            lock (EnterWaitLock)
            {
                int HasListeners = WaitingThreads.Count > 1 ? MutexHasListenersMask : 0;

                WriteMutexValue(HasListeners);

                HThread[] UnlockedThreads = new HThread[WaitingThreads.Count];

                int Index = 0;

                while (WaitingThreads.TryDequeue(out HThread Thread))
                {
                    UnlockedThreads[Index++] = Thread;
                }

                Process.Scheduler.Signal(UnlockedThreads);
            }
        }

        private int ReadMutexValue()
        {
            return AMemoryHelper.ReadInt32Exclusive(Process.Memory, MutexAddress);
        }

        private void WriteMutexValue(int Value)
        {
            AMemoryHelper.WriteInt32Exclusive(Process.Memory, MutexAddress, Value);
        }
    }
}