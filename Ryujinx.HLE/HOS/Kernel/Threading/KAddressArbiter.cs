using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KAddressArbiter
    {
        private const int HasListenersMask = 0x40000000;

        private readonly KernelContext _context;

        private readonly List<KThread> _condVarThreads;
        private readonly List<KThread> _arbiterThreads;

        public KAddressArbiter(KernelContext context)
        {
            _context = context;

            _condVarThreads = new List<KThread>();
            _arbiterThreads = new List<KThread>();
        }

        public KernelResult ArbitrateLock(int ownerHandle, ulong mutexAddress, int requesterHandle)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            _context.CriticalSection.Enter();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.Success;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(_context, mutexAddress, out int mutexValue))
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (mutexValue != (ownerHandle | HasListenersMask))
            {
                _context.CriticalSection.Leave();

                return 0;
            }

            KThread mutexOwner = currentProcess.HandleTable.GetObject<KThread>(ownerHandle);

            if (mutexOwner == null)
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidHandle;
            }

            currentThread.MutexAddress             = mutexAddress;
            currentThread.ThreadHandleForUserMutex = requesterHandle;

            mutexOwner.AddMutexWaiter(currentThread);

            currentThread.Reschedule(ThreadSchedState.Paused);

            _context.CriticalSection.Leave();
            _context.CriticalSection.Enter();

            if (currentThread.MutexOwner != null)
            {
                currentThread.MutexOwner.RemoveMutexWaiter(currentThread);
            }

            _context.CriticalSection.Leave();

            return currentThread.ObjSyncResult;
        }

        public KernelResult ArbitrateUnlock(ulong mutexAddress)
        {
            _context.CriticalSection.Enter();

            KThread currentThread = KernelStatic.GetCurrentThread();

            (KernelResult result, KThread newOwnerThread) = MutexUnlock(currentThread, mutexAddress);

            if (result != KernelResult.Success && newOwnerThread != null)
            {
                newOwnerThread.SignaledObj   = null;
                newOwnerThread.ObjSyncResult = result;
            }

            _context.CriticalSection.Leave();

            return result;
        }

        public KernelResult WaitProcessWideKeyAtomic(
            ulong mutexAddress,
            ulong condVarAddress,
            int   threadHandle,
            long  timeout)
        {
            _context.CriticalSection.Enter();

            KThread currentThread = KernelStatic.GetCurrentThread();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _context.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            (KernelResult result, _) = MutexUnlock(currentThread, mutexAddress);

            if (result != KernelResult.Success)
            {
                _context.CriticalSection.Leave();

                return result;
            }

            currentThread.MutexAddress             = mutexAddress;
            currentThread.ThreadHandleForUserMutex = threadHandle;
            currentThread.CondVarAddress           = condVarAddress;

            _condVarThreads.Add(currentThread);

            if (timeout != 0)
            {
                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _context.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }
            }

            _context.CriticalSection.Leave();

            if (timeout > 0)
            {
                _context.TimeManager.UnscheduleFutureInvocation(currentThread);
            }

            _context.CriticalSection.Enter();

            if (currentThread.MutexOwner != null)
            {
                currentThread.MutexOwner.RemoveMutexWaiter(currentThread);
            }

            _condVarThreads.Remove(currentThread);

            _context.CriticalSection.Leave();

            return currentThread.ObjSyncResult;
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

            if (!KernelTransfer.KernelToUserInt32(_context, mutexAddress, mutexValue))
            {
                result = KernelResult.InvalidMemState;
            }

            return (result, newOwnerThread);
        }

        public void SignalProcessWideKey(ulong address, int count)
        {
            Queue<KThread> signaledThreads = new Queue<KThread>();

            _context.CriticalSection.Enter();

            IOrderedEnumerable<KThread> sortedThreads = _condVarThreads.OrderBy(x => x.DynamicPriority);

            foreach (KThread thread in sortedThreads.Where(x => x.CondVarAddress == address))
            {
                TryAcquireMutex(thread);

                signaledThreads.Enqueue(thread);

                // If the count is <= 0, we should signal all threads waiting.
                if (count >= 1 && --count == 0)
                {
                    break;
                }
            }

            while (signaledThreads.TryDequeue(out KThread thread))
            {
                _condVarThreads.Remove(thread);
            }

            _context.CriticalSection.Leave();
        }

        private KThread TryAcquireMutex(KThread requester)
        {
            ulong address = requester.MutexAddress;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.CpuMemory.IsMapped(address))
            {
                // Invalid address.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = KernelResult.InvalidMemState;

                return null;
            }

            ref int mutexRef = ref currentProcess.CpuMemory.GetRef<int>(address);

            int mutexValue, newMutexValue;

            do
            {
                mutexValue = mutexRef;

                if (mutexValue != 0)
                {
                    // Update value to indicate there is a mutex waiter now.
                    newMutexValue = mutexValue | HasListenersMask;
                }
                else
                {
                    // No thread owning the mutex, assign to requesting thread.
                    newMutexValue = requester.ThreadHandleForUserMutex;
                }
            }
            while (Interlocked.CompareExchange(ref mutexRef, newMutexValue, mutexValue) != mutexValue);

            if (mutexValue == 0)
            {
                // We now own the mutex.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = KernelResult.Success;

                requester.ReleaseAndResume();

                return null;
            }

            mutexValue &= ~HasListenersMask;

            KThread mutexOwner = currentProcess.HandleTable.GetObject<KThread>(mutexValue);

            if (mutexOwner != null)
            {
                // Mutex already belongs to another thread, wait for it.
                mutexOwner.AddMutexWaiter(requester);
            }
            else
            {
                // Invalid mutex owner.
                requester.SignaledObj   = null;
                requester.ObjSyncResult = KernelResult.InvalidHandle;

                requester.ReleaseAndResume();
            }

            return mutexOwner;
        }

        public KernelResult WaitForAddressIfEqual(ulong address, int value, long timeout)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            _context.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _context.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            if (!KernelTransfer.UserToKernelInt32(_context, address, out int currentValue))
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (currentValue == value)
            {
                if (timeout == 0)
                {
                    _context.CriticalSection.Leave();

                    return KernelResult.TimedOut;
                }

                currentThread.MutexAddress         = address;
                currentThread.WaitingInArbitration = true;

                InsertSortedByPriority(_arbiterThreads, currentThread);

                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _context.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                _context.CriticalSection.Leave();

                if (timeout > 0)
                {
                    _context.TimeManager.UnscheduleFutureInvocation(currentThread);
                }

                _context.CriticalSection.Enter();

                if (currentThread.WaitingInArbitration)
                {
                    _arbiterThreads.Remove(currentThread);

                    currentThread.WaitingInArbitration = false;
                }

                _context.CriticalSection.Leave();

                return currentThread.ObjSyncResult;
            }

            _context.CriticalSection.Leave();

            return KernelResult.InvalidState;
        }

        public KernelResult WaitForAddressIfLessThan(
            ulong address,
            int   value,
            bool  shouldDecrement,
            long  timeout)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            _context.CriticalSection.Enter();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                _context.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.TimedOut;

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(_context, address, out int currentValue))
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            if (shouldDecrement)
            {
                currentValue = Interlocked.Decrement(ref currentProcess.CpuMemory.GetRef<int>(address)) + 1;
            }

            if (currentValue < value)
            {
                if (timeout == 0)
                {
                    _context.CriticalSection.Leave();

                    return KernelResult.TimedOut;
                }

                currentThread.MutexAddress         = address;
                currentThread.WaitingInArbitration = true;

                InsertSortedByPriority(_arbiterThreads, currentThread);

                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _context.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                _context.CriticalSection.Leave();

                if (timeout > 0)
                {
                    _context.TimeManager.UnscheduleFutureInvocation(currentThread);
                }

                _context.CriticalSection.Enter();

                if (currentThread.WaitingInArbitration)
                {
                    _arbiterThreads.Remove(currentThread);

                    currentThread.WaitingInArbitration = false;
                }

                _context.CriticalSection.Leave();

                return currentThread.ObjSyncResult;
            }

            _context.CriticalSection.Leave();

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
            _context.CriticalSection.Enter();

            WakeArbiterThreads(address, count);

            _context.CriticalSection.Leave();

            return KernelResult.Success;
        }

        public KernelResult SignalAndIncrementIfEqual(ulong address, int value, int count)
        {
            _context.CriticalSection.Enter();

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.CpuMemory.IsMapped(address))
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            ref int valueRef = ref currentProcess.CpuMemory.GetRef<int>(address);

            int currentValue;

            do
            {
                currentValue = valueRef;

                if (currentValue != value)
                {
                    _context.CriticalSection.Leave();

                    return KernelResult.InvalidState;
                }
            }
            while (Interlocked.CompareExchange(ref valueRef, currentValue + 1, currentValue) != currentValue);

            WakeArbiterThreads(address, count);

            _context.CriticalSection.Leave();

            return KernelResult.Success;
        }

        public KernelResult SignalAndModifyIfEqual(ulong address, int value, int count)
        {
            _context.CriticalSection.Enter();

            int offset;

            // The value is decremented if the number of threads waiting is less
            // or equal to the Count of threads to be signaled, or Count is zero
            // or negative. It is incremented if there are no threads waiting.
            int waitingCount = 0;

            foreach (KThread thread in _arbiterThreads.Where(x => x.MutexAddress == address))
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

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (!currentProcess.CpuMemory.IsMapped(address))
            {
                _context.CriticalSection.Leave();

                return KernelResult.InvalidMemState;
            }

            ref int valueRef = ref currentProcess.CpuMemory.GetRef<int>(address);

            int currentValue;

            do
            {
                currentValue = valueRef;

                if (currentValue != value)
                {
                    _context.CriticalSection.Leave();

                    return KernelResult.InvalidState;
                }
            }
            while (Interlocked.CompareExchange(ref valueRef, currentValue + offset, currentValue) != currentValue);

            WakeArbiterThreads(address, count);

            _context.CriticalSection.Leave();

            return KernelResult.Success;
        }

        private void WakeArbiterThreads(ulong address, int count)
        {
            Queue<KThread> signaledThreads = new Queue<KThread>();

            foreach (KThread thread in _arbiterThreads.Where(x => x.MutexAddress == address))
            {
                signaledThreads.Enqueue(thread);

                // If the count is <= 0, we should signal all threads waiting.
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

                _arbiterThreads.Remove(thread);
            }
        }
    }
}
