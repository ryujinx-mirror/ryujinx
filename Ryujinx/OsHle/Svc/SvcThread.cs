using ChocolArm64.State;
using Ryujinx.OsHle.Handles;
using System.Threading;

namespace Ryujinx.OsHle.Svc
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

            if (Ns.Os.TryGetProcess(ThreadState.ProcessId, out Process Process))
            {
                if (ProcessorId == -2)
                {
                    ProcessorId = 0;
                }

                int Handle = Process.MakeThread(
                    EntryPoint,
                    StackTop,
                    ArgsPtr,
                    Priority,
                    ProcessorId);

                ThreadState.X0 = (int)SvcResult.Success;
                ThreadState.X1 = (ulong)Handle;
            }

            //TODO: Error codes.
        }

        private void SvcStartThread(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            HThread Thread = Ns.Os.Handles.GetData<HThread>(Handle);

            if (Thread != null)
            {
                Process.Scheduler.StartThread(Thread);

                ThreadState.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private void SvcSleepThread(AThreadState ThreadState)
        {           
            ulong NanoSecs = ThreadState.X0;

            if (Process.TryGetThread(ThreadState.Tpidr, out HThread CurrThread))
            {
                Process.Scheduler.Yield(CurrThread);
            }
            else
            {
                Logging.Error($"Thread with TPIDR_EL0 0x{ThreadState.Tpidr:x16} not found!");
            }

            Thread.Sleep((int)(NanoSecs / 1000000));
        }

        private void SvcGetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            HThread Thread = Ns.Os.Handles.GetData<HThread>(Handle);

            if (Thread != null)
            {
                ThreadState.X1 = (ulong)Thread.Priority;
                ThreadState.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }
    }
}