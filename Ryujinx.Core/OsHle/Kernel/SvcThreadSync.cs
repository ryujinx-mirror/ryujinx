using ChocolArm64.State;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using System;
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

            if (MutexUnlock(Process.GetThread(ThreadState.Tpidr), MutexAddress))
            {
                Process.Scheduler.Yield(Process.GetThread(ThreadState.Tpidr));
            }

            ThreadState.X0 = 0;
        }

        private void SvcWaitProcessWideKeyAtomic(AThreadState ThreadState)
        {
            long  MutexAddress   = (long)ThreadState.X0;
            long  CondVarAddress = (long)ThreadState.X1;
            int   ThreadHandle   =  (int)ThreadState.X2;
            ulong Timeout        =       ThreadState.X3;

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

            Process.Scheduler.Yield(Process.GetThread(ThreadState.Tpidr));

            ThreadState.X0 = 0;
        }

        private void SvcSignalProcessWideKey(AThreadState ThreadState)
        {
            long CondVarAddress = (long)ThreadState.X0;
            int  Count          =  (int)ThreadState.X1;

            CondVarSignal(CondVarAddress, Count);

            ThreadState.X0 = 0;
        }

        private void MutexLock(
            KThread CurrThread,
            KThread WaitThread,
            int     OwnerThreadHandle,
            int     WaitThreadHandle,
            long    MutexAddress)
        {
            int MutexValue = Process.Memory.ReadInt32(MutexAddress);

            if (MutexValue != (OwnerThreadHandle | MutexHasListenersMask))
            {
                return;
            }

            CurrThread.WaitHandle   = WaitThreadHandle;
            CurrThread.MutexAddress = MutexAddress;

            InsertWaitingMutexThread(OwnerThreadHandle, WaitThread);

            Process.Scheduler.EnterWait(WaitThread);
        }

        private bool MutexUnlock(KThread CurrThread, long MutexAddress)
        {
            if (CurrThread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid mutex 0x{MutexAddress:x16}!");

                return false;
            }

            lock (CurrThread)
            {
                //This is the new thread that will not own the mutex.
                //If no threads are waiting for the lock, then it should be null.
                KThread OwnerThread = CurrThread.NextMutexThread;

                while (OwnerThread != null && OwnerThread.MutexAddress != MutexAddress)
                {
                    OwnerThread = OwnerThread.NextMutexThread;
                }

                CurrThread.NextMutexThread = null;

                if (OwnerThread != null)
                {
                    int HasListeners = OwnerThread.NextMutexThread != null ? MutexHasListenersMask : 0;

                    Process.Memory.WriteInt32(MutexAddress, HasListeners | OwnerThread.WaitHandle);

                    OwnerThread.WaitHandle     = 0;
                    OwnerThread.MutexAddress   = 0;
                    OwnerThread.CondVarAddress = 0;

                    OwnerThread.MutexOwner = null;

                    OwnerThread.UpdatePriority();

                    Process.Scheduler.WakeUp(OwnerThread);

                    return true;
                }
                else
                {
                    Process.Memory.WriteInt32(MutexAddress, 0);

                    return false;
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

            lock (CondVarLock)
            {
                KThread CurrThread = Process.ThreadArbiterList;

                if (CurrThread != null)
                {
                    bool DoInsert = CurrThread != WaitThread;

                    while (CurrThread.NextCondVarThread != null)
                    {
                        if (CurrThread.NextCondVarThread.ActualPriority < WaitThread.ActualPriority)
                        {
                            break;
                        }

                        CurrThread = CurrThread.NextCondVarThread;

                        DoInsert &= CurrThread != WaitThread;
                    }

                    //Only insert if the node doesn't already exist in the list.
                    //This prevents circular references.
                    if (DoInsert)
                    {
                        if (WaitThread.NextCondVarThread != null)
                        {
                            throw new InvalidOperationException();
                        }

                        WaitThread.NextCondVarThread = CurrThread.NextCondVarThread;
                        CurrThread.NextCondVarThread = WaitThread;
                    }
                }
                else
                {
                    Process.ThreadArbiterList = WaitThread;
                }
            }

            if (Timeout != ulong.MaxValue)
            {
                return Process.Scheduler.EnterWait(WaitThread, NsTimeConverter.GetTimeMs(Timeout));
            }
            else
            {
                return Process.Scheduler.EnterWait(WaitThread);
            }
        }

        private void CondVarSignal(long CondVarAddress, int Count)
        {
            lock (CondVarLock)
            {
                KThread PrevThread = null;
                KThread CurrThread = Process.ThreadArbiterList;

                while (CurrThread != null && (Count == -1 || Count > 0))
                {
                    if (CurrThread.CondVarAddress == CondVarAddress)
                    {
                        if (PrevThread != null)
                        {
                            PrevThread.NextCondVarThread = CurrThread.NextCondVarThread;
                        }
                        else
                        {
                            Process.ThreadArbiterList = CurrThread.NextCondVarThread;
                        }

                        CurrThread.NextCondVarThread = null;

                        AcquireMutexValue(CurrThread.MutexAddress);

                        int MutexValue = Process.Memory.ReadInt32(CurrThread.MutexAddress);

                        MutexValue &= ~MutexHasListenersMask;

                        if (MutexValue == 0)
                        {
                            //Give the lock to this thread.
                            Process.Memory.WriteInt32(CurrThread.MutexAddress, CurrThread.WaitHandle);

                            CurrThread.WaitHandle     = 0;
                            CurrThread.MutexAddress   = 0;
                            CurrThread.CondVarAddress = 0;

                            CurrThread.MutexOwner = null;

                            CurrThread.UpdatePriority();

                            Process.Scheduler.WakeUp(CurrThread);
                        }
                        else
                        {
                            //Wait until the lock is released.
                            InsertWaitingMutexThread(MutexValue, CurrThread);

                            MutexValue |= MutexHasListenersMask;

                            Process.Memory.WriteInt32(CurrThread.MutexAddress, MutexValue);
                        }

                        ReleaseMutexValue(CurrThread.MutexAddress);

                        Count--;
                    }

                    PrevThread = CurrThread;
                    CurrThread = CurrThread.NextCondVarThread;
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

            WaitThread.MutexOwner = OwnerThread;

            lock (OwnerThread)
            {
                KThread CurrThread = OwnerThread;

                while (CurrThread.NextMutexThread != null)
                {
                    if (CurrThread == WaitThread)
                    {
                        return;
                    }

                    if (CurrThread.NextMutexThread.ActualPriority < WaitThread.ActualPriority)
                    {
                        break;
                    }

                    CurrThread = CurrThread.NextMutexThread;
                }

                if (CurrThread != WaitThread)
                {
                    if (WaitThread.NextCondVarThread != null)
                    {
                        throw new InvalidOperationException();
                    }

                    WaitThread.NextMutexThread = CurrThread.NextMutexThread;
                    CurrThread.NextMutexThread = WaitThread;
                }
            }

            OwnerThread.UpdatePriority();
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