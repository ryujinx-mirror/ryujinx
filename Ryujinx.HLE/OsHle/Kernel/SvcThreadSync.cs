using ChocolArm64.State;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using System;

using static Ryujinx.HLE.OsHle.ErrorCode;

namespace Ryujinx.HLE.OsHle.Kernel
{
    partial class SvcHandler
    {
        private const int MutexHasListenersMask = 0x40000000;

        private void SvcArbitrateLock(AThreadState ThreadState)
        {
            int  OwnerThreadHandle =  (int)ThreadState.X0;
            long MutexAddress      = (long)ThreadState.X1;
            int  WaitThreadHandle  =  (int)ThreadState.X2;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "OwnerThreadHandle = " + OwnerThreadHandle.ToString("x8")  + ", " +
                "MutexAddress = "      + MutexAddress     .ToString("x16") + ", " +
                "WaitThreadHandle = "  + WaitThreadHandle .ToString("x8"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KThread OwnerThread = Process.HandleTable.GetData<KThread>(OwnerThreadHandle);

            if (OwnerThread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid owner thread handle 0x{OwnerThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            KThread WaitThread = Process.HandleTable.GetData<KThread>(WaitThreadHandle);

            if (WaitThread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid requesting thread handle 0x{WaitThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            MutexLock(CurrThread, WaitThread, OwnerThreadHandle, WaitThreadHandle, MutexAddress);

            ThreadState.X0 = 0;
        }

        private void SvcArbitrateUnlock(AThreadState ThreadState)
        {
            long MutexAddress = (long)ThreadState.X0;

            Ns.Log.PrintDebug(LogClass.KernelSvc, "MutexAddress = " + MutexAddress.ToString("x16"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            MutexUnlock(Process.GetThread(ThreadState.Tpidr), MutexAddress);

            ThreadState.X0 = 0;
        }

        private void SvcWaitProcessWideKeyAtomic(AThreadState ThreadState)
        {
            long  MutexAddress   = (long)ThreadState.X0;
            long  CondVarAddress = (long)ThreadState.X1;
            int   ThreadHandle   =  (int)ThreadState.X2;
            ulong Timeout        =       ThreadState.X3;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "MutexAddress = "   + MutexAddress  .ToString("x16") + ", " +
                "CondVarAddress = " + CondVarAddress.ToString("x16") + ", " +
                "ThreadHandle = "   + ThreadHandle  .ToString("x8")  + ", " +
                "Timeout = "        + Timeout       .ToString("x16"));

            if (IsPointingInsideKernel(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            KThread Thread = Process.HandleTable.GetData<KThread>(ThreadHandle);

            if (Thread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            KThread WaitThread = Process.GetThread(ThreadState.Tpidr);

            if (!CondVarWait(WaitThread, ThreadHandle, MutexAddress, CondVarAddress, Timeout))
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.Timeout);

                return;
            }

            ThreadState.X0 = 0;
        }

        private void SvcSignalProcessWideKey(AThreadState ThreadState)
        {
            long CondVarAddress = (long)ThreadState.X0;
            int  Count          =  (int)ThreadState.X1;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "CondVarAddress = " + CondVarAddress.ToString("x16") + ", " +
                "Count = "          + Count         .ToString("x8"));

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            CondVarSignal(ThreadState, CurrThread, CondVarAddress, Count);

            ThreadState.X0 = 0;
        }

        private void MutexLock(
            KThread CurrThread,
            KThread WaitThread,
            int     OwnerThreadHandle,
            int     WaitThreadHandle,
            long    MutexAddress)
        {
            lock (Process.ThreadSyncLock)
            {
                int MutexValue = Memory.ReadInt32(MutexAddress);

                Ns.Log.PrintDebug(LogClass.KernelSvc, "MutexValue = " + MutexValue.ToString("x8"));

                if (MutexValue != (OwnerThreadHandle | MutexHasListenersMask))
                {
                    return;
                }

                CurrThread.WaitHandle   = WaitThreadHandle;
                CurrThread.MutexAddress = MutexAddress;

                InsertWaitingMutexThreadUnsafe(OwnerThreadHandle, WaitThread);
            }

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Entering wait state...");

            Process.Scheduler.EnterWait(CurrThread);
        }

        private void SvcWaitForAddress(AThreadState ThreadState)
        {
            long            Address = (long)ThreadState.X0;
            ArbitrationType Type    = (ArbitrationType)ThreadState.X1;
            int             Value   = (int)ThreadState.X2;
            ulong           Timeout = ThreadState.X3;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "Address = "         + Address.ToString("x16") + ", " +
                "ArbitrationType = " + Type   .ToString()      + ", " +
                "Value = "           + Value  .ToString("x8")  + ", " +
                "Timeout = "         + Timeout.ToString("x16"));

            if (IsPointingInsideKernel(Address))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (IsWordAddressUnaligned(Address))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned address 0x{Address:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            switch (Type)
            {
                case ArbitrationType.WaitIfLessThan:
                    ThreadState.X0 = AddressArbiter.WaitForAddressIfLessThan(Process, ThreadState, Memory, Address, Value, Timeout, false);
                    break;

                case ArbitrationType.DecrementAndWaitIfLessThan:
                    ThreadState.X0 = AddressArbiter.WaitForAddressIfLessThan(Process, ThreadState, Memory, Address, Value, Timeout, true);
                    break;

                case ArbitrationType.WaitIfEqual:
                    ThreadState.X0 = AddressArbiter.WaitForAddressIfEqual(Process, ThreadState, Memory, Address, Value, Timeout);
                    break;

                default:
                    ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidEnumValue);
                    break;
            }
        }

        private void MutexUnlock(KThread CurrThread, long MutexAddress)
        {
            lock (Process.ThreadSyncLock)
            {
                //This is the new thread that will now own the mutex.
                //If no threads are waiting for the lock, then it should be null.
                (KThread OwnerThread, int Count) = PopMutexThreadUnsafe(CurrThread, MutexAddress);

                if (OwnerThread == CurrThread)
                {
                    throw new InvalidOperationException();
                }

                if (OwnerThread != null)
                {
                    //Remove all waiting mutex from the old owner,
                    //and insert then on the new owner.
                    UpdateMutexOwnerUnsafe(CurrThread, OwnerThread, MutexAddress);

                    CurrThread.UpdatePriority();

                    int HasListeners = Count >= 2 ? MutexHasListenersMask : 0;

                    Memory.WriteInt32ToSharedAddr(MutexAddress, HasListeners | OwnerThread.WaitHandle);

                    OwnerThread.WaitHandle     = 0;
                    OwnerThread.MutexAddress   = 0;
                    OwnerThread.CondVarAddress = 0;
                    OwnerThread.MutexOwner     = null;

                    OwnerThread.UpdatePriority();

                    Process.Scheduler.WakeUp(OwnerThread);

                    Ns.Log.PrintDebug(LogClass.KernelSvc, "Gave mutex to thread id " + OwnerThread.ThreadId + "!");
                }
                else
                {
                    Memory.WriteInt32ToSharedAddr(MutexAddress, 0);

                    Ns.Log.PrintDebug(LogClass.KernelSvc, "No threads waiting mutex!");
                }
            }
        }

        private bool CondVarWait(
            KThread WaitThread,
            int     WaitThreadHandle,
            long    MutexAddress,
            long    CondVarAddress,
            ulong   Timeout)
        {
            WaitThread.WaitHandle     = WaitThreadHandle;
            WaitThread.MutexAddress   = MutexAddress;
            WaitThread.CondVarAddress = CondVarAddress;

            lock (Process.ThreadSyncLock)
            {
                MutexUnlock(WaitThread, MutexAddress);

                WaitThread.CondVarSignaled = false;

                Process.ThreadArbiterList.Add(WaitThread);
            }

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Entering wait state...");

            if (Timeout != ulong.MaxValue)
            {
                Process.Scheduler.EnterWait(WaitThread, NsTimeConverter.GetTimeMs(Timeout));

                lock (Process.ThreadSyncLock)
                {
                    if (!WaitThread.CondVarSignaled || WaitThread.MutexOwner != null)
                    {
                        if (WaitThread.MutexOwner != null)
                        {
                            WaitThread.MutexOwner.MutexWaiters.Remove(WaitThread);
                            WaitThread.MutexOwner.UpdatePriority();

                            WaitThread.MutexOwner = null;
                        }

                        Process.ThreadArbiterList.Remove(WaitThread);

                        Ns.Log.PrintDebug(LogClass.KernelSvc, "Timed out...");

                        return false;
                    }
                }
            }
            else
            {
                Process.Scheduler.EnterWait(WaitThread);
            }

            return true;
        }

        private void CondVarSignal(
            AThreadState ThreadState,
            KThread      CurrThread,
            long         CondVarAddress,
            int          Count)
        {
            lock (Process.ThreadSyncLock)
            {
                while (Count == -1 || Count-- > 0)
                {
                    KThread WaitThread = PopCondVarThreadUnsafe(CondVarAddress);

                    if (WaitThread == null)
                    {
                        Ns.Log.PrintDebug(LogClass.KernelSvc, "No more threads to wake up!");

                        break;
                    }

                    WaitThread.CondVarSignaled = true;

                    long MutexAddress = WaitThread.MutexAddress;

                    Memory.SetExclusive(ThreadState, MutexAddress);

                    int MutexValue = Memory.ReadInt32(MutexAddress);

                    while (MutexValue != 0)
                    {
                        if (Memory.TestExclusive(ThreadState, MutexAddress))
                        {
                            //Wait until the lock is released.
                            InsertWaitingMutexThreadUnsafe(MutexValue & ~MutexHasListenersMask, WaitThread);

                            Memory.WriteInt32(MutexAddress, MutexValue | MutexHasListenersMask);

                            Memory.ClearExclusiveForStore(ThreadState);

                            break;
                        }

                        Memory.SetExclusive(ThreadState, MutexAddress);

                        MutexValue = Memory.ReadInt32(MutexAddress);
                    }

                    Ns.Log.PrintDebug(LogClass.KernelSvc, "MutexValue = " + MutexValue.ToString("x8"));

                    if (MutexValue == 0)
                    {
                        //Give the lock to this thread.
                        Memory.WriteInt32ToSharedAddr(MutexAddress, WaitThread.WaitHandle);

                        WaitThread.WaitHandle     = 0;
                        WaitThread.MutexAddress   = 0;
                        WaitThread.CondVarAddress = 0;

                        WaitThread.MutexOwner?.UpdatePriority();

                        WaitThread.MutexOwner = null;

                        Process.Scheduler.WakeUp(WaitThread);
                    }
                }
            }
        }

        private void UpdateMutexOwnerUnsafe(KThread CurrThread, KThread NewOwner, long MutexAddress)
        {
            //Go through all threads waiting for the mutex,
            //and update the MutexOwner field to point to the new owner.
            for (int Index = 0; Index < CurrThread.MutexWaiters.Count; Index++)
            {
                KThread Thread = CurrThread.MutexWaiters[Index];

                if (Thread.MutexAddress == MutexAddress)
                {
                    CurrThread.MutexWaiters.RemoveAt(Index--);

                    InsertWaitingMutexThreadUnsafe(NewOwner, Thread);
                }
            }
        }

        private void InsertWaitingMutexThreadUnsafe(int OwnerThreadHandle, KThread WaitThread)
        {
            KThread OwnerThread = Process.HandleTable.GetData<KThread>(OwnerThreadHandle);

            if (OwnerThread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{OwnerThreadHandle:x8}!");

                return;
            }

            InsertWaitingMutexThreadUnsafe(OwnerThread, WaitThread);
        }

        private void InsertWaitingMutexThreadUnsafe(KThread OwnerThread, KThread WaitThread)
        {
            WaitThread.MutexOwner = OwnerThread;

            if (!OwnerThread.MutexWaiters.Contains(WaitThread))
            {
                OwnerThread.MutexWaiters.Add(WaitThread);

                OwnerThread.UpdatePriority();
            }
        }

        private (KThread, int) PopMutexThreadUnsafe(KThread OwnerThread, long MutexAddress)
        {
            int Count = 0;

            KThread WakeThread = null;

            foreach (KThread Thread in OwnerThread.MutexWaiters)
            {
                if (Thread.MutexAddress != MutexAddress)
                {
                    continue;
                }

                if (WakeThread == null || Thread.ActualPriority < WakeThread.ActualPriority)
                {
                    WakeThread = Thread;
                }

                Count++;
            }

            if (WakeThread != null)
            {
                OwnerThread.MutexWaiters.Remove(WakeThread);
            }

            return (WakeThread, Count);
        }

        private KThread PopCondVarThreadUnsafe(long CondVarAddress)
        {
            KThread WakeThread = null;

            foreach (KThread Thread in Process.ThreadArbiterList)
            {
                if (Thread.CondVarAddress != CondVarAddress)
                {
                    continue;
                }

                if (WakeThread == null || Thread.ActualPriority < WakeThread.ActualPriority)
                {
                    WakeThread = Thread;
                }
            }

            if (WakeThread != null)
            {
                Process.ThreadArbiterList.Remove(WakeThread);
            }

            return WakeThread;
        }

        private bool IsPointingInsideKernel(long Address)
        {
            return ((ulong)Address + 0x1000000000) < 0xffffff000;
        }

        private bool IsWordAddressUnaligned(long Address)
        {
            return (Address & 3) != 0;
        }
    }
}