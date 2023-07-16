using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    static class KConditionVariable
    {
        public static void Wait(KernelContext context, LinkedList<KThread> threadList, object mutex, long timeout)
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            context.CriticalSection.Enter();

            Monitor.Exit(mutex);

            currentThread.Withholder = threadList;

            currentThread.Reschedule(ThreadSchedState.Paused);

            currentThread.WithholderNode = threadList.AddLast(currentThread);

            if (currentThread.TerminationRequested)
            {
                threadList.Remove(currentThread.WithholderNode);

                currentThread.Reschedule(ThreadSchedState.Running);

                currentThread.Withholder = null;

                context.CriticalSection.Leave();
            }
            else
            {
                if (timeout > 0)
                {
                    context.TimeManager.ScheduleFutureInvocation(currentThread, timeout);
                }

                context.CriticalSection.Leave();

                if (timeout > 0)
                {
                    context.TimeManager.UnscheduleFutureInvocation(currentThread);
                }
            }

            Monitor.Enter(mutex);
        }

        public static void NotifyAll(KernelContext context, LinkedList<KThread> threadList)
        {
            context.CriticalSection.Enter();

            LinkedListNode<KThread> node = threadList.First;

            for (; node != null; node = threadList.First)
            {
                KThread thread = node.Value;

                threadList.Remove(thread.WithholderNode);

                thread.Withholder = null;

                thread.Reschedule(ThreadSchedState.Running);
            }

            context.CriticalSection.Leave();
        }
    }
}
