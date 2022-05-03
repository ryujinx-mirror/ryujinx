using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelStatic
    {
        [ThreadStatic]
        private static KernelContext Context;

        [ThreadStatic]
        private static KThread CurrentThread;

        public static KernelResult StartInitialProcess(
            KernelContext context,
            ProcessCreationInfo creationInfo,
            ReadOnlySpan<int> capabilities,
            int mainThreadPriority,
            ThreadStart customThreadStart)
        {
            KProcess process = new KProcess(context);

            KernelResult result = process.Initialize(
                creationInfo,
                capabilities,
                context.ResourceLimit,
                MemoryRegion.Service,
                null,
                customThreadStart);

            if (result != KernelResult.Success)
            {
                return result;
            }

            process.DefaultCpuCore = 3;

            context.Processes.TryAdd(process.Pid, process);

            return process.Start(mainThreadPriority, 0x1000UL);
        }

        internal static void SetKernelContext(KernelContext context, KThread thread)
        {
            Context = context;
            CurrentThread = thread;
        }

        internal static KThread GetCurrentThread()
        {
            return CurrentThread;
        }

        internal static KProcess GetCurrentProcess()
        {
            return GetCurrentThread().Owner;
        }

        internal static KProcess GetProcessByPid(ulong pid)
        {
            if (Context.Processes.TryGetValue(pid, out KProcess process))
            {
                return process;
            }

            return null;
        }
    }
}
