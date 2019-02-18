using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KAddressArbiter
    {
        private const int HasListenersMask = 0x40000000;

        private Horizon _system;

        public List<KThread> CondVarThreads;
        public List<KThread> ArbiterThreads;

        public KAddressArbiter(Horizon system)
        {
            _system = system;

            CondVarThreads = new List<KThread>();
            ArbiterThreads = new List<KThread>();
        }

        public KernelResult ArbitrateLock(int ownerHandle, ulong mutexAddress, int requesterHandle)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.Success;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(_system, mutexAddress, out int mutexValue))
            {
                _system.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (mutexValue != (ownerHandle | HasListenersMask))
            {
                _system.CriticalSection.Leave();

                return 0;
            }

            KThread mutexOwner = currentProcess.HandleTable.GetObject<KThread>(ownerHandle);

            if (mutexOwner == null)
            {
                _system.CriticalSection.Leave();

                return KernelResult.InvalidHandle;
            }

            currentThread.MutexAddress             = mutexAddress;
            currentThread.ThreadHandleForUserMutex = requesterHandle;

            mutexOwner.AddMutexWaiter(currentThread);

            currentThread.Reschedule(ThreadSchedState.Paused);

            _system.CriticalSection.Leave();
            _system.CriticalSection.Enter();

            if (currentThread.MutexOwner != null)
            {
                currentThread.MutexOwner.RemoveMutexWaiter(currentThread);
            }

            _system.CriticalSection.Leave();

            return (KernelResult)currentThread.ObjSyncResult;
        }

        public KernelResult ArbitrateUnlock(ulong mutexAddress)
        {
            _system.CriticalSection.Enter();

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            (KernelResult result, KThread newOwnerThread) = MutexUnlock(currentThread, mutexAddress);

            if (result != KernelResult.Success && newOwnerThread != null)
            {
                newOwnerThread.SignaledObj   = null;
                newOwnerThread.ObjSyncResult = result;
            }

            _system.CriticalSection.Leave();

            return result;
        }

        public KernelResult WaitProcessWideKeyAtomic(
            ulong mutexAddress,
            ulong condVarAddress,
            int   threadHandle,
            long  timeout)
        {
            _system.CriticalSection.Enter();

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            (KernelResult result, _) = MutexUnlock(currentThread, mutexAddress);

            if (result != KernelResult.Success)
            {
                _system.CriticalSection.Leave();

                return result;
            }

            currentThread.MutexAddress             = mutexAddress;
            currentThread.ThreadHandleForUserMutex = threadHandle;
            currentThread.CondVarAddress           = condVarAddress;

            CondVarThreads.Add(currentThread);

            if (timeout != 0)
            {
                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _system.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }
            }

            _system.CriticalSection.Leave();

            if (timeout > 0)
            {
                _system.TimeManager.UnscheduleFutureInvocation(currentThread);
            }

            _system.CriticalSection.Enter();

            if (currentThread.MutexOwner != null)
            {
                currentThread.MutexOwner.RemoveMutexWaiter(currentThread);
            }

            CondVarThreads.Remove(currentThread);

            _system.CriticalSection.Leave();

            return (KernelResult)currentThread.ObjSyncResult;
        }

        private (KernelResult, KThread) MutexUnlock(KThread currentThread, ulong mutexAddress)
        {
            KThread newOwnerThread = currentThread.RelinquishMutex(mutexAddress, out int count);

            int mutexValue = 0;

            if (newOwnerThread != null)
            {
                mutexValue = newOwnerThread.ThreadHandleForUserMutex;

                if (count >= 2)
                {
                    mutexValue |= HasListenersMask;
                }

                newOwnerThread.SignaledObj   = null;
                newOwnerThread.ObjSyncResult = KernelResult.Success;

                newOwnerThread.ReleaseAndResume();
            }

            KernelResult result = KernelResult.Success;

            if (!KernelTransfer.KernelToUserInt32(_system, mutexAddress, mutexValue))
            {
                result = KernelResult.InvalidMemState;
            }

            return (result, newOwnerThread);
        }

        public void SignalProcessWideKey(ulong address, int count)
        {
            Queue<KThread> signaledThreads = new Queue<KThread>();

            _system.CriticalSection.Enter();

            IOrderedEnumerable<KThread> sortedThreads = CondVarThreads.OrderBy(x => x.DynamicPriority);

            foreach (KThread thread in sortedThreads.Where(x => x.CondVarAddress == address))
            {
                TryAcquireMutex(thread);

                signaledThreads.Enqueue(thread);

                //If the count is <= 0, we should signal all threads waiting.
                if (count >= 1 && --count == 0)
                {
                    break;
                }
            }

            while (signaledThreads.TryDequeue(out KThread thread))
            {
                CondVarThreads.Remove(thread);
            }

            _system.CriticalSection.Leave();
        }

        private KThread TryAcquireMutex(KThread requester)
        {
            ulong address = requester.MutexAddress;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            int mutexValue, newMutexValue;

            do
            {
                if (!KernelTransfer.UserToKernelInt32(_system, address, out mutexValue))
                {
                    //Invalid address.
                    requester.SignaledObj   = null;
                    requester.ObjSyncResult = KernelResult.InvalidMemState;

                    return null;
                }

                if (mutexValue != 0)
                {
                    //Update value to indicate there is a mutex waiter now.
                    newMutexValue = mutexValue | HasListenersMask;
                }
                else
                {
                    //No thread owning the mutex, assign to requesting thread.
                    newMutexValue = requester.ThreadHandleForUserMutex;
                }
            }
            while (!currentProcess.CpuMemory.AtomicCompareExchangeInt32((long)address, mutexValue, newMutexValue));

            if (mutexValue == 0)
            {
                //We now own the mutex.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = KernelResult.Success;

                requester.ReleaseAndResume();

                return null;
            }

            mutexValue &= ~HasListenersMask;

            KThread mutexOwner = currentProcess.HandleTable.GetObject<KThread>(mutexValue);

            if (mutexOwner != null)
            {
                //Mutex already belongs to another thread, wait for it.
                mutexOwner.AddMutexWaiter(requester);
            }
            else
            {
                //Invalid mutex owner.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = KernelResult.InvalidHandle;

                requester.ReleaseAndResume();
            }

            return mutexOwner;
        }

        public KernelResult WaitForAddressIfEqual(ulong address, int value, long timeout)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (currentValue == value)
            {
                if (timeout == 0)
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.TimedOut;
                }

                currentThread.MutexAddress         = address;
                currentThread.WaitingInArbitration = true;

                InsertSortedByPriority(ArbiterThreads, currentThread);

                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _system.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                _system.CriticalSection.Leave();

                if (timeout > 0)
                {
                    _system.TimeManager.UnscheduleFutureInvocation(currentThread);
                }

                _system.CriticalSection.Enter();

                if (currentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(currentThread);

                    currentThread.WaitingInArbitration = false;
                }

                _system.CriticalSection.Leave();

                return (KernelResult)currentThread.ObjSyncResult;
            }

            _system.CriticalSection.Leave();

            return KernelResult.InvalidState;
        }

        public KernelResult WaitForAddressIfLessThan(
            ulong address,
            int   value,
            bool  shouldDecrement,
            long  timeout)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (shouldDecrement)
            {
                currentValue = currentProcess.CpuMemory.AtomicDecrementInt32((long)address) + 1;
            }

            if (currentValue < value)
            {
                if (timeout == 0)
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.TimedOut;
                }

                currentThread.MutexAddress         = address;
                currentThread.WaitingInArbitration = true;

                InsertSortedByPriority(ArbiterThreads, currentThread);

                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _system.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                _system.CriticalSection.Leave();

                if (timeout > 0)
                {
                    _system.TimeManager.UnscheduleFutureInvocation(currentThread);
                }

                _system.CriticalSection.Enter();

                if (currentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(currentThread);

                    currentThread.WaitingInArbitration = false;
                }

                _system.CriticalSection.Leave();

                return (KernelResult)currentThread.ObjSyncResult;
            }

            _system.CriticalSection.Leave();

            return KernelResult.InvalidState;
        }

        private void InsertSortedByPriority(List<KThread> threads, KThread thread)
        {
            int nextIndex = -1;

            for (int index = 0; index < threads.Count; index++)
            {
                if (threads[index].DynamicPriority > thread.DynamicPriority)
                {
                    nextIndex = index;

                    break;
                }
            }

            if (nextIndex != -1)
            {
                threads.Insert(nextIndex, thread);
            }
            else
            {
                threads.Add(thread);
            }
        }

        public KernelResult Signal(ulong address, int count)
        {
            _system.CriticalSection.Enter();

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return KernelResult.Success;
        }

        public KernelResult SignalAndIncrementIfEqual(ulong address, int value, int count)
        {
            _system.CriticalSection.Enter();

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            int currentValue;

            do
            {
                if (!KernelTransfer.UserToKernelInt32(_system, address, out currentValue))
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.InvalidMemState;
                }

                if (currentValue != value)
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.InvalidState;
                }
            }
            while (!currentProcess.CpuMemory.AtomicCompareExchangeInt32((long)address, currentValue, currentValue + 1));

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return KernelResult.Success;
        }

        public KernelResult SignalAndModifyIfEqual(ulong address, int value, int count)
        {
            _system.CriticalSection.Enter();

            int offset;

            //The value is decremented if the number of threads waiting is less
            //or equal to the Count of threads to be signaled, or Count is zero
            //or negative. It is incremented if there are no threads waiting.
            int waitingCount = 0;

            foreach (KThread thread in ArbiterThreads.Where(x => x.MutexAddress == address))
            {
                if (++waitingCount > count)
                {
                    break;
                }
            }

            if (waitingCount > 0)
            {
                offset = waitingCount <= count || count <= 0 ? -1 : 0;
            }
            else
            {
                offset = 1;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            int currentValue;

            do
            {
                if (!KernelTransfer.UserToKernelInt32(_system, address, out currentValue))
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.InvalidMemState;
                }

                if (currentValue != value)
                {
                    _system.CriticalSection.Leave();

                    return KernelResult.InvalidState;
                }
            }
            while (!currentProcess.CpuMemory.AtomicCompareExchangeInt32((long)address, currentValue, currentValue + offset));

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return KernelResult.Success;
        }

        private void WakeArbiterThreads(ulong address, int count)
        {
            Queue<KThread> signaledThreads = new Queue<KThread>();

            foreach (KThread thread in ArbiterThreads.Where(x => x.MutexAddress == address))
            {
                signaledThreads.Enqueue(thread);

                //If the count is <= 0, we should signal all threads waiting.
                if (count >= 1 && --count == 0)
                {
                    break;
                }
            }

            while (signaledThreads.TryDequeue(out KThread thread))
            {
                thread.SignaledObj   = null;
                thread.ObjSyncResult = KernelResult.Success;

                thread.ReleaseAndResume();

                thread.WaitingInArbitration = false;

                ArbiterThreads.Remove(thread);
            }
        }
    }
}
