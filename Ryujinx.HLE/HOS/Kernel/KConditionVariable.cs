using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KConditionVariable
    {
        public static void Wait(Horizon system, LinkedList<KThread> threadList, object mutex, long timeout)
        {
            KThread currentThread = system.Scheduler.GetCurrentThread();

            system.CriticalSection.Enter();

            Monitor.Exit(mutex);

            currentThread.Withholder = threadList;

            currentThread.Reschedule(ThreadSchedState.Paused);

            currentThread.WithholderNode = threadList.AddLast(currentThread);

            if (currentThread.ShallBeTerminated ||
                currentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                threadList.Remove(currentThread.WithholderNode);

                currentThread.Reschedule(ThreadSchedState.Running);

                currentThread.Withholder = null;

                system.CriticalSection.Leave();
            }
            else
            {
                if (timeout > 0)
                {
                    system.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                system.CriticalSection.Leave();

                if (timeout > 0)
                {
                    system.TimeManager.UnscheduleFutureInvocation(currentThread);
                }
            }

            Monitor.Enter(mutex);
        }

        public static void NotifyAll(Horizon system, LinkedList<KThread> threadList)
        {
            system.CriticalSection.Enter();

            LinkedListNode<KThread> node = threadList.First;

            for (; node != null; node = threadList.First)
            {
                KThread thread = node.Value;

                threadList.Remove(thread.WithholderNode);

                thread.Withholder = null;

                thread.Reschedule(ThreadSchedState.Running);
            }

            system.CriticalSection.Leave();
        }
    }
}