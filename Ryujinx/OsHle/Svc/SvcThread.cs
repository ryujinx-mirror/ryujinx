using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.OsHle.Handles;
using System.Threading;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private static void SvcCreateThread(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long EntryPoint  = (long)Registers.X1;
            long ArgsPtr     = (long)Registers.X2;
            long StackTop    = (long)Registers.X3;
            int  Priority    =  (int)Registers.X4;
            int  ProcessorId =  (int)Registers.X5;

            if (Ns.Os.TryGetProcess(Registers.ProcessId, out Process Process))
            {
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

        private static void SvcStartThread(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int Handle = (int)Registers.X0;

            HThread HndData = Ns.Os.Handles.GetData<HThread>(Handle);

            if (HndData != null)
            {
                HndData.Thread.Execute();

                Registers.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private static void SvcSleepThread(Switch Ns, ARegisters Registers, AMemory Memory)
        {           
            ulong NanoSecs = Registers.X0;

            if (NanoSecs == 0)
            {
                Thread.Yield();
            }
            else
            {
                Thread.Sleep((int)(NanoSecs / 1000000));
            }
        }

        private static void SvcGetThreadPriority(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int Handle = (int)Registers.X1;

            HThread HndData = Ns.Os.Handles.GetData<HThread>(Handle);

            if (HndData != null)
            {
                Registers.X1 = (ulong)HndData.Thread.Priority;
                Registers.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }
    }
}