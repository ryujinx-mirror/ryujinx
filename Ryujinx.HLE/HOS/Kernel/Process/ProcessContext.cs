using ARMeilleure.State;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class ProcessContext : IProcessContext
    {
        public IVirtualMemoryManager AddressSpace { get; }

        public ProcessContext(IVirtualMemoryManager asManager)
        {
            AddressSpace = asManager;
        }

        public void Execute(ExecutionContext context, ulong codeAddress)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}
