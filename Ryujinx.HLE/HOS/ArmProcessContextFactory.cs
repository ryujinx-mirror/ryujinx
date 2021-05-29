using Ryujinx.Common.Configuration;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContextFactory : IProcessContextFactory
    {
        public IProcessContext Create(KernelContext context, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

            switch (mode)
            {
                case MemoryManagerMode.SoftwarePageTable:
                    return new ArmProcessContext<MemoryManager>(new MemoryManager(addressSpaceSize, invalidAccessHandler), for64Bit);

                case MemoryManagerMode.HostMapped:
                case MemoryManagerMode.HostMappedUnsafe:
                    bool unsafeMode = mode == MemoryManagerMode.HostMappedUnsafe;
                    return new ArmProcessContext<MemoryManagerHostMapped>(new MemoryManagerHostMapped(addressSpaceSize, unsafeMode, invalidAccessHandler), for64Bit);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
