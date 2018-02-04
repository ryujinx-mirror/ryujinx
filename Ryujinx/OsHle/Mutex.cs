using ChocolArm64;
using ChocolArm64.Memory;
using System.Threading;

namespace Ryujinx.OsHle
{
    class Mutex
    {
        private const int MutexHasListenersMask = 0x40000000;

        private AMemory Memory;

        private long MutexAddress;

        private int CurrRequestingThreadHandle;

        private int HighestPriority;

        private ManualResetEvent ThreadEvent;

        private object EnterWaitLock;

        public Mutex(AMemory Memory, long MutexAddress)
        {
            this.Memory       = Memory;
            this.MutexAddress = MutexAddress;

            ThreadEvent = new ManualResetEvent(false);

            EnterWaitLock = new object();
        }

        public void WaitForLock(AThread RequestingThread, int RequestingThreadHandle)
        {
            lock (EnterWaitLock)
            {               
                int CurrentThreadHandle = Memory.ReadInt32(MutexAddress) & ~MutexHasListenersMask;

                if (CurrentThreadHandle == RequestingThreadHandle ||
                    CurrentThreadHandle == 0)
                {
                    return;
                }

                if (CurrRequestingThreadHandle == 0 || RequestingThread.Priority < HighestPriority)
                {
                    CurrRequestingThreadHandle = RequestingThreadHandle;

                    HighestPriority = RequestingThread.Priority;
                }
            }

            ThreadEvent.Reset();
            ThreadEvent.WaitOne();
        }

        public void GiveUpLock(int ThreadHandle)
        {
            lock (EnterWaitLock)
            {
                int CurrentThread = Memory.ReadInt32(MutexAddress) & ~MutexHasListenersMask;

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
                if (CurrRequestingThreadHandle != 0)
                {
                    Memory.WriteInt32(MutexAddress, CurrRequestingThreadHandle);
                }
                else
                {
                    Memory.WriteInt32(MutexAddress, 0);
                }

                CurrRequestingThreadHandle = 0;

                ThreadEvent.Set();
            }
        }
    }
}