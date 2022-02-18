using ARMeilleure.Memory;
using ARMeilleure.State;
using Ryujinx.Cpu;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContext<T> : IProcessContext where T : class, IVirtualMemoryManagerTracked, IMemoryManager
    {
        private readonly ulong _pid;
        private readonly GpuContext _gpuContext;
        private readonly CpuContext _cpuContext;
        private T _memoryManager;

        public IVirtualMemoryManager AddressSpace => _memoryManager;

        public ArmProcessContext(ulong pid, GpuContext gpuContext, T memoryManager, bool for64Bit)
        {
            if (memoryManager is IRefCounted rc)
            {
                rc.IncrementReferenceCount();
            }

            gpuContext.RegisterProcess(pid, memoryManager);

            _pid = pid;
            _gpuContext = gpuContext;
            _cpuContext = new CpuContext(memoryManager, for64Bit);
            _memoryManager = memoryManager;
        }

        public void Execute(ExecutionContext context, ulong codeAddress)
        {
            _cpuContext.Execute(context, codeAddress);
        }

        public void InvalidateCacheRegion(ulong address, ulong size)
        {
            _cpuContext.InvalidateCacheRegion(address, size);
        }

        public void Dispose()
        {
            if (_memoryManager is IRefCounted rc)
            {
                rc.DecrementReferenceCount();

                _memoryManager = null;
                _gpuContext.UnregisterProcess(_pid);
            }
        }
    }
}
