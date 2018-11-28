using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KConditionVariable
    {
        public static void Wait(Horizon System, LinkedList<KThread> ThreadList, object Mutex, long Timeout)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.CriticalSection.Enter();

            Monitor.Exit(Mutex);

            CurrentThread.Withholder = ThreadList;

            CurrentThread.Reschedule(ThreadSchedState.Paused);

            CurrentThread.WithholderNode = ThreadList.AddLast(CurrentThread);

            if (CurrentThread.ShallBeTerminated ||
                CurrentThread.SchedFlags == ThreadSchedState.TerminationPending)
            {
                ThreadList.Remove(CurrentThread.WithholderNode);

                CurrentThread.Reschedule(ThreadSchedState.Running);

                CurrentThread.Withholder = null;

                System.CriticalSection.Leave();
            }
            else
            {
                if (Timeout > 0)
                {
                    System.TimeManager.ScheduleFutureInvocation(CurrentThread, Timeout);
                }

                System.CriticalSection.Leave();

                if (Timeout > 0)
                {
                    System.TimeManager.UnscheduleFutureInvocation(CurrentThread);
                }
            }

            Monitor.Enter(Mutex);
        }

        public static void NotifyAll(Horizon System, LinkedList<KThread> ThreadList)
        {
            System.CriticalSection.Enter();

            LinkedListNode<KThread> Node = ThreadList.First;

            for (; Node != null; Node = ThreadList.First)
            {
                KThread Thread = Node.Value;

                ThreadList.Remove(Thread.WithholderNode);

                Thread.Withholder = null;

                Thread.Reschedule(ThreadSchedState.Running);
            }

            System.CriticalSection.Leave();
        }
    }
}