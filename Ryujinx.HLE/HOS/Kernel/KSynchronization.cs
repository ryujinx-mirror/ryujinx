using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSynchronization
    {
        private Horizon _system;

        public KSynchronization(Horizon system)
        {
            _system = system;
        }

        public long WaitFor(KSynchronizationObject[] syncObjs, long timeout, ref int hndIndex)
        {
            long result = MakeError(ErrorModule.Kernel, KernelErr.Timeout);

            _system.CriticalSection.Enter();

            //Check if objects are already signaled before waiting.
            for (int index = 0; index < syncObjs.Length; index++)
            {
                if (!syncObjs[index].IsSignaled())
                {
                    continue;
                }

                hndIndex = index;

                _system.CriticalSection.Leave();

                return 0;
            }

            if (timeout == 0)
            {
                _system.CriticalSection.Leave();

                return result;
            }

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                result = MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }
            else if (currentThread.SyncCancelled)
            {
                currentThread.SyncCancelled = false;

                result = MakeError(ErrorModule.Kernel, KernelErr.Cancelled);
            }
            else
            {
                LinkedListNode<KThread>[] syncNodes = new LinkedListNode<KThread>[syncObjs.Length];

                for (int index = 0; index < syncObjs.Length; index++)
                {
                    syncNodes[index] = syncObjs[index].AddWaitingThread(currentThread);
                }

                currentThread.WaitingSync   = true;
                currentThread.SignaledObj   = null;
                currentThread.ObjSyncResult = (int)result;

                currentThread.Reschedule(ThreadSchedState.Paused);

                if (timeout > 0)
                {
                    _system.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                _system.CriticalSection.Leave();

                currentThread.WaitingSync = false;

                if (timeout > 0)
                {
                    _system.TimeManager.UnscheduleFutureInvocation(currentThread);
                }

                _system.CriticalSection.Enter();

                result = (uint)currentThread.ObjSyncResult;

                hndIndex = -1;

                for (int index = 0; index < syncObjs.Length; index++)
                {
                    syncObjs[index].RemoveWaitingThread(syncNodes[index]);

                    if (syncObjs[index] == currentThread.SignaledObj)
                    {
                        hndIndex = index;
                    }
                }
            }

            _system.CriticalSection.Leave();

            return result;
        }

        public void SignalObject(KSynchronizationObject syncObj)
        {
            _system.CriticalSection.Enter();

            if (syncObj.IsSignaled())
            {
                LinkedListNode<KThread> node = syncObj.WaitingThreads.First;

                while (node != null)
                {
                    KThread thread = node.Value;

                    if ((thread.SchedFlags & ThreadSchedState.LowMask) == ThreadSchedState.Paused)
                    {
                        thread.SignaledObj   = syncObj;
                        thread.ObjSyncResult = 0;

                        thread.Reschedule(ThreadSchedState.Running);
                    }

                    node = node.Next;
                }
            }

            _system.CriticalSection.Leave();
        }
    }
}