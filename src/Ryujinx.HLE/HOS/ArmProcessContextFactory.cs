using Ryujinx.Common.Configuration;
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
                processContext = new ArmProcessContext<HvMemoryManager>(pid, cpuEngine, _gpu, memoryManager, for64Bit);
            }
            else
            {
                MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

                if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
                {
                    mode = MemoryManagerMode.SoftwarePageTable;
                }

                var cpuEngine = new JitEngine(_tickSource);

                switch (mode)
                {
                    case MemoryManagerMode.SoftwarePageTable:
                        var memoryManager = new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                        processContext = new ArmProcessContext<MemoryManager>(pid, cpuEngine, _gpu, memoryManager, for64Bit);
                        break;

                    case MemoryManagerMode.HostMapped:
                    case MemoryManagerMode.HostMappedUnsafe:
                        bool unsafeMode = mode == MemoryManagerMode.HostMappedUnsafe;
                        var memoryManagerHostMapped = new MemoryManagerHostMapped(context.Memory, addressSpaceSize, unsafeMode, invalidAccessHandler);
                        processContext = new ArmProcessContext<MemoryManagerHostMapped>(pid, cpuEngine, _gpu, memoryManagerHostMapped, for64Bit);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            DiskCacheLoadState = processContext.Initialize(_titleIdText, _displayVersion, _diskCacheEnabled, _codeAddress, _codeSize);

            return processContext;
        }
    }
}
