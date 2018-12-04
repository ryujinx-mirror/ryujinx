using System.Collections.Generic;
using System.Linq;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
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

        public long ArbitrateLock(int ownerHandle, long mutexAddress, int requesterHandle)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(_system, mutexAddress, out int mutexValue))
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
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

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
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

            return (uint)currentThread.ObjSyncResult;
        }

        public long ArbitrateUnlock(long mutexAddress)
        {
            _system.CriticalSection.Enter();

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            (long result, KThread newOwnerThread) = MutexUnlock(currentThread, mutexAddress);

            if (result != 0 && newOwnerThread != null)
            {
                newOwnerThread.SignaledObj   = null;
                newOwnerThread.ObjSyncResult = (int)result;
            }

            _system.CriticalSection.Leave();

            return result;
        }

        public long WaitProcessWideKeyAtomic(
            long mutexAddress,
            long condVarAddress,
            int  threadHandle,
            long timeout)
        {
            _system.CriticalSection.Enter();

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            (long result, _) = MutexUnlock(currentThread, mutexAddress);

            if (result != 0)
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

            return (uint)currentThread.ObjSyncResult;
        }

        private (long, KThread) MutexUnlock(KThread currentThread, long mutexAddress)
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
                newOwnerThread.ObjSyncResult = 0;

                newOwnerThread.ReleaseAndResume();
            }

            long result = 0;

            if (!KernelTransfer.KernelToUserInt32(_system, mutexAddress, mutexValue))
            {
                result = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            return (result, newOwnerThread);
        }

        public void SignalProcessWideKey(long address, int count)
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
            long address = requester.MutexAddress;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            currentProcess.CpuMemory.SetExclusive(0, address);

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int mutexValue))
            {
                //Invalid address.
                currentProcess.CpuMemory.ClearExclusive(0);

                requester.SignaledObj   = null;
                requester.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return null;
            }

            while (true)
            {
                if (currentProcess.CpuMemory.TestExclusive(0, address))
                {
                    if (mutexValue != 0)
                    {
                        //Update value to indicate there is a mutex waiter now.
                        currentProcess.CpuMemory.WriteInt32(address, mutexValue | HasListenersMask);
                    }
                    else
                    {
                        //No thread owning the mutex, assign to requesting thread.
                        currentProcess.CpuMemory.WriteInt32(address, requester.ThreadHandleForUserMutex);
                    }

                    currentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                currentProcess.CpuMemory.SetExclusive(0, address);

                mutexValue = currentProcess.CpuMemory.ReadInt32(address);
            }

            if (mutexValue == 0)
            {
                //We now own the mutex.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = 0;

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
                requester.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                requester.ReleaseAndResume();
            }

            return mutexOwner;
        }

        public long WaitForAddressIfEqual(long address, int value, long timeout)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (currentValue == value)
            {
                if (timeout == 0)
                {
                    _system.CriticalSection.Leave();

                    return MakeError(ErrorModule.Kernel, KernelErr.Timeout);
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

                return currentThread.ObjSyncResult;
            }

            _system.CriticalSection.Leave();

            return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
        }

        public long WaitForAddressIfLessThan(long address, int value, bool shouldDecrement, long timeout)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            //If ShouldDecrement is true, do atomic decrement of the value at Address.
            currentProcess.CpuMemory.SetExclusive(0, address);

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (shouldDecrement)
            {
                while (currentValue < value)
                {
                    if (currentProcess.CpuMemory.TestExclusive(0, address))
                    {
                        currentProcess.CpuMemory.WriteInt32(address, currentValue - 1);

                        currentProcess.CpuMemory.ClearExclusiveForStore(0);

                        break;
                    }

                    currentProcess.CpuMemory.SetExclusive(0, address);

                    currentValue = currentProcess.CpuMemory.ReadInt32(address);
                }
            }

            currentProcess.CpuMemory.ClearExclusive(0);

            if (currentValue < value)
            {
                if (timeout == 0)
                {
                    _system.CriticalSection.Leave();

                    return MakeError(ErrorModule.Kernel, KernelErr.Timeout);
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

                return currentThread.ObjSyncResult;
            }

            _system.CriticalSection.Leave();

            return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
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

        public long Signal(long address, int count)
        {
            _system.CriticalSection.Enter();

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return 0;
        }

        public long SignalAndIncrementIfEqual(long address, int value, int count)
        {
            _system.CriticalSection.Enter();

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            currentProcess.CpuMemory.SetExclusive(0, address);

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (currentValue == value)
            {
                if (currentProcess.CpuMemory.TestExclusive(0, address))
                {
                    currentProcess.CpuMemory.WriteInt32(address, currentValue + 1);

                    currentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                currentProcess.CpuMemory.SetExclusive(0, address);

                currentValue = currentProcess.CpuMemory.ReadInt32(address);
            }

            currentProcess.CpuMemory.ClearExclusive(0);

            if (currentValue != value)
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return 0;
        }

        public long SignalAndModifyIfEqual(long address, int value, int count)
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

            currentProcess.CpuMemory.SetExclusive(0, address);

            if (!KernelTransfer.UserToKernelInt32(_system, address, out int currentValue))
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (currentValue == value)
            {
                if (currentProcess.CpuMemory.TestExclusive(0, address))
                {
                    currentProcess.CpuMemory.WriteInt32(address, currentValue + offset);

                    currentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                currentProcess.CpuMemory.SetExclusive(0, address);

                currentValue = currentProcess.CpuMemory.ReadInt32(address);
            }

            currentProcess.CpuMemory.ClearExclusive(0);

            if (currentValue != value)
            {
                _system.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(address, count);

            _system.CriticalSection.Leave();

            return 0;
        }

        private void WakeArbiterThreads(long address, int count)
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
                thread.ObjSyncResult = 0;

                thread.ReleaseAndResume();

                thread.WaitingInArbitration = false;

                ArbiterThreads.Remove(thread);
            }
        }
    }
}
