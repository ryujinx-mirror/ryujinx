using ChocolArm64.State;
using Ryujinx.Core.OsHle.Handles;
using System.Threading;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Kernel
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

            if (ProcessorId == -2)
            {
                //TODO: Get this value from the NPDM file.
                ProcessorId = 0;
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

            KThread CurrThread = Process.HandleTable.GetData<KThread>(Handle);

            if (CurrThread != null)
            {
                Process.Scheduler.StartThread(CurrThread);

                Process.Scheduler.Yield(Process.GetThread(ThreadState.Tpidr));

                ThreadState.X0 = 0;
            }
            else
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

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
            ulong NanoSecs = ThreadState.X0;

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            if (NanoSecs == 0)
            {
                Process.Scheduler.Yield(CurrThread);
            }
            else
            {
                Process.Scheduler.Suspend(CurrThread.ProcessorId);

                Thread.Sleep((int)(NanoSecs / 1000000));

                Process.Scheduler.Resume(CurrThread);
            }
        }

        private void SvcGetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread CurrThread = Process.HandleTable.GetData<KThread>(Handle);

            if (CurrThread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)CurrThread.Priority;
            }
            else
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadPriority(AThreadState ThreadState)
        {
            int Prio   = (int)ThreadState.X0;
            int Handle = (int)ThreadState.X1;

            KThread CurrThread = Process.HandleTable.GetData<KThread>(Handle);

            if (CurrThread != null)
            {
                CurrThread.Priority = Prio;

                ThreadState.X0 = 0;
            }
            else
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }

        private void SvcSetThreadCoreMask(AThreadState ThreadState)
        {
            ThreadState.X0 = 0;

            //TODO: Error codes.
        }

        private void SvcGetCurrentProcessorNumber(AThreadState ThreadState)
        {
            ThreadState.X0 = (ulong)Process.GetThread(ThreadState.Tpidr).ProcessorId;
        }

        private void SvcGetThreadId(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            KThread CurrThread = Process.HandleTable.GetData<KThread>(Handle);

            if (CurrThread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)CurrThread.ThreadId;
            }
            else
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to GetThreadId on invalid thread handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }
        }
    }
}