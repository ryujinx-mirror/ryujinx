using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSynchronization
    {
        private Horizon System;

        public KSynchronization(Horizon System)
        {
            this.System = System;
        }

        public long WaitFor(KSynchronizationObject[] SyncObjs, long Timeout, ref int HndIndex)
        {
            long Result = MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            System.CriticalSectionLock.Lock();

            //Check if objects are already signaled before waiting.
            for (int Index = 0; Index < SyncObjs.Length; Index++)
            {
                if (!SyncObjs[Index].IsSignaled())
                {
                    continue;
                }

                HndIndex = Index;

                System.CriticalSectionLock.Unlock();

                return 0;
            }

            if (Timeout == 0)
            {
                System.CriticalSectionLock.Unlock();

                return Result;
            }

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                Result = MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }
            else if (CurrentThread.SyncCancelled)
            {
                CurrentThread.SyncCancelled = false;

                Result = MakeError(ErrorModule.Kernel, KernelErr.Cancelled);
            }
            else
            {
                LinkedListNode<KThread>[] SyncNodes = new LinkedListNode<KThread>[SyncObjs.Length];

                for (int Index = 0; Index < SyncObjs.Length; Index++)
                {
                    SyncNodes[Index] = SyncObjs[Index].AddWaitingThread(CurrentThread);
                }

                CurrentThread.WaitingSync   = true;
                CurrentThread.SignaledObj   = null;
                CurrentThread.ObjSyncResult = (int)Result;

                CurrentThread.Reschedule(ThreadSchedState.Paused);

                if (Timeout > 0)
                {
                    System.TimeManager.ScheduleFutureInvocation(CurrentThread, Timeout);
                }

                System.CriticalSectionLock.Unlock();

                CurrentThread.WaitingSync = false;

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }

                System.CriticalSectionLock.Lock();

                Result = (uint)CurrentThread.ObjSyncResult;

                HndIndex = -1;

                for (int Index = 0; Index < SyncObjs.Length; Index++)
                {
                    SyncObjs[Index].RemoveWaitingThread(SyncNodes[Index]);

                    if (SyncObjs[Index] == CurrentThread.SignaledObj)
                    {
                        HndIndex = Index;
                    }
                }
            }

            System.CriticalSectionLock.Unlock();

            return Result;
        }

        public void SignalObject(KSynchronizationObject SyncObj)
        {
            System.CriticalSectionLock.Lock();

            if (SyncObj.IsSignaled())
            {
                LinkedListNode<KThread> Node = SyncObj.WaitingThreads.First;

                while (Node != null)
                {
                    KThread Thread = Node.Value;

                    if ((Thread.SchedFlags & ThreadSchedState.LowNibbleMask) == ThreadSchedState.Paused)
                    {
                        Thread.SignaledObj   = SyncObj;
                        Thread.ObjSyncResult = 0;

                        Thread.Reschedule(ThreadSchedState.Running);
                    }

                    Node = Node.Next;
                }
            }

            System.CriticalSectionLock.Unlock();
        }
    }
}