using ChocolArm64.Memory;
using System.Collections.Generic;
using System.Linq;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KAddressArbiter
    {
        private const int HasListenersMask = 0x40000000;

        private Horizon System;

        public List<KThread> CondVarThreads;
        public List<KThread> ArbiterThreads;

        public KAddressArbiter(Horizon System)
        {
            this.System = System;

            CondVarThreads = new List<KThread>();
            ArbiterThreads = new List<KThread>();
        }

        public long ArbitrateLock(
            Process Process,
            AMemory Memory,
            int     OwnerHandle,
            long    MutexAddress,
            int     RequesterHandle)
        {
            System.CriticalSectionLock.Lock();

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = 0;

            if (!UserToKernelInt32(Memory, MutexAddress, out int MutexValue))
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);;
            }

            if (MutexValue != (OwnerHandle | HasListenersMask))
            {
                System.CriticalSectionLock.Unlock();

                return 0;
            }

            KThread MutexOwner = Process.HandleTable.GetObject<KThread>(OwnerHandle);

            if (MutexOwner == null)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }

            CurrentThread.MutexAddress             = MutexAddress;
            CurrentThread.ThreadHandleForUserMutex = RequesterHandle;

            MutexOwner.AddMutexWaiter(CurrentThread);

            CurrentThread.Reschedule(ThreadSchedState.Paused);

            System.CriticalSectionLock.Unlock();
            System.CriticalSectionLock.Lock();

            if (CurrentThread.MutexOwner != null)
            {
                CurrentThread.MutexOwner.RemoveMutexWaiter(CurrentThread);
            }

            System.CriticalSectionLock.Unlock();

            return (uint)CurrentThread.ObjSyncResult;
        }

        public long ArbitrateUnlock(AMemory Memory, long MutexAddress)
        {
            System.CriticalSectionLock.Lock();

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            (long Result, KThread NewOwnerThread) = MutexUnlock(Memory, CurrentThread, MutexAddress);

            if (Result != 0 && NewOwnerThread != null)
            {
                NewOwnerThread.SignaledObj   = null;
                NewOwnerThread.ObjSyncResult = (int)Result;
            }

            System.CriticalSectionLock.Unlock();

            return Result;
        }

        public long WaitProcessWideKeyAtomic(
            AMemory Memory,
            long    MutexAddress,
            long    CondVarAddress,
            int     ThreadHandle,
            long    Timeout)
        {
            System.CriticalSectionLock.Lock();

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            (long Result, _) = MutexUnlock(Memory, CurrentThread, MutexAddress);

            if (Result != 0)
            {
                System.CriticalSectionLock.Unlock();

                return Result;
            }

            CurrentThread.MutexAddress             = MutexAddress;
            CurrentThread.ThreadHandleForUserMutex = ThreadHandle;
            CurrentThread.CondVarAddress           = CondVarAddress;

            CondVarThreads.Add(CurrentThread);

            if (Timeout != 0)
            {
                CurrentThread.Reschedule(ThreadSchedState.Paused);

                if (Timeout > 0)
                {
                    System.TimeManager.ScheduleFutureInvocation(CurrentThread, Timeout);
                }
            }

            System.CriticalSectionLock.Unlock();

            if (Timeout > 0)
            {
                System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
            }

            System.CriticalSectionLock.Lock();

            if (CurrentThread.MutexOwner != null)
            {
                CurrentThread.MutexOwner.RemoveMutexWaiter(CurrentThread);
            }

            CondVarThreads.Remove(CurrentThread);

            System.CriticalSectionLock.Unlock();

            return (uint)CurrentThread.ObjSyncResult;
        }

        private (long, KThread) MutexUnlock(AMemory Memory, KThread CurrentThread, long MutexAddress)
        {
            KThread NewOwnerThread = CurrentThread.RelinquishMutex(MutexAddress, out int Count);

            int MutexValue = 0;

            if (NewOwnerThread != null)
            {
                MutexValue = NewOwnerThread.ThreadHandleForUserMutex;

                if (Count >= 2)
                {
                    MutexValue |= HasListenersMask;
                }

                NewOwnerThread.SignaledObj   = null;
                NewOwnerThread.ObjSyncResult = 0;

                NewOwnerThread.ReleaseAndResume();
            }

            long Result = 0;

            if (!KernelToUserInt32(Memory, MutexAddress, MutexValue))
            {
                Result = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            return (Result, NewOwnerThread);
        }

        public void SignalProcessWideKey(Process Process, AMemory Memory, long Address, int Count)
        {
            Queue<KThread> SignaledThreads = new Queue<KThread>();

            System.CriticalSectionLock.Lock();

            IOrderedEnumerable<KThread> SortedThreads = CondVarThreads.OrderBy(x => x.DynamicPriority);

            foreach (KThread Thread in SortedThreads.Where(x => x.CondVarAddress == Address))
            {
                TryAcquireMutex(Process, Memory, Thread);

                SignaledThreads.Enqueue(Thread);

                //If the count is <= 0, we should signal all threads waiting.
                if (Count >= 1 && --Count == 0)
                {
                    break;
                }
            }

            while (SignaledThreads.TryDequeue(out KThread Thread))
            {
                CondVarThreads.Remove(Thread);
            }

            System.CriticalSectionLock.Unlock();
        }

        private KThread TryAcquireMutex(Process Process, AMemory Memory, KThread Requester)
        {
            long Address = Requester.MutexAddress;

            Memory.SetExclusive(0, Address);

            if (!UserToKernelInt32(Memory, Address, out int MutexValue))
            {
                //Invalid address.
                Memory.ClearExclusive(0);

                Requester.SignaledObj   = null;
                Requester.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return null;
            }

            while (true)
            {
                if (Memory.TestExclusive(0, Address))
                {
                    if (MutexValue != 0)
                    {
                        //Update value to indicate there is a mutex waiter now.
                        Memory.WriteInt32(Address, MutexValue | HasListenersMask);
                    }
                    else
                    {
                        //No thread owning the mutex, assign to requesting thread.
                        Memory.WriteInt32(Address, Requester.ThreadHandleForUserMutex);
                    }

                    Memory.ClearExclusiveForStore(0);

                    break;
                }

                Memory.SetExclusive(0, Address);

                MutexValue = Memory.ReadInt32(Address);
            }

            if (MutexValue == 0)
            {
                //We now own the mutex.
                Requester.SignaledObj   = null;
                Requester.ObjSyncResult = 0;

                Requester.ReleaseAndResume();

                return null;
            }

            MutexValue &= ~HasListenersMask;

            KThread MutexOwner = Process.HandleTable.GetObject<KThread>(MutexValue);

            if (MutexOwner != null)
            {
                //Mutex already belongs to another thread, wait for it.
                MutexOwner.AddMutexWaiter(Requester);
            }
            else
            {
                //Invalid mutex owner.
                Requester.SignaledObj   = null;
                Requester.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                Requester.ReleaseAndResume();
            }

            return MutexOwner;
        }

        public long WaitForAddressIfEqual(AMemory Memory, long Address, int Value, long Timeout)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSectionLock.Lock();

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (!UserToKernelInt32(Memory, Address, out int CurrentValue))
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (CurrentValue == Value)
            {
                if (Timeout == 0)
                {
                    System.CriticalSectionLock.Unlock();

                    return MakeError(ErrorModule.Kernel, KernelErr.Timeout);
                }

                CurrentThread.MutexAddress         = Address;
                CurrentThread.WaitingInArbitration = true;

                InsertSortedByPriority(ArbiterThreads, CurrentThread);

                CurrentThread.Reschedule(ThreadSchedState.Paused);

                if (Timeout > 0)
                {
                    System.TimeManager.ScheduleFutureInvocation(CurrentThread, Timeout);
                }

                System.CriticalSectionLock.Unlock();

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }

                System.CriticalSectionLock.Lock();

                if (CurrentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(CurrentThread);

                    CurrentThread.WaitingInArbitration = false;
                }

                System.CriticalSectionLock.Unlock();

                return CurrentThread.ObjSyncResult;
            }

            System.CriticalSectionLock.Unlock();

            return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
        }

        public long WaitForAddressIfLessThan(
            AMemory Memory,
            long    Address,
            int     Value,
            bool    ShouldDecrement,
            long    Timeout)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSectionLock.Lock();

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            //If ShouldDecrement is true, do atomic decrement of the value at Address.
            Memory.SetExclusive(0, Address);

            if (!UserToKernelInt32(Memory, Address, out int CurrentValue))
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (ShouldDecrement)
            {
                while (CurrentValue < Value)
                {
                    if (Memory.TestExclusive(0, Address))
                    {
                        Memory.WriteInt32(Address, CurrentValue - 1);

                        Memory.ClearExclusiveForStore(0);

                        break;
                    }

                    Memory.SetExclusive(0, Address);

                    CurrentValue = Memory.ReadInt32(Address);
                }
            }

            Memory.ClearExclusive(0);

            if (CurrentValue < Value)
            {
                if (Timeout == 0)
                {
                    System.CriticalSectionLock.Unlock();

                    return MakeError(ErrorModule.Kernel, KernelErr.Timeout);
                }

                CurrentThread.MutexAddress         = Address;
                CurrentThread.WaitingInArbitration = true;

                InsertSortedByPriority(ArbiterThreads, CurrentThread);

                CurrentThread.Reschedule(ThreadSchedState.Paused);

                if (Timeout > 0)
                {
                    System.TimeManager.ScheduleFutureInvocation(CurrentThread, Timeout);
                }

                System.CriticalSectionLock.Unlock();

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }

                System.CriticalSectionLock.Lock();

                if (CurrentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(CurrentThread);

                    CurrentThread.WaitingInArbitration = false;
                }

                System.CriticalSectionLock.Unlock();

                return CurrentThread.ObjSyncResult;
            }

            System.CriticalSectionLock.Unlock();

            return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
        }

        private void InsertSortedByPriority(List<KThread> Threads, KThread Thread)
        {
            int NextIndex = -1;

            for (int Index = 0; Index < Threads.Count; Index++)
            {
                if (Threads[Index].DynamicPriority > Thread.DynamicPriority)
                {
                    NextIndex = Index;

                    break;
                }
            }

            if (NextIndex != -1)
            {
                Threads.Insert(NextIndex, Thread);
            }
            else
            {
                Threads.Add(Thread);
            }
        }

        public long Signal(long Address, int Count)
        {
            System.CriticalSectionLock.Lock();

            WakeArbiterThreads(Address, Count);

            System.CriticalSectionLock.Unlock();

            return 0;
        }

        public long SignalAndIncrementIfEqual(AMemory Memory, long Address, int Value, int Count)
        {
            System.CriticalSectionLock.Lock();

            Memory.SetExclusive(0, Address);

            if (!UserToKernelInt32(Memory, Address, out int CurrentValue))
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (CurrentValue == Value)
            {
                if (Memory.TestExclusive(0, Address))
                {
                    Memory.WriteInt32(Address, CurrentValue + 1);

                    Memory.ClearExclusiveForStore(0);

                    break;
                }

                Memory.SetExclusive(0, Address);

                CurrentValue = Memory.ReadInt32(Address);
            }

            Memory.ClearExclusive(0);

            if (CurrentValue != Value)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(Address, Count);

            System.CriticalSectionLock.Unlock();

            return 0;
        }

        public long SignalAndModifyIfEqual(AMemory Memory, long Address, int Value, int Count)
        {
            System.CriticalSectionLock.Lock();

            int Offset;

            //The value is decremented if the number of threads waiting is less
            //or equal to the Count of threads to be signaled, or Count is zero
            //or negative. It is incremented if there are no threads waiting.
            int WaitingCount = 0;

            foreach (KThread Thread in ArbiterThreads.Where(x => x.MutexAddress == Address))
            {
                if (++WaitingCount > Count)
                {
                    break;
                }
            }

            if (WaitingCount > 0)
            {
                Offset = WaitingCount <= Count || Count <= 0 ? -1 : 0;
            }
            else
            {
                Offset = 1;
            }

            Memory.SetExclusive(0, Address);

            if (!UserToKernelInt32(Memory, Address, out int CurrentValue))
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (CurrentValue == Value)
            {
                if (Memory.TestExclusive(0, Address))
                {
                    Memory.WriteInt32(Address, CurrentValue + Offset);

                    Memory.ClearExclusiveForStore(0);

                    break;
                }

                Memory.SetExclusive(0, Address);

                CurrentValue = Memory.ReadInt32(Address);
            }

            Memory.ClearExclusive(0);

            if (CurrentValue != Value)
            {
                System.CriticalSectionLock.Unlock();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(Address, Count);

            System.CriticalSectionLock.Unlock();

            return 0;
        }

        private void WakeArbiterThreads(long Address, int Count)
        {
            Queue<KThread> SignaledThreads = new Queue<KThread>();

            foreach (KThread Thread in ArbiterThreads.Where(x => x.MutexAddress == Address))
            {
                SignaledThreads.Enqueue(Thread);

                //If the count is <= 0, we should signal all threads waiting.
                if (Count >= 1 && --Count == 0)
                {
                    break;
                }
            }

            while (SignaledThreads.TryDequeue(out KThread Thread))
            {
                Thread.SignaledObj   = null;
                Thread.ObjSyncResult = 0;

                Thread.ReleaseAndResume();

                Thread.WaitingInArbitration = false;

                ArbiterThreads.Remove(Thread);
            }
        }

        private bool UserToKernelInt32(AMemory Memory, long Address, out int Value)
        {
            if (Memory.IsMapped(Address))
            {
                Value = Memory.ReadInt32(Address);

                return true;
            }

            Value = 0;

            return false;
        }

        private bool KernelToUserInt32(AMemory Memory, long Address, int Value)
        {
            if (Memory.IsMapped(Address))
            {
                Memory.WriteInt32ToSharedAddr(Address, Value);

                return true;
            }

            return false;
        }
    }
}
