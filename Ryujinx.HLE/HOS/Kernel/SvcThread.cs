using ChocolArm64.State;
using Ryujinx.Common.Logging;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void CreateThread64(CpuThreadState threadState)
        {
            ulong entrypoint =      threadState.X1;
            ulong argsPtr    =      threadState.X2;
            ulong stackTop   =      threadState.X3;
            int   priority   = (int)threadState.X4;
            int   cpuCore    = (int)threadState.X5;

            KernelResult result = CreateThread(entrypoint, argsPtr, stackTop, priority, cpuCore, out int handle);

            threadState.X0 = (ulong)result;
            threadState.X1 = (ulong)handle;
        }

        private KernelResult CreateThread(
            ulong   entrypoint,
            ulong   argsPtr,
            ulong   stackTop,
            int     priority,
            int     cpuCore,
            out int handle)
        {
            handle = 0;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (cpuCore == -2)
            {
                cpuCore = currentProcess.DefaultCpuCore;
            }

            if ((uint)cpuCore >= KScheduler.CpuCoresCount || !currentProcess.IsCpuCoreAllowed(cpuCore))
            {
                return KernelResult.InvalidCpuCore;
            }

            if ((uint)priority >= KScheduler.PrioritiesCount || !currentProcess.IsPriorityAllowed(priority))
            {
                return KernelResult.InvalidPriority;
            }

            long timeout = KTimeManager.ConvertMillisecondsToNanoseconds(100);

            if (currentProcess.ResourceLimit != null &&
               !currentProcess.ResourceLimit.Reserve(LimitableResource.Thread, 1, timeout))
            {
                return KernelResult.ResLimitExceeded;
            }

            KThread thread = new KThread(_system);

            KernelResult result = currentProcess.InitializeThread(
                thread,
                entrypoint,
                argsPtr,
                stackTop,
                priority,
                cpuCore);

            if (result != KernelResult.Success)
            {
                currentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);

                return result;
            }

            result = _process.HandleTable.GenerateHandle(thread, out handle);

            if (result != KernelResult.Success)
            {
                thread.Terminate();

                currentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);
            }

            return result;
        }

        private void SvcStartThread(CpuThreadState threadState)
        {
            int handle = (int)threadState.X0;

            KThread thread = _process.HandleTable.GetObject<KThread>(handle);

            if (thread != null)
            {
                KernelResult result = thread.Start();

                if (result != KernelResult.Success)
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
                }

                threadState.X0 = (ulong)result;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcExitThread(CpuThreadState threadState)
        {
            KThread currentThread = _system.Scheduler.GetCurrentThread();

            _system.Scheduler.ExitThread(currentThread);

            currentThread.Exit();
        }

        private void SvcSleepThread(CpuThreadState threadState)
        {
            long timeout = (long)threadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "Timeout = 0x" + timeout.ToString("x16"));

            KThread currentThread = _system.Scheduler.GetCurrentThread();

            if (timeout < 1)
            {
                switch (timeout)
                {
                    case  0: currentThread.Yield();                        break;
                    case -1: currentThread.YieldWithLoadBalancing();       break;
                    case -2: currentThread.YieldAndWaitForLoadBalancing(); break;
                }
            }
            else
            {
                currentThread.Sleep(timeout);

                threadState.X0 = 0;
            }
        }

        private void SvcGetThreadPriority(CpuThreadState threadState)
        {
            int handle = (int)threadState.X1;

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                threadState.X0 = 0;
                threadState.X1 = (ulong)thread.DynamicPriority;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadPriority(CpuThreadState threadState)
        {
            int handle   = (int)threadState.X0;
            int priority = (int)threadState.X1;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Handle = 0x"   + handle  .ToString("x8") + ", " +
                "Priority = 0x" + priority.ToString("x8"));

            //TODO: NPDM check.

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            thread.SetPriority(priority);

            threadState.X0 = 0;
        }

        private void SvcGetThreadCoreMask(CpuThreadState threadState)
        {
            int handle = (int)threadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc, "Handle = 0x" + handle.ToString("x8"));

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                threadState.X0 = 0;
                threadState.X1 = (ulong)thread.PreferredCore;
                threadState.X2 = (ulong)thread.AffinityMask;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SetThreadCoreMask64(CpuThreadState threadState)
        {
            int  handle        =  (int)threadState.X0;
            int  preferredCore =  (int)threadState.X1;
            long affinityMask  = (long)threadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Handle = 0x"        + handle       .ToString("x8") + ", " +
                "PreferredCore = 0x" + preferredCore.ToString("x8") + ", " +
                "AffinityMask = 0x"  + affinityMask .ToString("x16"));

            KernelResult result = SetThreadCoreMask(handle, preferredCore, affinityMask);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
            }

            threadState.X0 = (ulong)result;
        }

        private KernelResult SetThreadCoreMask(int handle, int preferredCore, long affinityMask)
        {
            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (preferredCore == -2)
            {
                preferredCore = currentProcess.DefaultCpuCore;

                affinityMask = 1 << preferredCore;
            }
            else
            {
                if ((currentProcess.Capabilities.AllowedCpuCoresMask | affinityMask) !=
                     currentProcess.Capabilities.AllowedCpuCoresMask)
                {
                    return KernelResult.InvalidCpuCore;
                }

                if (affinityMask == 0)
                {
                    return KernelResult.InvalidCombination;
                }

                if ((uint)preferredCore > 3)
                {
                    if ((preferredCore | 2) != -1)
                    {
                        return KernelResult.InvalidCpuCore;
                    }
                }
                else if ((affinityMask & (1 << preferredCore)) == 0)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            return thread.SetCoreAndAffinityMask(preferredCore, affinityMask);
        }

        private void SvcGetCurrentProcessorNumber(CpuThreadState threadState)
        {
            threadState.X0 = (ulong)_system.Scheduler.GetCurrentThread().CurrentCore;
        }

        private void SvcGetThreadId(CpuThreadState threadState)
        {
            int handle = (int)threadState.X1;

            KThread thread = _process.HandleTable.GetKThread(handle);

            if (thread != null)
            {
                threadState.X0 = 0;
                threadState.X1 = (ulong)thread.ThreadUid;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadActivity(CpuThreadState threadState)
        {
            int  handle = (int)threadState.X0;
            bool pause  = (int)threadState.X1 == 1;

            KThread thread = _process.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (thread.Owner != _system.Scheduler.GetCurrentProcess())
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread, it belongs to another process.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (thread == _system.Scheduler.GetCurrentThread())
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid thread, current thread is not accepted.");

                threadState.X0 = (ulong)KernelResult.InvalidThread;

                return;
            }

            long result = thread.SetActivity(pause);

            if (result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcGetThreadContext3(CpuThreadState threadState)
        {
            long position = (long)threadState.X0;
            int  handle   =  (int)threadState.X1;

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();
            KThread  currentThread  = _system.Scheduler.GetCurrentThread();

            KThread thread = _process.HandleTable.GetObject<KThread>(handle);

            if (thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (thread.Owner != currentProcess)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread, it belongs to another process.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (currentThread == thread)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid thread, current thread is not accepted.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidThread);

                return;
            }

            _memory.WriteUInt64(position + 0x0,  thread.Context.ThreadState.X0);
            _memory.WriteUInt64(position + 0x8,  thread.Context.ThreadState.X1);
            _memory.WriteUInt64(position + 0x10, thread.Context.ThreadState.X2);
            _memory.WriteUInt64(position + 0x18, thread.Context.ThreadState.X3);
            _memory.WriteUInt64(position + 0x20, thread.Context.ThreadState.X4);
            _memory.WriteUInt64(position + 0x28, thread.Context.ThreadState.X5);
            _memory.WriteUInt64(position + 0x30, thread.Context.ThreadState.X6);
            _memory.WriteUInt64(position + 0x38, thread.Context.ThreadState.X7);
            _memory.WriteUInt64(position + 0x40, thread.Context.ThreadState.X8);
            _memory.WriteUInt64(position + 0x48, thread.Context.ThreadState.X9);
            _memory.WriteUInt64(position + 0x50, thread.Context.ThreadState.X10);
            _memory.WriteUInt64(position + 0x58, thread.Context.ThreadState.X11);
            _memory.WriteUInt64(position + 0x60, thread.Context.ThreadState.X12);
            _memory.WriteUInt64(position + 0x68, thread.Context.ThreadState.X13);
            _memory.WriteUInt64(position + 0x70, thread.Context.ThreadState.X14);
            _memory.WriteUInt64(position + 0x78, thread.Context.ThreadState.X15);
            _memory.WriteUInt64(position + 0x80, thread.Context.ThreadState.X16);
            _memory.WriteUInt64(position + 0x88, thread.Context.ThreadState.X17);
            _memory.WriteUInt64(position + 0x90, thread.Context.ThreadState.X18);
            _memory.WriteUInt64(position + 0x98, thread.Context.ThreadState.X19);
            _memory.WriteUInt64(position + 0xa0, thread.Context.ThreadState.X20);
            _memory.WriteUInt64(position + 0xa8, thread.Context.ThreadState.X21);
            _memory.WriteUInt64(position + 0xb0, thread.Context.ThreadState.X22);
            _memory.WriteUInt64(position + 0xb8, thread.Context.ThreadState.X23);
            _memory.WriteUInt64(position + 0xc0, thread.Context.ThreadState.X24);
            _memory.WriteUInt64(position + 0xc8, thread.Context.ThreadState.X25);
            _memory.WriteUInt64(position + 0xd0, thread.Context.ThreadState.X26);
            _memory.WriteUInt64(position + 0xd8, thread.Context.ThreadState.X27);
            _memory.WriteUInt64(position + 0xe0, thread.Context.ThreadState.X28);
            _memory.WriteUInt64(position + 0xe8, thread.Context.ThreadState.X29);
            _memory.WriteUInt64(position + 0xf0, thread.Context.ThreadState.X30);
            _memory.WriteUInt64(position + 0xf8, thread.Context.ThreadState.X31);

            _memory.WriteInt64(position + 0x100, thread.LastPc);

            _memory.WriteUInt64(position + 0x108, (ulong)thread.Context.ThreadState.Psr);

            _memory.WriteVector128(position + 0x110, thread.Context.ThreadState.V0);
            _memory.WriteVector128(position + 0x120, thread.Context.ThreadState.V1);
            _memory.WriteVector128(position + 0x130, thread.Context.ThreadState.V2);
            _memory.WriteVector128(position + 0x140, thread.Context.ThreadState.V3);
            _memory.WriteVector128(position + 0x150, thread.Context.ThreadState.V4);
            _memory.WriteVector128(position + 0x160, thread.Context.ThreadState.V5);
            _memory.WriteVector128(position + 0x170, thread.Context.ThreadState.V6);
            _memory.WriteVector128(position + 0x180, thread.Context.ThreadState.V7);
            _memory.WriteVector128(position + 0x190, thread.Context.ThreadState.V8);
            _memory.WriteVector128(position + 0x1a0, thread.Context.ThreadState.V9);
            _memory.WriteVector128(position + 0x1b0, thread.Context.ThreadState.V10);
            _memory.WriteVector128(position + 0x1c0, thread.Context.ThreadState.V11);
            _memory.WriteVector128(position + 0x1d0, thread.Context.ThreadState.V12);
            _memory.WriteVector128(position + 0x1e0, thread.Context.ThreadState.V13);
            _memory.WriteVector128(position + 0x1f0, thread.Context.ThreadState.V14);
            _memory.WriteVector128(position + 0x200, thread.Context.ThreadState.V15);
            _memory.WriteVector128(position + 0x210, thread.Context.ThreadState.V16);
            _memory.WriteVector128(position + 0x220, thread.Context.ThreadState.V17);
            _memory.WriteVector128(position + 0x230, thread.Context.ThreadState.V18);
            _memory.WriteVector128(position + 0x240, thread.Context.ThreadState.V19);
            _memory.WriteVector128(position + 0x250, thread.Context.ThreadState.V20);
            _memory.WriteVector128(position + 0x260, thread.Context.ThreadState.V21);
            _memory.WriteVector128(position + 0x270, thread.Context.ThreadState.V22);
            _memory.WriteVector128(position + 0x280, thread.Context.ThreadState.V23);
            _memory.WriteVector128(position + 0x290, thread.Context.ThreadState.V24);
            _memory.WriteVector128(position + 0x2a0, thread.Context.ThreadState.V25);
            _memory.WriteVector128(position + 0x2b0, thread.Context.ThreadState.V26);
            _memory.WriteVector128(position + 0x2c0, thread.Context.ThreadState.V27);
            _memory.WriteVector128(position + 0x2d0, thread.Context.ThreadState.V28);
            _memory.WriteVector128(position + 0x2e0, thread.Context.ThreadState.V29);
            _memory.WriteVector128(position + 0x2f0, thread.Context.ThreadState.V30);
            _memory.WriteVector128(position + 0x300, thread.Context.ThreadState.V31);

            _memory.WriteInt32(position + 0x310, thread.Context.ThreadState.Fpcr);
            _memory.WriteInt32(position + 0x314, thread.Context.ThreadState.Fpsr);
            _memory.WriteInt64(position + 0x318, thread.Context.ThreadState.Tpidr);

            threadState.X0 = 0;
        }
    }
}