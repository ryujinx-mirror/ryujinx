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
        private readonly string _titleIdText;
        private readonly string _displayVersion;
        private readonly bool _diskCacheEnabled;
        private readonly ulong _codeAddress;
        private readonly ulong _codeSize;

        public IDiskCacheLoadState DiskCacheLoadState { get; private set; }

        public ArmProcessContextFactory(
            ICpuEngine cpuEngine,
            GpuContext gpu,
            string titleIdText,
            string displayVersion,
            bool diskCacheEnabled,
            ulong codeAddress,
            ulong codeSize)
        {
            _cpuEngine = cpuEngine;
            _gpu = gpu;
            _titleIdText = titleIdText;
            _displayVersion = displayVersion;
            _diskCacheEnabled = diskCacheEnabled;
            _codeAddress = codeAddress;
            _codeSize = codeSize;
        }

        public IProcessContext Create(KernelContext context, ulong pid, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

            if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
            {
                mode = MemoryManagerMode.SoftwarePageTable;
            }

            IArmProcessContext processContext;

            switch (mode)
            {
                case MemoryManagerMode.SoftwarePageTable:
                    var memoryManager = new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                    processContext = new ArmProcessContext<MemoryManager>(pid, _cpuEngine, _gpu, memoryManager, for64Bit);
                    break;

                case MemoryManagerMode.HostMapped:
                case MemoryManagerMode.HostMappedUnsafe:
                    bool unsafeMode = mode == MemoryManagerMode.HostMappedUnsafe;
                    var memoryManagerHostMapped = new MemoryManagerHostMapped(context.Memory, addressSpaceSize, unsafeMode, invalidAccessHandler);
                    processContext = new ArmProcessContext<MemoryManagerHostMapped>(pid, _cpuEngine, _gpu, memoryManagerHostMapped, for64Bit);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            DiskCacheLoadState = processContext.Initialize(_titleIdText, _displayVersion, _diskCacheEnabled, _codeAddress, _codeSize);

            return processContext;
        }
    }
}
