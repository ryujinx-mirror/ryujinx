using ARMeilleure.Memory;
using ARMeilleure.State;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContext<T> : IProcessContext where T : class, IVirtualMemoryManager, IMemoryManager
    {
        private readonly CpuContext _cpuContext;
        private T _memoryManager;

        public IVirtualMemoryManager AddressSpace => _memoryManager;

        public ArmProcessContext(T memoryManager)
        {
            if (memoryManager is IRefCounted rc)
            {
                rc.IncrementReferenceCount();
            }

            _memoryManager = memoryManager;
            _cpuContext = new CpuContext(memoryManager);
        }

        public void Execute(ExecutionContext context, ulong codeAddress)
        {
            _cpuContext.Execute(context, codeAddress);
        }

        public void Dispose()
        {
            if (_memoryManager is IRefCounted rc)
            {
                rc.DecrementReferenceCount();

                _memoryManager = null;
            }
        }
    }
}
