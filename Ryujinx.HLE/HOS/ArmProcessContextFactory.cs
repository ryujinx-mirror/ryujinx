using Ryujinx.Common.Configuration;
using Ryujinx.Cpu;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContextFactory : IProcessContextFactory
    {
        private readonly GpuContext _gpu;

        public ArmProcessContextFactory(GpuContext gpu)
        {
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
                    return new ArmProcessContext<MemoryManager>(pid, _gpu, new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler), for64Bit);

                case MemoryManagerMode.HostMapped:
                case MemoryManagerMode.HostMappedUnsafe:
                    bool unsafeMode = mode == MemoryManagerMode.HostMappedUnsafe;
                    return new ArmProcessContext<MemoryManagerHostMapped>(pid, _gpu, new MemoryManagerHostMapped(context.Memory, addressSpaceSize, unsafeMode, invalidAccessHandler), for64Bit);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
