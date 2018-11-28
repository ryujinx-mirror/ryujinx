using ChocolArm64.State;
using Ryujinx.Common.Logging;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void CreateThread64(CpuThreadState ThreadState)
        {
            ulong Entrypoint =      ThreadState.X1;
            ulong ArgsPtr    =      ThreadState.X2;
            ulong StackTop   =      ThreadState.X3;
            int   Priority   = (int)ThreadState.X4;
            int   CpuCore    = (int)ThreadState.X5;

            KernelResult Result = CreateThread(Entrypoint, ArgsPtr, StackTop, Priority, CpuCore, out int Handle);

            ThreadState.X0 = (ulong)Result;
            ThreadState.X1 = (ulong)Handle;
        }

        private KernelResult CreateThread(
            ulong   Entrypoint,
            ulong   ArgsPtr,
            ulong   StackTop,
            int     Priority,
            int     CpuCore,
            out int Handle)
        {
            Handle = 0;

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (CpuCore == -2)
            {
                CpuCore = CurrentProcess.DefaultCpuCore;
            }

            if ((uint)CpuCore >= KScheduler.CpuCoresCount || !CurrentProcess.IsCpuCoreAllowed(CpuCore))
            {
                return KernelResult.InvalidCpuCore;
            }

            if ((uint)Priority >= KScheduler.PrioritiesCount || !CurrentProcess.IsPriorityAllowed(Priority))
            {
                return KernelResult.InvalidPriority;
            }

            long Timeout = KTimeManager.ConvertMillisecondsToNanoseconds(100);

            if (CurrentProcess.ResourceLimit != null &&
               !CurrentProcess.ResourceLimit.Reserve(LimitableResource.Thread, 1, Timeout))
            {
                return KernelResult.ResLimitExceeded;
            }

            KThread Thread = new KThread(System);

            KernelResult Result = CurrentProcess.InitializeThread(
                Thread,
                Entrypoint,
                ArgsPtr,
                StackTop,
                Priority,
                CpuCore);

            if (Result != KernelResult.Success)
            {
                CurrentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);

                return Result;
            }

            Result = Process.HandleTable.GenerateHandle(Thread, out Handle);

            if (Result != KernelResult.Success)
            {
                Thread.Terminate();

                CurrentProcess.ResourceLimit?.Release(LimitableResource.Thread, 1);
            }

            return Result;
        }

        private void SvcStartThread(CpuThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            KThread Thread = Process.HandleTable.GetObject<KThread>(Handle);

            if (Thread != null)
            {
                KernelResult Result = Thread.Start();

                if (Result != KernelResult.Success)
                {
                    Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
                }

                ThreadState.X0 = (ulong)Result;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcExitThread(CpuThreadState ThreadState)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            System.Scheduler.ExitThread(CurrentThread);

            CurrentThread.Exit();
        }

        private void SvcSleepThread(CpuThreadState ThreadState)
        {
            long Timeout = (long)ThreadState.X0;

            Logger.PrintDebug(LogClass.KernelSvc, "Timeout = 0x" + Timeout.ToString("x16"));

            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            if (Timeout < 1)
            {
                switch (Timeout)
                {
                    case  0: CurrentThread.Yield();                        break;
                    case -1: CurrentThread.YieldWithLoadBalancing();       break;
                    case -2: CurrentThread.YieldAndWaitForLoadBalancing(); break;
                }
            }
            else
            {
                CurrentThread.Sleep(Timeout);

                ThreadState.X0 = 0;
            }
        }

        private void SvcGetThreadPriority(CpuThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread Thread = Process.HandleTable.GetKThread(Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.DynamicPriority;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadPriority(CpuThreadState ThreadState)
        {
            int Handle   = (int)ThreadState.X0;
            int Priority = (int)ThreadState.X1;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Handle = 0x"   + Handle  .ToString("x8") + ", " +
                "Priority = 0x" + Priority.ToString("x8"));

            //TODO: NPDM check.

            KThread Thread = Process.HandleTable.GetKThread(Handle);

            if (Thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            Thread.SetPriority(Priority);

            ThreadState.X0 = 0;
        }

        private void SvcGetThreadCoreMask(CpuThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc, "Handle = 0x" + Handle.ToString("x8"));

            KThread Thread = Process.HandleTable.GetKThread(Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.PreferredCore;
                ThreadState.X2 = (ulong)Thread.AffinityMask;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SetThreadCoreMask64(CpuThreadState ThreadState)
        {
            int  Handle        =  (int)ThreadState.X0;
            int  PreferredCore =  (int)ThreadState.X1;
            long AffinityMask  = (long)ThreadState.X2;

            Logger.PrintDebug(LogClass.KernelSvc,
                "Handle = 0x"        + Handle       .ToString("x8") + ", " +
                "PreferredCore = 0x" + PreferredCore.ToString("x8") + ", " +
                "AffinityMask = 0x"  + AffinityMask .ToString("x16"));

            KernelResult Result = SetThreadCoreMask(Handle, PreferredCore, AffinityMask);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private KernelResult SetThreadCoreMask(int Handle, int PreferredCore, long AffinityMask)
        {
            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (PreferredCore == -2)
            {
                PreferredCore = CurrentProcess.DefaultCpuCore;

                AffinityMask = 1 << PreferredCore;
            }
            else
            {
                if ((CurrentProcess.Capabilities.AllowedCpuCoresMask | AffinityMask) !=
                     CurrentProcess.Capabilities.AllowedCpuCoresMask)
                {
                    return KernelResult.InvalidCpuCore;
                }

                if (AffinityMask == 0)
                {
                    return KernelResult.InvalidCombination;
                }

                if ((uint)PreferredCore > 3)
                {
                    if ((PreferredCore | 2) != -1)
                    {
                        return KernelResult.InvalidCpuCore;
                    }
                }
                else if ((AffinityMask & (1 << PreferredCore)) == 0)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KThread Thread = Process.HandleTable.GetKThread(Handle);

            if (Thread == null)
            {
                return KernelResult.InvalidHandle;
            }

            return Thread.SetCoreAndAffinityMask(PreferredCore, AffinityMask);
        }

        private void SvcGetCurrentProcessorNumber(CpuThreadState ThreadState)
        {
            ThreadState.X0 = (ulong)System.Scheduler.GetCurrentThread().CurrentCore;
        }

        private void SvcGetThreadId(CpuThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread Thread = Process.HandleTable.GetKThread(Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.ThreadUid;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadActivity(CpuThreadState ThreadState)
        {
            int  Handle = (int)ThreadState.X0;
            bool Pause  = (int)ThreadState.X1 == 1;

            KThread Thread = Process.HandleTable.GetObject<KThread>(Handle);

            if (Thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Thread.Owner != System.Scheduler.GetCurrentProcess())
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread, it belongs to another process.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Thread == System.Scheduler.GetCurrentThread())
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid thread, current thread is not accepted.");

                ThreadState.X0 = (ulong)KernelResult.InvalidThread;

                return;
            }

            long Result = Thread.SetActivity(Pause);

            if (Result != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcGetThreadContext3(CpuThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            int  Handle   =  (int)ThreadState.X1;

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();
            KThread  CurrentThread  = System.Scheduler.GetCurrentThread();

            KThread Thread = Process.HandleTable.GetObject<KThread>(Handle);

            if (Thread == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Thread.Owner != CurrentProcess)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid thread, it belongs to another process.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (CurrentThread == Thread)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid thread, current thread is not accepted.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidThread);

                return;
            }

            Memory.WriteUInt64(Position + 0x0,  Thread.Context.ThreadState.X0);
            Memory.WriteUInt64(Position + 0x8,  Thread.Context.ThreadState.X1);
            Memory.WriteUInt64(Position + 0x10, Thread.Context.ThreadState.X2);
            Memory.WriteUInt64(Position + 0x18, Thread.Context.ThreadState.X3);
            Memory.WriteUInt64(Position + 0x20, Thread.Context.ThreadState.X4);
            Memory.WriteUInt64(Position + 0x28, Thread.Context.ThreadState.X5);
            Memory.WriteUInt64(Position + 0x30, Thread.Context.ThreadState.X6);
            Memory.WriteUInt64(Position + 0x38, Thread.Context.ThreadState.X7);
            Memory.WriteUInt64(Position + 0x40, Thread.Context.ThreadState.X8);
            Memory.WriteUInt64(Position + 0x48, Thread.Context.ThreadState.X9);
            Memory.WriteUInt64(Position + 0x50, Thread.Context.ThreadState.X10);
            Memory.WriteUInt64(Position + 0x58, Thread.Context.ThreadState.X11);
            Memory.WriteUInt64(Position + 0x60, Thread.Context.ThreadState.X12);
            Memory.WriteUInt64(Position + 0x68, Thread.Context.ThreadState.X13);
            Memory.WriteUInt64(Position + 0x70, Thread.Context.ThreadState.X14);
            Memory.WriteUInt64(Position + 0x78, Thread.Context.ThreadState.X15);
            Memory.WriteUInt64(Position + 0x80, Thread.Context.ThreadState.X16);
            Memory.WriteUInt64(Position + 0x88, Thread.Context.ThreadState.X17);
            Memory.WriteUInt64(Position + 0x90, Thread.Context.ThreadState.X18);
            Memory.WriteUInt64(Position + 0x98, Thread.Context.ThreadState.X19);
            Memory.WriteUInt64(Position + 0xa0, Thread.Context.ThreadState.X20);
            Memory.WriteUInt64(Position + 0xa8, Thread.Context.ThreadState.X21);
            Memory.WriteUInt64(Position + 0xb0, Thread.Context.ThreadState.X22);
            Memory.WriteUInt64(Position + 0xb8, Thread.Context.ThreadState.X23);
            Memory.WriteUInt64(Position + 0xc0, Thread.Context.ThreadState.X24);
            Memory.WriteUInt64(Position + 0xc8, Thread.Context.ThreadState.X25);
            Memory.WriteUInt64(Position + 0xd0, Thread.Context.ThreadState.X26);
            Memory.WriteUInt64(Position + 0xd8, Thread.Context.ThreadState.X27);
            Memory.WriteUInt64(Position + 0xe0, Thread.Context.ThreadState.X28);
            Memory.WriteUInt64(Position + 0xe8, Thread.Context.ThreadState.X29);
            Memory.WriteUInt64(Position + 0xf0, Thread.Context.ThreadState.X30);
            Memory.WriteUInt64(Position + 0xf8, Thread.Context.ThreadState.X31);

            Memory.WriteInt64(Position + 0x100, Thread.LastPc);

            Memory.WriteUInt64(Position + 0x108, (ulong)Thread.Context.ThreadState.Psr);

            Memory.WriteVector128(Position + 0x110, Thread.Context.ThreadState.V0);
            Memory.WriteVector128(Position + 0x120, Thread.Context.ThreadState.V1);
            Memory.WriteVector128(Position + 0x130, Thread.Context.ThreadState.V2);
            Memory.WriteVector128(Position + 0x140, Thread.Context.ThreadState.V3);
            Memory.WriteVector128(Position + 0x150, Thread.Context.ThreadState.V4);
            Memory.WriteVector128(Position + 0x160, Thread.Context.ThreadState.V5);
            Memory.WriteVector128(Position + 0x170, Thread.Context.ThreadState.V6);
            Memory.WriteVector128(Position + 0x180, Thread.Context.ThreadState.V7);
            Memory.WriteVector128(Position + 0x190, Thread.Context.ThreadState.V8);
            Memory.WriteVector128(Position + 0x1a0, Thread.Context.ThreadState.V9);
            Memory.WriteVector128(Position + 0x1b0, Thread.Context.ThreadState.V10);
            Memory.WriteVector128(Position + 0x1c0, Thread.Context.ThreadState.V11);
            Memory.WriteVector128(Position + 0x1d0, Thread.Context.ThreadState.V12);
            Memory.WriteVector128(Position + 0x1e0, Thread.Context.ThreadState.V13);
            Memory.WriteVector128(Position + 0x1f0, Thread.Context.ThreadState.V14);
            Memory.WriteVector128(Position + 0x200, Thread.Context.ThreadState.V15);
            Memory.WriteVector128(Position + 0x210, Thread.Context.ThreadState.V16);
            Memory.WriteVector128(Position + 0x220, Thread.Context.ThreadState.V17);
            Memory.WriteVector128(Position + 0x230, Thread.Context.ThreadState.V18);
            Memory.WriteVector128(Position + 0x240, Thread.Context.ThreadState.V19);
            Memory.WriteVector128(Position + 0x250, Thread.Context.ThreadState.V20);
            Memory.WriteVector128(Position + 0x260, Thread.Context.ThreadState.V21);
            Memory.WriteVector128(Position + 0x270, Thread.Context.ThreadState.V22);
            Memory.WriteVector128(Position + 0x280, Thread.Context.ThreadState.V23);
            Memory.WriteVector128(Position + 0x290, Thread.Context.ThreadState.V24);
            Memory.WriteVector128(Position + 0x2a0, Thread.Context.ThreadState.V25);
            Memory.WriteVector128(Position + 0x2b0, Thread.Context.ThreadState.V26);
            Memory.WriteVector128(Position + 0x2c0, Thread.Context.ThreadState.V27);
            Memory.WriteVector128(Position + 0x2d0, Thread.Context.ThreadState.V28);
            Memory.WriteVector128(Position + 0x2e0, Thread.Context.ThreadState.V29);
            Memory.WriteVector128(Position + 0x2f0, Thread.Context.ThreadState.V30);
            Memory.WriteVector128(Position + 0x300, Thread.Context.ThreadState.V31);

            Memory.WriteInt32(Position + 0x310, Thread.Context.ThreadState.Fpcr);
            Memory.WriteInt32(Position + 0x314, Thread.Context.ThreadState.Fpsr);
            Memory.WriteInt64(Position + 0x318, Thread.Context.ThreadState.Tpidr);

            ThreadState.X0 = 0;
        }
    }
}