using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelStatic
    {
        [ThreadStatic]
        private static KernelContext Context;

        public static void YieldUntilCompletion(Action action)
        {
            YieldUntilCompletion(Task.Factory.StartNew(action));
        }

        public static void YieldUntilCompletion(Task task)
        {
            KThread currentThread = Context.Scheduler.GetCurrentThread();

            Context.CriticalSection.Enter();

            currentThread.Reschedule(ThreadSchedState.Paused);

            task.ContinueWith((antecedent) =>
            {
                currentThread.Reschedule(ThreadSchedState.Running);
            });

            Context.CriticalSection.Leave();
        }

        internal static void SetKernelContext(KernelContext context)
        {
            Context = context;
        }
    }
}
