using ChocolArm64.State;
using Ryujinx.OsHle.Handles;
using System.Threading;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private void SvcCreateThread(ARegisters Registers)
        {
            long EntryPoint  = (long)Registers.X1;
            long ArgsPtr     = (long)Registers.X2;
            long StackTop    = (long)Registers.X3;
            int  Priority    =  (int)Registers.X4;
            int  ProcessorId =  (int)Registers.X5;

            if (Ns.Os.TryGetProcess(Registers.ProcessId, out Process Process))
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

                Registers.X0 = (int)SvcResult.Success;
                Registers.X1 = (ulong)Handle;
            }

            //TODO: Error codes.
        }

        private void SvcStartThread(ARegisters Registers)
        {
            int Handle = (int)Registers.X0;

            HThread Thread = Ns.Os.Handles.GetData<HThread>(Handle);

            if (Thread != null)
            {
                Process.Scheduler.StartThread(Thread);

                Registers.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private void SvcSleepThread(ARegisters Registers)
        {           
            ulong NanoSecs = Registers.X0;

            if (Process.TryGetThread(Registers.Tpidr, out HThread CurrThread))
            {
                Process.Scheduler.Yield(CurrThread);
            }
            else
            {
                Logging.Error($"Thread with TPIDR_EL0 0x{Registers.Tpidr:x16} not found!");
            }

            Thread.Sleep((int)(NanoSecs / 1000000));
        }

        private void SvcGetThreadPriority(ARegisters Registers)
        {
            int Handle = (int)Registers.X1;

            HThread Thread = Ns.Os.Handles.GetData<HThread>(Handle);

            if (Thread != null)
            {
                Registers.X1 = (ulong)Thread.Priority;
                Registers.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }
    }
}