using ChocolArm64.State;
using Ryujinx.Core.OsHle.Handles;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Kernel
{
    partial class SvcHandler
    {
        private void SvcArbitrateLock(AThreadState ThreadState)
        {
            int  OwnerThreadHandle      =  (int)ThreadState.X0;
            long MutexAddress           = (long)ThreadState.X1;
            int  RequestingThreadHandle =  (int)ThreadState.X2;

            KThread OwnerThread = Process.HandleTable.GetData<KThread>(OwnerThreadHandle);

            if (OwnerThread == null)
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid owner thread handle 0x{OwnerThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            KThread RequestingThread = Process.HandleTable.GetData<KThread>(RequestingThreadHandle);

            if (RequestingThread == null)
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid requesting thread handle 0x{RequestingThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            MutualExclusion Mutex = GetMutex(MutexAddress);

            Mutex.WaitForLock(RequestingThread, OwnerThreadHandle);

            ThreadState.X0 = 0;
        }

        private void SvcArbitrateUnlock(AThreadState ThreadState)
        {
            long MutexAddress = (long)ThreadState.X0;

            GetMutex(MutexAddress).Unlock();

            Process.Scheduler.Yield(Process.GetThread(ThreadState.Tpidr));

            ThreadState.X0 = 0;
        }

        private void SvcWaitProcessWideKeyAtomic(AThreadState ThreadState)
        {
            long  MutexAddress   = (long)ThreadState.X0;
            long  CondVarAddress = (long)ThreadState.X1;
            int   ThreadHandle   =  (int)ThreadState.X2;
            ulong Timeout        =       ThreadState.X3;

            KThread Thread = Process.HandleTable.GetData<KThread>(ThreadHandle);

            if (Thread == null)
            {
                Logging.Warn(LogClass.KernelSvc, $"Invalid thread handle 0x{ThreadHandle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);
            }

            Process.Scheduler.Suspend(Thread.ProcessorId);

            MutualExclusion Mutex = GetMutex(MutexAddress);

            Mutex.Unlock();

            if (!GetCondVar(CondVarAddress).WaitForSignal(Thread, Timeout))
            {
                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.Timeout);

                return;
            }

            Mutex.WaitForLock(Thread);

            Process.Scheduler.Resume(Thread);

            ThreadState.X0 = 0;
        }

        private void SvcSignalProcessWideKey(AThreadState ThreadState)
        {
            long CondVarAddress = (long)ThreadState.X0;
            int  Count          =  (int)ThreadState.X1;

            KThread CurrThread = Process.GetThread(ThreadState.Tpidr);

            GetCondVar(CondVarAddress).SetSignal(CurrThread, Count);

            ThreadState.X0 = 0;
        }

        private MutualExclusion GetMutex(long MutexAddress)
        {
            MutualExclusion MutexFactory(long Key)
            {
                return new MutualExclusion(Process, MutexAddress);
            }

            return Mutexes.GetOrAdd(MutexAddress, MutexFactory);
        }

        private ConditionVariable GetCondVar(long CondVarAddress)
        {
            ConditionVariable CondVarFactory(long Key)
            {
                return new ConditionVariable(Process, CondVarAddress);
            }

            return CondVars.GetOrAdd(CondVarAddress, CondVarFactory);
        }
    }
}