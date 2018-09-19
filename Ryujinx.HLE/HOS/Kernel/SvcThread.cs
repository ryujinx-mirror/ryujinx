using ChocolArm64.State;
using Ryujinx.HLE.Logging;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
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
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid priority 0x{Priority:x8}!");

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
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core id 0x{ProcessorId:x8}!");

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

            KThread Thread = Process.HandleTable.GetData<KThread>(Handle);

            if (Thread != null)
            {
                long Result = Thread.Start();

                if (Result != 0)
                {
                    Device.Log.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
                }

                ThreadState.X0 = (ulong)Result;
            }
            else
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcExitThread(AThreadState ThreadState)
        {
            KThread CurrentThread = System.Scheduler.GetCurrentThread();

            CurrentThread.Exit();

            System.Scheduler.StopThread(CurrentThread);
        }

        private void SvcSleepThread(AThreadState ThreadState)
        {
            long Timeout = (long)ThreadState.X0;

            Device.Log.PrintDebug(LogClass.KernelSvc, "Timeout = 0x" + Timeout.ToString("x16"));

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

        private void SvcGetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.DynamicPriority;
            }
            else
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadPriority(AThreadState ThreadState)
        {
            int Handle   = (int)ThreadState.X0;
            int Priority = (int)ThreadState.X1;

            Device.Log.PrintDebug(LogClass.KernelSvc,
                "Handle = 0x"   + Handle  .ToString("x8") + ", " +
                "Priority = 0x" + Priority.ToString("x8"));

            //TODO: NPDM check.

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread == null)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            Thread.SetPriority(Priority);

            ThreadState.X0 = 0;
        }

        private void SvcGetThreadCoreMask(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X2;

            Device.Log.PrintDebug(LogClass.KernelSvc, "Handle = 0x" + Handle.ToString("x8"));

            KThread Thread = GetThread(ThreadState.Tpidr, Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.PreferredCore;
                ThreadState.X2 = (ulong)Thread.AffinityMask;
            }
            else
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadCoreMask(AThreadState ThreadState)
        {
            int  ThreadHandle  =  (int)ThreadState.X0;
            int  PrefferedCore =  (int)ThreadState.X1;
            long AffinityMask  = (long)ThreadState.X2;

            Device.Log.PrintDebug(LogClass.KernelSvc,
                "ThreadHandle = 0x"  + ThreadHandle .ToString("x8") + ", " +
                "PrefferedCore = 0x" + PrefferedCore.ToString("x8") + ", " +
                "AffinityMask = 0x"  + AffinityMask .ToString("x16"));

            if (PrefferedCore == -2)
            {
                //TODO: Get this value from the NPDM file.
                PrefferedCore = 0;

                AffinityMask = 1 << PrefferedCore;
            }
            else
            {
                //TODO: Check allowed cores from NPDM file.

                if ((uint)PrefferedCore > 3)
                {
                    if ((PrefferedCore | 2) != -1)
                    {
                        Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core id 0x{PrefferedCore:x8}!");

                        ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidCoreId);

                        return;
                    }
                }
                else if ((AffinityMask & (1 << PrefferedCore)) == 0)
                {
                    Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid core mask 0x{AffinityMask:x8}!");

                    ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMaskValue);

                    return;
                }
            }

            KThread Thread = GetThread(ThreadState.Tpidr, ThreadHandle);

            if (Thread == null)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            long Result = Thread.SetCoreAndAffinityMask(PrefferedCore, AffinityMask);

            if (Result != 0)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcGetCurrentProcessorNumber(AThreadState ThreadState)
        {
            ThreadState.X0 = (ulong)Process.GetThread(ThreadState.Tpidr).CurrentCore;
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
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadActivity(AThreadState ThreadState)
        {
            int  Handle = (int)ThreadState.X0;
            bool Pause  = (int)ThreadState.X1 == 1;

            KThread Thread = Process.HandleTable.GetData<KThread>(Handle);

            if (Thread == null)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Thread.Owner != Process)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread owner process!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            long Result = Thread.SetActivity(Pause);

            if (Result != 0)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcGetThreadContext3(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            int  Handle   =  (int)ThreadState.X1;

            KThread Thread = Process.HandleTable.GetData<KThread>(Handle);

            if (Thread == null)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (Process.GetThread(ThreadState.Tpidr) == Thread)
            {
                Device.Log.PrintWarning(LogClass.KernelSvc, $"Thread handle 0x{Handle:x8} is current thread!");

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