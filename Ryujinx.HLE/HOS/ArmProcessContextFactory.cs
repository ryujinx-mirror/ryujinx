using Ryujinx.Common.Configuration;
using Ryujinx.Cpu;
using Ryujinx.Cpu.Jit;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContextFactory : IProcessContextFactory
    {
        private readonly ICpuEngine _cpuEngine;
        private readonly GpuContext _gpu;

        public ArmProcessContextFactory(ICpuEngine cpuEngine, GpuContext gpu)
        {
            _cpuEngine = cpuEngine;
            _gpu = gpu;
        }

        public IProcessContext Create(KernelContext context, ulong pid, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

            if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
            {
                mode = MemoryManagerMode.SoftwarePageTable;
            }

            switch (mode)
            {
                case MemoryManagerMode.SoftwarePageTable:
                    var memoryManager = new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                    return new ArmProcessContext<MemoryManager>(pid, _cpuEngine, _gpu, memoryManager, for64Bit);

                case MemoryManagerMode.HostMapped:
                case MemoryManagerMode.HostMappedUnsafe:
                    bool unsafeMode = mode == MemoryManagerMode.HostMappedUnsafe;
                    var memoryManagerHostMapped = new MemoryManagerHostMapped(context.Memory, addressSpaceSize, unsafeMode, invalidAccessHandler);
                    return new ArmProcessContext<MemoryManagerHostMapped>(pid, _cpuEngine, _gpu, memoryManagerHostMapped, for64Bit);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
