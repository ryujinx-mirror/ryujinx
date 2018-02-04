using ChocolArm64;
using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.OsHle.Handles;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private static void SvcArbitrateLock(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int  OwnerThreadHandle      =  (int)Registers.X0;
            long MutexAddress           = (long)Registers.X1;
            int  RequestingThreadHandle =  (int)Registers.X2;

            AThread RequestingThread = Ns.Os.Handles.GetData<HThread>(RequestingThreadHandle).Thread;

            Mutex M = new Mutex(Memory, MutexAddress);

            M = Ns.Os.Mutexes.GetOrAdd(MutexAddress, M);

            //FIXME
            //M.WaitForLock(RequestingThread, RequestingThreadHandle);

            Memory.WriteInt32(MutexAddress, 0);

            Registers.X0 = (int)SvcResult.Success;
        }

        private static void SvcArbitrateUnlock(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long MutexAddress = (long)Registers.X0;

            if (Ns.Os.Mutexes.TryGetValue(MutexAddress, out Mutex M))
            {
                M.Unlock();
            }

            Registers.X0 = (int)SvcResult.Success;
        }

        private static void SvcWaitProcessWideKeyAtomic(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long MutexAddress   = (long)Registers.X0;
            long CondVarAddress = (long)Registers.X1;
            int  ThreadHandle   =  (int)Registers.X2;
            long Timeout        = (long)Registers.X3;

            AThread Thread = Ns.Os.Handles.GetData<HThread>(ThreadHandle).Thread;

            if (Ns.Os.Mutexes.TryGetValue(MutexAddress, out Mutex M))
            {
                M.GiveUpLock(ThreadHandle);
            }

            CondVar Signal = new CondVar(Memory, CondVarAddress, Timeout);

            Signal = Ns.Os.CondVars.GetOrAdd(CondVarAddress, Signal);

            Signal.WaitForSignal(ThreadHandle);

            M = new Mutex(Memory, MutexAddress);

            M = Ns.Os.Mutexes.GetOrAdd(MutexAddress, M);

            //FIXME
            //M.WaitForLock(Thread, ThreadHandle);

            Memory.WriteInt32(MutexAddress, 0);

            Registers.X0 = (int)SvcResult.Success;
        }

        private static void SvcSignalProcessWideKey(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long CondVarAddress = (long)Registers.X0;
            int  Count          =  (int)Registers.X1;

            if (Ns.Os.CondVars.TryGetValue(CondVarAddress, out CondVar Cv))
            {
                Cv.SetSignal(Count);
            }

            Registers.X0 = (int)SvcResult.Success;
        }
    }
}