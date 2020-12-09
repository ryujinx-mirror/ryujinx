using ARMeilleure.State;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SyscallHandler
    {
        private readonly KernelContext _context;
        private readonly Syscall32 _syscall32;
        private readonly Syscall64 _syscall64;

        public SyscallHandler(KernelContext context)
        {
            _context = context;
            _syscall32 = new Syscall32(context.Syscall);
            _syscall64 = new Syscall64(context.Syscall);
        }

        public void SvcCall(object sender, InstExceptionEventArgs e)
        {
            ExecutionContext context = (ExecutionContext)sender;

            if (context.IsAarch32)
            {
                var svcFunc = SyscallTable.SvcTable32[e.Id];

                if (svcFunc == null)
                {
                    throw new NotImplementedException($"SVC 0x{e.Id:X4} is not implemented.");
                }

                svcFunc(_syscall32, context);
            }
            else
            {
                var svcFunc = SyscallTable.SvcTable64[e.Id];

                if (svcFunc == null)
                {
                    throw new NotImplementedException($"SVC 0x{e.Id:X4} is not implemented.");
                }

                svcFunc(_syscall64, context);
            }

            PostSvcHandler();
        }

        private void PostSvcHandler()
        {
            KThread currentThread = KernelStatic.GetCurrentThread();

            currentThread.HandlePostSyscall();
        }
    }
}