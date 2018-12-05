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

        public long ArbitrateLock(int OwnerHandle, long MutexAddress, int RequesterHandle)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSection.Enter();

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = 0;

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (!KernelTransfer.UserToKernelInt32(System, MutexAddress, out int MutexValue))
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);;
            }

            if (MutexValue != (OwnerHandle | HasListenersMask))
            {
                System.CriticalSection.Leave();

                return 0;
            }

            KThread MutexOwner = CurrentProcess.HandleTable.GetObject<KThread>(OwnerHandle);

            if (MutexOwner == null)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }

            CurrentThread.MutexAddress             = MutexAddress;
            CurrentThread.ThreadHandleForUserMutex = RequesterHandle;

            MutexOwner.AddMutexWaiter(CurrentThread);

            CurrentThread.Reschedule(ThreadSchedState.Paused);

            System.CriticalSection.Leave();
            System.CriticalSection.Enter();

            if (CurrentThread.MutexOwner != null)
            {
                CurrentThread.MutexOwner.RemoveMutexWaiter(CurrentThread);
            }

            System.CriticalSection.Leave();

            return (uint)CurrentThread.ObjSyncResult;
        }

        public long ArbitrateUnlock(long MutexAddress)
        {
            System.CriticalSection.Enter();

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            (long Result, KThread NewOwnerThread) = MutexUnlock(CurrentThread, MutexAddress);

            if (Result != 0 && NewOwnerThread != null)
            {
                NewOwnerThread.SignaledObj   = null;
                NewOwnerThread.ObjSyncResult = (int)Result;
            }

            System.CriticalSection.Leave();

            return Result;
        }

        public long WaitProcessWideKeyAtomic(
            long MutexAddress,
            long CondVarAddress,
            int  ThreadHandle,
            long Timeout)
        {
            System.CriticalSection.Enter();

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            (long Result, _) = MutexUnlock(CurrentThread, MutexAddress);

            if (Result != 0)
            {
                System.CriticalSection.Leave();

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

            System.CriticalSection.Leave();

            if (Timeout > 0)
            {
                System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
            }

            System.CriticalSection.Enter();

            if (CurrentThread.MutexOwner != null)
            {
                CurrentThread.MutexOwner.RemoveMutexWaiter(CurrentThread);
            }

            CondVarThreads.Remove(CurrentThread);

            System.CriticalSection.Leave();

            return (uint)CurrentThread.ObjSyncResult;
        }

        private (long, KThread) MutexUnlock(KThread CurrentThread, long MutexAddress)
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

            if (!KernelTransfer.KernelToUserInt32(System, MutexAddress, MutexValue))
            {
                Result = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            return (Result, NewOwnerThread);
        }

        public void SignalProcessWideKey(long Address, int Count)
        {
            Queue<KThread> SignaledThreads = new Queue<KThread>();

            System.CriticalSection.Enter();

            IOrderedEnumerable<KThread> SortedThreads = CondVarThreads.OrderBy(x => x.DynamicPriority);

            foreach (KThread Thread in SortedThreads.Where(x => x.CondVarAddress == Address))
            {
                TryAcquireMutex(Thread);

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

            System.CriticalSection.Leave();
        }

        private KThread TryAcquireMutex(KThread Requester)
        {
            long Address = Requester.MutexAddress;

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            CurrentProcess.CpuMemory.SetExclusive(0, Address);

            if (!KernelTransfer.UserToKernelInt32(System, Address, out int MutexValue))
            {
                //Invalid address.
                CurrentProcess.CpuMemory.ClearExclusive(0);

                Requester.SignaledObj   = null;
                Requester.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return null;
            }

            while (true)
            {
                if (CurrentProcess.CpuMemory.TestExclusive(0, Address))
                {
                    if (MutexValue != 0)
                    {
                        //Update value to indicate there is a mutex waiter now.
                        CurrentProcess.CpuMemory.WriteInt32(Address, MutexValue | HasListenersMask);
                    }
                    else
                    {
                        //No thread owning the mutex, assign to requesting thread.
                        CurrentProcess.CpuMemory.WriteInt32(Address, Requester.ThreadHandleForUserMutex);
                    }

                    CurrentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                CurrentProcess.CpuMemory.SetExclusive(0, Address);

                MutexValue = CurrentProcess.CpuMemory.ReadInt32(Address);
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

            KThread MutexOwner = CurrentProcess.HandleTable.GetObject<KThread>(MutexValue);

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

        public long WaitForAddressIfEqual(long Address, int Value, long Timeout)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSection.Enter();

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            if (!KernelTransfer.UserToKernelInt32(System, Address, out int CurrentValue))
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (CurrentValue == Value)
            {
                if (Timeout == 0)
                {
                    System.CriticalSection.Leave();

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

                System.CriticalSection.Leave();

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }

                System.CriticalSection.Enter();

                if (CurrentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(CurrentThread);

                    CurrentThread.WaitingInArbitration = false;
                }

                System.CriticalSection.Leave();

                return CurrentThread.ObjSyncResult;
            }

            System.CriticalSection.Leave();

            return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
        }

        public long WaitForAddressIfLessThan(long Address, int Value, bool ShouldDecrement, long Timeout)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSection.Enter();

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            CurrentThread.SignaledObj   = null;
            CurrentThread.ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            //If ShouldDecrement is true, do atomic decrement of the value at Address.
            CurrentProcess.CpuMemory.SetExclusive(0, Address);

            if (!KernelTransfer.UserToKernelInt32(System, Address, out int CurrentValue))
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            if (ShouldDecrement)
            {
                while (CurrentValue < Value)
                {
                    if (CurrentProcess.CpuMemory.TestExclusive(0, Address))
                    {
                        CurrentProcess.CpuMemory.WriteInt32(Address, CurrentValue - 1);

                        CurrentProcess.CpuMemory.ClearExclusiveForStore(0);

                        break;
                    }

                    CurrentProcess.CpuMemory.SetExclusive(0, Address);

                    CurrentValue = CurrentProcess.CpuMemory.ReadInt32(Address);
                }
            }

            CurrentProcess.CpuMemory.ClearExclusive(0);

            if (CurrentValue < Value)
            {
                if (Timeout == 0)
                {
                    System.CriticalSection.Leave();

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

                System.CriticalSection.Leave();

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }

                System.CriticalSection.Enter();

                if (CurrentThread.WaitingInArbitration)
                {
                    ArbiterThreads.Remove(CurrentThread);

                    CurrentThread.WaitingInArbitration = false;
                }

                System.CriticalSection.Leave();

                return CurrentThread.ObjSyncResult;
            }

            System.CriticalSection.Leave();

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
            System.CriticalSection.Enter();

            WakeArbiterThreads(Address, Count);

            System.CriticalSection.Leave();

            return 0;
        }

        public long SignalAndIncrementIfEqual(long Address, int Value, int Count)
        {
            System.CriticalSection.Enter();

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            CurrentProcess.CpuMemory.SetExclusive(0, Address);

            if (!KernelTransfer.UserToKernelInt32(System, Address, out int CurrentValue))
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (CurrentValue == Value)
            {
                if (CurrentProcess.CpuMemory.TestExclusive(0, Address))
                {
                    CurrentProcess.CpuMemory.WriteInt32(Address, CurrentValue + 1);

                    CurrentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                CurrentProcess.CpuMemory.SetExclusive(0, Address);

                CurrentValue = CurrentProcess.CpuMemory.ReadInt32(Address);
            }

            CurrentProcess.CpuMemory.ClearExclusive(0);

            if (CurrentValue != Value)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(Address, Count);

            System.CriticalSection.Leave();

            return 0;
        }

        public long SignalAndModifyIfEqual(long Address, int Value, int Count)
        {
            System.CriticalSection.Enter();

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

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            CurrentProcess.CpuMemory.SetExclusive(0, Address);

            if (!KernelTransfer.UserToKernelInt32(System, Address, out int CurrentValue))
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);
            }

            while (CurrentValue == Value)
            {
                if (CurrentProcess.CpuMemory.TestExclusive(0, Address))
                {
                    CurrentProcess.CpuMemory.WriteInt32(Address, CurrentValue + Offset);

                    CurrentProcess.CpuMemory.ClearExclusiveForStore(0);

                    break;
                }

                CurrentProcess.CpuMemory.SetExclusive(0, Address);

                CurrentValue = CurrentProcess.CpuMemory.ReadInt32(Address);
            }

            CurrentProcess.CpuMemory.ClearExclusive(0);

            if (CurrentValue != Value)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            WakeArbiterThreads(Address, Count);

            System.CriticalSection.Leave();

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
    }
}
