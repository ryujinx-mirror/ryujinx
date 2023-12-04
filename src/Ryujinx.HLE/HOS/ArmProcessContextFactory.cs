using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.Cpu.AppleHv;
using Ryujinx.Cpu.Jit;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS
{
    class ArmProcessContextFactory : IProcessContextFactory
    {
        private readonly ITickSource _tickSource;
        private readonly GpuContext _gpu;
        private readonly string _titleIdText;
        private readonly string _displayVersion;
        private readonly bool _diskCacheEnabled;
        private readonly ulong _codeAddress;
        private readonly ulong _codeSize;

        public IDiskCacheLoadState DiskCacheLoadState { get; private set; }

        public ArmProcessContextFactory(
            ITickSource tickSource,
            GpuContext gpu,
            string titleIdText,
            string displayVersion,
            bool diskCacheEnabled,
            ulong codeAddress,
            ulong codeSize)
        {
            _tickSource = tickSource;
            _gpu = gpu;
            _titleIdText = titleIdText;
            _displayVersion = displayVersion;
            _diskCacheEnabled = diskCacheEnabled;
            _codeAddress = codeAddress;
            _codeSize = codeSize;
        }

        public IProcessContext Create(KernelContext context, ulong pid, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            IArmProcessContext processContext;

            if (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && for64Bit && context.Device.Configuration.UseHypervisor)
            {
                var cpuEngine = new HvEngine(_tickSource);
                var memoryManager = new HvMemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                processContext = new ArmProcessContext<HvMemoryManager>(pid, cpuEngine, _gpu, memoryManager, addressSpaceSize, for64Bit);
            }
            else
            {
                MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

                if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
                {
                    Logger.Warning?.Print(LogClass.Cpu, "Host system doesn't support views, falling back to software page table");

                    mode = MemoryManagerMode.SoftwarePageTable;
                }

                var cpuEngine = new JitEngine(_tickSource);

                AddressSpace addressSpace = null;

                if (mode == MemoryManagerMode.HostMapped || mode == MemoryManagerMode.HostMappedUnsafe)
                {
                    if (!AddressSpace.TryCreate(context.Memory, addressSpaceSize, MemoryBlock.GetPageSize() == MemoryManagerHostMapped.PageSize, out addressSpace))
                    {
                        Logger.Warning?.Print(LogClass.Cpu, "Address space creation failed, falling back to software page table");

                        mode = MemoryManagerMode.SoftwarePageTable;
                    }
                }

                switch (mode)
                {
                    case MemoryManagerMode.SoftwarePageTable:
                        var memoryManager = new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                        processContext = new ArmProcessContext<MemoryManager>(pid, cpuEngine, _gpu, memoryManager, addressSpaceSize, for64Bit);
                        break;

                    case MemoryManagerMode.HostMapped:
                    case MemoryManagerMode.HostMappedUnsafe:
                        if (addressSpaceSize != addressSpace.AddressSpaceSize)
                        {
                            Logger.Warning?.Print(LogClass.Emulation, $"Allocated address space (0x{addressSpace.AddressSpaceSize:X}) is smaller than guest application requirements (0x{addressSpaceSize:X})");
                        }

                        var memoryManagerHostMapped = new MemoryManagerHostMapped(addressSpace, mode == MemoryManagerMode.HostMappedUnsafe, invalidAccessHandler);
                        processContext = new ArmProcessContext<MemoryManagerHostMapped>(pid, cpuEngine, _gpu, memoryManagerHostMapped, addressSpace.AddressSpaceSize, for64Bit);
                        break;

                    default:
                        throw new InvalidOperationException($"{nameof(mode)} contains an invalid value: {mode}");
                }
            }

            DiskCacheLoadState = processContext.Initialize(_titleIdText, _displayVersion, _diskCacheEnabled, _codeAddress, _codeSize);

            return processContext;
        }
    }
}
