using ChocolArm64.State;
using Ryujinx.OsHle.Handles;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private void SvcArbitrateLock(ARegisters Registers)
        {
            int  OwnerThreadHandle      =  (int)Registers.X0;
            long MutexAddress           = (long)Registers.X1;
            int  RequestingThreadHandle =  (int)Registers.X2;

            HThread RequestingThread = Ns.Os.Handles.GetData<HThread>(RequestingThreadHandle);

            Mutex M = new Mutex(Process, MutexAddress, OwnerThreadHandle);

            M = Ns.Os.Mutexes.GetOrAdd(MutexAddress, M);

            M.WaitForLock(RequestingThread, RequestingThreadHandle);

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcArbitrateUnlock(ARegisters Registers)
        {
            long MutexAddress = (long)Registers.X0;

            if (Ns.Os.Mutexes.TryGetValue(MutexAddress, out Mutex M))
            {
                M.Unlock();
            }

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcWaitProcessWideKeyAtomic(ARegisters Registers)
        {
            long MutexAddress   = (long)Registers.X0;
            long CondVarAddress = (long)Registers.X1;
            int  ThreadHandle   =  (int)Registers.X2;
            long Timeout        = (long)Registers.X3;

            HThread Thread = Ns.Os.Handles.GetData<HThread>(ThreadHandle);

            if (Ns.Os.Mutexes.TryGetValue(MutexAddress, out Mutex M))
            {
                M.GiveUpLock(ThreadHandle);
            }

            CondVar Cv = new CondVar(Process, CondVarAddress, Timeout);

            Cv = Ns.Os.CondVars.GetOrAdd(CondVarAddress, Cv);

            Cv.WaitForSignal(Thread);

            M = new Mutex(Process, MutexAddress, ThreadHandle);

            M = Ns.Os.Mutexes.GetOrAdd(MutexAddress, M);

            M.WaitForLock(Thread, ThreadHandle);

            Registers.X0 = (int)SvcResult.Success;
        }

        private void SvcSignalProcessWideKey(ARegisters Registers)
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