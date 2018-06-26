using ChocolArm64.State;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using System.Threading;

using static Ryujinx.HLE.OsHle.ErrorCode;

namespace Ryujinx.HLE.OsHle.Kernel
{
    partial class SvcHandler
    {
        private void SvcCreateThread(AThreadState ThreadState)
        {
            long EntryPoint  = (long)ThreadState.X1;
            long ArgsPtr     = (long)ThreadState.X2;
            long StackTop    = (long)ThreadState.X3;
            int  Priority    =  (int)ThreadState.X4;
            int  ProcessorId =  (int)ThreadState.X5;

            if ((uint)Priority > 0x3f)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid priority 0x{Priority:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidPriority);

                return;
            }

            if (ProcessorId == -2)
            {
                //TODO: Get this value from the NPDM file.
                ProcessorId = 0;
            }
            else if ((uint)ProcessorId > 3)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core id 0x{ProcessorId:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidCoreId);

                return;
            }

            int Handle = Process.MakeThread(
                EntryPoint,
                StackTop,
                ArgsPtr,
                Priority,
                ProcessorId);

            ThreadState.X0 = 0;
            ThreadState.X1 = (ulong)Handle;
        }

        private void SvcStartThread(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            KThread NewThread = Process.HandleTable.GetData<KThread>(Handle);

            if (NewThread != null)
            {
                Process.Scheduler.StartThread(NewThread);
                Process.Scheduler.SetReschedule(NewThread.ProcessorId);

                ThreadState.X0 = 0;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcExitThread(AThreadState ThreadState)
        {
            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            CurrThread.Thread.StopExecution();
        }

        private void SvcSleepThread(AThreadState ThreadState)
        {
            ulong TimeoutNs = ThreadState.X0;

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Timeout = " + TimeoutNs.ToString("x16"));

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            if (TimeoutNs == 0)
            {
                Process.Scheduler.Yield(CurrThread);
            }
            else
            {
                Process.Scheduler.Suspend(CurrThread);

                Thread.Sleep(NsTimeConverter.GetTimeMs(TimeoutNs));

                Process.Scheduler.Resume(CurrThread);
            }
        }

        private void SvcGetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.ActualPriority;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadPriority(AThreadState ThreadState)
        {
            int Handle   = (int)ThreadState.X0;
            int Priority = (int)ThreadState.X1;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "Handle = "   + Handle  .ToString("x8") + ", " +
                "Priority = " + Priority.ToString("x8"));

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                Thread.SetPriority(Priority);

                ThreadState.X0 = 0;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcGetThreadCoreMask(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X2;

            Ns.Log.PrintDebug(LogClass.KernelSvc, "Handle = " + Handle.ToString("x8"));

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.IdealCore;
                ThreadState.X2 = (ulong)Thread.CoreMask;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadCoreMask(AThreadState ThreadState)
        {
            int  Handle    =  (int)ThreadState.X0;
            int  IdealCore =  (int)ThreadState.X1;
            long CoreMask  = (long)ThreadState.X2;

            Ns.Log.PrintDebug(LogClass.KernelSvc,
                "Handle = "    + Handle   .ToString("x8") + ", " +
                "IdealCore = " + IdealCore.ToString("x8") + ", " +
                "CoreMask = "  + CoreMask .ToString("x16"));

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (IdealCore == -2)
            {
                //TODO: Get this value from the NPDM file.
                IdealCore = 0;

                CoreMask = 1 << IdealCore;
            }
            else
            {
                if ((uint)IdealCore > 3)
                {
                    if ((IdealCore | 2) != -1)
                    {
                        Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core id 0x{IdealCore:x8}!");

                        ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidCoreId);

                        return;
                    }
                }
                else if ((CoreMask & (1 << IdealCore)) == 0)
                {
                    Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core mask 0x{CoreMask:x8}!");

                    ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidCoreMask);

                    return;
                }
            }

            if (Thread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            //-1 is used as "don't care", so the IdealCore value is ignored.
            //-2 is used as "use NPDM default core id" (handled above).
            //-3 is used as "don't update", the old IdealCore value is kept.
            if (IdealCore == -3 && (CoreMask & (1 << Thread.IdealCore)) == 0)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core mask 0x{CoreMask:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidCoreMask);

                return;
            }

            Process.Scheduler.ChangeCore(Thread, IdealCore, (int)CoreMask);

            ThreadState.X0 = 0;
        }

        private void SvcGetCurrentProcessorNumber(AThreadState ThreadState)
        {
            ThreadState.X0 = (ulong)Process.GetThread(ThreadState.Tpidr).ActualCore;
        }

        private void SvcGetThreadId(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.ThreadId;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadActivity(AThreadState ThreadState)
        {
            int  Handle = (int)ThreadState.X0;
            bool Active = (int)ThreadState.X1 == 0;

            KThread Thread = Process.HandleTable.GetData<KThread>(Handle);

            if (Thread != null)
            {
                Process.Scheduler.SetThreadActivity(Thread, Active);

                ThreadState.X0 = 0;
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcGetThreadContext3(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            int  Handle   =  (int)ThreadState.X1;

            KThread Thread = Process.HandleTable.GetData<KThread>(Handle);

            if (Thread == null)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Process.GetThread(ThreadState.Tpidr) == Thread)
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Thread handle 0x{Handle:x8} is current thread!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidThread);

                return;
            }

            if (Process.Scheduler.IsThreadRunning(Thread))
            {
                Ns.Log.PrintWarning(LogClass.KernelSvc, $"Thread handle 0x{Handle:x8} is running!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);

                return;
            }

            Memory.WriteUInt64(Position + 0x0,  ThreadState.X0);
            Memory.WriteUInt64(Position + 0x8,  ThreadState.X1);
            Memory.WriteUInt64(Position + 0x10, ThreadState.X2);
            Memory.WriteUInt64(Position + 0x18, ThreadState.X3);
            Memory.WriteUInt64(Position + 0x20, ThreadState.X4);
            Memory.WriteUInt64(Position + 0x28, ThreadState.X5);
            Memory.WriteUInt64(Position + 0x30, ThreadState.X6);
            Memory.WriteUInt64(Position + 0x38, ThreadState.X7);
            Memory.WriteUInt64(Position + 0x40, ThreadState.X8);
            Memory.WriteUInt64(Position + 0x48, ThreadState.X9);
            Memory.WriteUInt64(Position + 0x50, ThreadState.X10);
            Memory.WriteUInt64(Position + 0x58, ThreadState.X11);
            Memory.WriteUInt64(Position + 0x60, ThreadState.X12);
            Memory.WriteUInt64(Position + 0x68, ThreadState.X13);
            Memory.WriteUInt64(Position + 0x70, ThreadState.X14);
            Memory.WriteUInt64(Position + 0x78, ThreadState.X15);
            Memory.WriteUInt64(Position + 0x80, ThreadState.X16);
            Memory.WriteUInt64(Position + 0x88, ThreadState.X17);
            Memory.WriteUInt64(Position + 0x90, ThreadState.X18);
            Memory.WriteUInt64(Position + 0x98, ThreadState.X19);
            Memory.WriteUInt64(Position + 0xa0, ThreadState.X20);
            Memory.WriteUInt64(Position + 0xa8, ThreadState.X21);
            Memory.WriteUInt64(Position + 0xb0, ThreadState.X22);
            Memory.WriteUInt64(Position + 0xb8, ThreadState.X23);
            Memory.WriteUInt64(Position + 0xc0, ThreadState.X24);
            Memory.WriteUInt64(Position + 0xc8, ThreadState.X25);
            Memory.WriteUInt64(Position + 0xd0, ThreadState.X26);
            Memory.WriteUInt64(Position + 0xd8, ThreadState.X27);
            Memory.WriteUInt64(Position + 0xe0, ThreadState.X28);
            Memory.WriteUInt64(Position + 0xe8, ThreadState.X29);
            Memory.WriteUInt64(Position + 0xf0, ThreadState.X30);
            Memory.WriteUInt64(Position + 0xf8, ThreadState.X31);

            Memory.WriteInt64(Position + 0x100, Thread.LastPc);

            Memory.WriteUInt64(Position + 0x108, (ulong)ThreadState.Psr);

            Memory.WriteVector128(Position + 0x110, ThreadState.V0);
            Memory.WriteVector128(Position + 0x120, ThreadState.V1);
            Memory.WriteVector128(Position + 0x130, ThreadState.V2);
            Memory.WriteVector128(Position + 0x140, ThreadState.V3);
            Memory.WriteVector128(Position + 0x150, ThreadState.V4);
            Memory.WriteVector128(Position + 0x160, ThreadState.V5);
            Memory.WriteVector128(Position + 0x170, ThreadState.V6);
            Memory.WriteVector128(Position + 0x180, ThreadState.V7);
            Memory.WriteVector128(Position + 0x190, ThreadState.V8);
            Memory.WriteVector128(Position + 0x1a0, ThreadState.V9);
            Memory.WriteVector128(Position + 0x1b0, ThreadState.V10);
            Memory.WriteVector128(Position + 0x1c0, ThreadState.V11);
            Memory.WriteVector128(Position + 0x1d0, ThreadState.V12);
            Memory.WriteVector128(Position + 0x1e0, ThreadState.V13);
            Memory.WriteVector128(Position + 0x1f0, ThreadState.V14);
            Memory.WriteVector128(Position + 0x200, ThreadState.V15);
            Memory.WriteVector128(Position + 0x210, ThreadState.V16);
            Memory.WriteVector128(Position + 0x220, ThreadState.V17);
            Memory.WriteVector128(Position + 0x230, ThreadState.V18);
            Memory.WriteVector128(Position + 0x240, ThreadState.V19);
            Memory.WriteVector128(Position + 0x250, ThreadState.V20);
            Memory.WriteVector128(Position + 0x260, ThreadState.V21);
            Memory.WriteVector128(Position + 0x270, ThreadState.V22);
            Memory.WriteVector128(Position + 0x280, ThreadState.V23);
            Memory.WriteVector128(Position + 0x290, ThreadState.V24);
            Memory.WriteVector128(Position + 0x2a0, ThreadState.V25);
            Memory.WriteVector128(Position + 0x2b0, ThreadState.V26);
            Memory.WriteVector128(Position + 0x2c0, ThreadState.V27);
            Memory.WriteVector128(Position + 0x2d0, ThreadState.V28);
            Memory.WriteVector128(Position + 0x2e0, ThreadState.V29);
            Memory.WriteVector128(Position + 0x2f0, ThreadState.V30);
            Memory.WriteVector128(Position + 0x300, ThreadState.V31);

            Memory.WriteInt32(Position + 0x310, ThreadState.Fpcr);
            Memory.WriteInt32(Position + 0x314, ThreadState.Fpsr);
            Memory.WriteInt64(Position + 0x318, ThreadState.Tpidr);

            ThreadState.X0 = 0;
        }
    }
}