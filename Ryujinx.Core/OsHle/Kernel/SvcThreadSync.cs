using ChocolArm64.State;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Kernel
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

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAlignment);

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

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAlignment);

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

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (IsWordAddressUnaligned(MutexAddress))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Unaligned mutex address 0x{MutexAddress:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAlignment);

                return;
            }

            KThread Thread = Process.HandleTable.GetData<KThread>(ThreadHandle);

            if (Thread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            MutexUnlock(CurrThread, MutexAddress);

            if (!CondVarWait(CurrThread, ThreadHandle, MutexAddress, CondVarAddress, Timeout))
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

            CondVarSignal(CurrThread, CondVarAddress, Count);

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
                int MutexValue = Process.Memory.ReadInt32(MutexAddress);

                Ns.Log.PrintDebug(LogClass.KernelSvc, "MutexValue = " + MutexValue.ToString("x8"));

                if (MutexValue != (OwnerThreadHandle | MutexHasListenersMask))
                {
                    return;
                }

                CurrThread.WaitHandle   = WaitThreadHandle;
                CurrThread.MutexAddress = MutexAddress;

                InsertWaitingMutexThread(OwnerThreadHandle, WaitThread);
            }

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Entering wait state...");

            Process.Scheduler.EnterWait(CurrThread);
        }

        private void MutexUnlock(KThread CurrThread, long MutexAddress)
        {
            lock (Process.ThreadSyncLock)
            {
                //This is the new thread that will now own the mutex.
                //If no threads are waiting for the lock, then it should be null.
                KThread OwnerThread = PopThread(CurrThread.MutexWaiters, x => x.MutexAddress == MutexAddress);

                if (OwnerThread != null)
                {
                    //Remove all waiting mutex from the old owner,
                    //and insert then on the new owner.
                    UpdateMutexOwner(CurrThread, OwnerThread, MutexAddress);

                    CurrThread.UpdatePriority();

                    int HasListeners = OwnerThread.MutexWaiters.Count > 0 ? MutexHasListenersMask : 0;

                    Process.Memory.WriteInt32(MutexAddress, HasListeners | OwnerThread.WaitHandle);

                    OwnerThread.WaitHandle     = 0;
                    OwnerThread.MutexAddress   = 0;
                    OwnerThread.CondVarAddress = 0;

                    OwnerThread.MutexOwner = null;

                    OwnerThread.UpdatePriority();

                    Process.Scheduler.WakeUp(OwnerThread);

                    Ns.Log.PrintDebug(LogClass.KernelSvc, "Gave mutex to thread id " + OwnerThread.ThreadId + "!");
                }
                else
                {
                    Process.Memory.WriteInt32(MutexAddress, 0);

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

            lock (Process.ThreadArbiterList)
            {
                WaitThread.CondVarSignaled = false;

                Process.ThreadArbiterList.Add(WaitThread);
            }

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Entering wait state...");

            if (Timeout != ulong.MaxValue)
            {
                Process.Scheduler.EnterWait(WaitThread, NsTimeConverter.GetTimeMs(Timeout));

                lock (Process.ThreadArbiterList)
                {
                    if (!WaitThread.CondVarSignaled)
                    {
                        Process.ThreadArbiterList.Remove(WaitThread);

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

        private void CondVarSignal(KThread CurrThread, long CondVarAddress, int Count)
        {
            lock (Process.ThreadArbiterList)
            {
                while (Count == -1 || Count-- > 0)
                {
                    KThread WaitThread = PopThread(Process.ThreadArbiterList, x => x.CondVarAddress == CondVarAddress);

                    if (WaitThread == null)
                    {
                        Ns.Log.PrintDebug(LogClass.KernelSvc, "No more threads to wake up!");

                        break;
                    }

                    WaitThread.CondVarSignaled = true;

                    AcquireMutexValue(WaitThread.MutexAddress);

                    int MutexValue = Process.Memory.ReadInt32(WaitThread.MutexAddress);

                    Ns.Log.PrintDebug(LogClass.KernelSvc, "MutexValue = " + MutexValue.ToString("x8"));

                    if (MutexValue == 0)
                    {
                        //Give the lock to this thread.
                        Process.Memory.WriteInt32(WaitThread.MutexAddress, WaitThread.WaitHandle);

                        WaitThread.WaitHandle     = 0;
                        WaitThread.MutexAddress   = 0;
                        WaitThread.CondVarAddress = 0;

                        WaitThread.MutexOwner?.UpdatePriority();

                        WaitThread.MutexOwner = null;

                        Process.Scheduler.WakeUp(WaitThread);
                    }
                    else
                    {
                        //Wait until the lock is released.
                        MutexValue &= ~MutexHasListenersMask;

                        InsertWaitingMutexThread(MutexValue, WaitThread);

                        MutexValue |= MutexHasListenersMask;

                        Process.Memory.WriteInt32(WaitThread.MutexAddress, MutexValue);
                    }

                    ReleaseMutexValue(WaitThread.MutexAddress);
                }
            }
        }

        private void UpdateMutexOwner(KThread CurrThread, KThread NewOwner, long MutexAddress)
        {
            //Go through all threads waiting for the mutex,
            //and update the MutexOwner field to point to the new owner.
            lock (Process.ThreadSyncLock)
            {
                for (int Index = 0; Index < CurrThread.MutexWaiters.Count; Index++)
                {
                    KThread Thread = CurrThread.MutexWaiters[Index];

                    if (Thread.MutexAddress == MutexAddress)
                    {
                        CurrThread.MutexWaiters.RemoveAt(Index--);

                        Thread.MutexOwner = NewOwner;

                        InsertWaitingMutexThread(NewOwner, Thread);
                    }
                }
            }
        }

        private void InsertWaitingMutexThread(int OwnerThreadHandle, KThread WaitThread)
        {
            KThread OwnerThread = Process.HandleTable.GetData<KThread>(OwnerThreadHandle);

            if (OwnerThread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{OwnerThreadHandle:x8}!");

                return;
            }

            InsertWaitingMutexThread(OwnerThread, WaitThread);
        }

        private void InsertWaitingMutexThread(KThread OwnerThread, KThread WaitThread)
        {
            lock (Process.ThreadSyncLock)
            {
                WaitThread.MutexOwner = OwnerThread;

                if (!OwnerThread.MutexWaiters.Contains(WaitThread))
                {
                    OwnerThread.MutexWaiters.Add(WaitThread);

                    OwnerThread.UpdatePriority();
                }
            }
        }

        private KThread PopThread(List<KThread> Threads, Func<KThread, bool> Predicate)
        {
            KThread Thread = Threads.OrderBy(x => x.ActualPriority).FirstOrDefault(Predicate);

            if (Thread != null)
            {
                Threads.Remove(Thread);
            }

            return Thread;
        }

        private void AcquireMutexValue(long MutexAddress)
        {
            while (!Process.Memory.AcquireAddress(MutexAddress))
            {
                Thread.Yield();
            }
        }

        private void ReleaseMutexValue(long MutexAddress)
        {
            Process.Memory.ReleaseAddress(MutexAddress);
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