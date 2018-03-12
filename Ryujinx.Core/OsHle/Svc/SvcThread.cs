using ChocolArm64.State;
using Ryujinx.Core.OsHle.Handles;

namespace Ryujinx.Core.OsHle.Svc
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

            //TODO: Error codes.
        }

        private void SvcStartThread(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            HThread Thread = Process.HandleTable.GetData<HThread>(Handle);

            if (Thread != null)
            {
                Process.Scheduler.StartThread(Thread);

                ThreadState.X0 = 0;
            }

            //TODO: Error codes.
        }

        private void SvcExitThread(AThreadState ThreadState)
        {
            HThread CurrThread = Process.GetThread(ThreadState.Tpidr);
            
            CurrThread.Thread.StopExecution();
        }

        private void SvcSleepThread(AThreadState ThreadState)
        {           
            ulong NanoSecs = ThreadState.X0;

            HThread CurrThread = Process.GetThread(ThreadState.Tpidr);
            
            if (NanoSecs == 0)
            {
                Process.Scheduler.Yield(CurrThread);
            }
            else
            {
                Process.Scheduler.WaitForSignal(CurrThread, (int)(NanoSecs / 1000000));
            }
        }

        private void SvcGetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;

            HThread Thread = Process.HandleTable.GetData<HThread>(Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.Priority;
            }

            //TODO: Error codes.
        }

        private void SvcSetThreadPriority(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X1;
            int Prio = (int)ThreadState.X0;

            HThread Thread = Process.HandleTable.GetData<HThread>(Handle);

            if (Thread != null)
            {
                Thread.Priority = Prio;

                ThreadState.X0 = 0;
            }

            //TODO: Error codes.
        }

        private void SvcSetThreadCoreMask(AThreadState ThreadState)
        {
            ThreadState.X0 = 0;

            //TODO: Error codes.
        }

        private void SvcGetThreadId(AThreadState ThreadState)
        {
            int Handle = (int)ThreadState.X0;

            HThread Thread = Process.HandleTable.GetData<HThread>(Handle);

            if (Thread != null)
            {
                ThreadState.X0 = 0;
                ThreadState.X1 = (ulong)Thread.ThreadId;
            }

            //TODO: Error codes.
        }
    }
}