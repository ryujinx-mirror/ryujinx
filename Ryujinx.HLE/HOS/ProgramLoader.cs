using ARMeilleure.Translation.PTC;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;

namespace Ryujinx.HLE.HOS
{
    static class ProgramLoader
    {
        private const bool AslrEnabled = true;

        private const int ArgsHeaderSize = 8;
        private const int ArgsDataSize   = 0x9000;
        private const int ArgsTotalSize  = ArgsHeaderSize + ArgsDataSize;

        public static bool LoadKip(KernelContext context, KipExecutable kip)
        {
            uint endOffset = kip.DataOffset + (uint)kip.Data.Length;

            if (kip.BssSize != 0)
            {
                endOffset = kip.BssOffset + kip.BssSize;
            }

            uint codeSize = BitUtils.AlignUp(kip.TextOffset + endOffset, KMemoryManager.PageSize);

            int codePagesCount = (int)(codeSize / KMemoryManager.PageSize);

            ulong codeBaseAddress = kip.Is64BitAddressSpace ? 0x8000000UL : 0x200000UL;

            ulong codeAddress = codeBaseAddress + (ulong)kip.TextOffset;

            ProcessCreationFlags flags = 0;

            if (AslrEnabled)
            {
                // TODO: Randomization.

                flags |= ProcessCreationFlags.EnableAslr;
            }

            if (kip.Is64BitAddressSpace)
            {
                flags |= ProcessCreationFlags.AddressSpace64Bit;
            }

            if (kip.Is64Bit)
            {
                flags |= ProcessCreationFlags.Is64Bit;
            }

            ProcessCreationInfo creationInfo = new ProcessCreationInfo(
                kip.Name,
                kip.Version,
                kip.ProgramId,
                codeAddress,
                codePagesCount,
                flags,
                0,
                0);

            MemoryRegion memoryRegion = kip.UsesSecureMemory
                ? MemoryRegion.Service
                : MemoryRegion.Application;

            KMemoryRegionManager region = context.MemoryRegions[(int)memoryRegion];

            KernelResult result = region.AllocatePages((ulong)codePagesCount, false, out KPageList pageList);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            KProcess process = new KProcess(context);

            var processContextFactory = new ArmProcessContextFactory();

            result = process.InitializeKip(
                creationInfo,
                kip.Capabilities,
                pageList,
                context.ResourceLimit,
                memoryRegion,
                processContextFactory);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            result = LoadIntoMemory(process, kip, codeBaseAddress);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            process.DefaultCpuCore = kip.IdealCoreId;

            result = process.Start(kip.Priority, (ulong)kip.StackSize);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process start returned error \"{result}\".");

                return false;
            }

            context.Processes.TryAdd(process.Pid, process);

            return true;
        }

        public static bool LoadNsos(KernelContext context, Npdm metaData, byte[] arguments = null, params IExecutable[] executables)
        {
            ulong argsStart = 0;
            uint  argsSize  = 0;
            ulong codeStart = metaData.Is64Bit ? 0x8000000UL : 0x200000UL;
            uint  codeSize  = 0;

            ulong[] nsoBase = new ulong[executables.Length];

            for (int index = 0; index < executables.Length; index++)
            {
                IExecutable nso = executables[index];

                uint textEnd = nso.TextOffset + (uint)nso.Text.Length;
                uint roEnd   = nso.RoOffset   + (uint)nso.Ro.Length;
                uint dataEnd = nso.DataOffset + (uint)nso.Data.Length + nso.BssSize;

                uint nsoSize = textEnd;

                if (nsoSize < roEnd)
                {
                    nsoSize = roEnd;
                }

                if (nsoSize < dataEnd)
                {
                    nsoSize = dataEnd;
                }

                nsoSize = BitUtils.AlignUp(nsoSize, KMemoryManager.PageSize);

                nsoBase[index] = codeStart + (ulong)codeSize;

                codeSize += nsoSize;

                if (arguments != null && argsSize == 0)
                {
                    argsStart = (ulong)codeSize;

                    argsSize = (uint)BitUtils.AlignDown(arguments.Length * 2 + ArgsTotalSize - 1, KMemoryManager.PageSize);

                    codeSize += argsSize;
                }
            }

            PtcProfiler.StaticCodeStart = codeStart;
            PtcProfiler.StaticCodeSize  = (ulong)codeSize;

            int codePagesCount = (int)(codeSize / KMemoryManager.PageSize);

            int personalMmHeapPagesCount = metaData.PersonalMmHeapSize / KMemoryManager.PageSize;

            ProcessCreationInfo creationInfo = new ProcessCreationInfo(
                metaData.TitleName,
                metaData.Version,
                metaData.Aci0.TitleId,
                codeStart,
                codePagesCount,
                (ProcessCreationFlags)metaData.ProcessFlags | ProcessCreationFlags.IsApplication,
                0,
                personalMmHeapPagesCount);

            KernelResult result;

            KResourceLimit resourceLimit = new KResourceLimit(context);

            long applicationRgSize = (long)context.MemoryRegions[(int)MemoryRegion.Application].Size;

            result  = resourceLimit.SetLimitValue(LimitableResource.Memory,         applicationRgSize);
            result |= resourceLimit.SetLimitValue(LimitableResource.Thread,         608);
            result |= resourceLimit.SetLimitValue(LimitableResource.Event,          700);
            result |= resourceLimit.SetLimitValue(LimitableResource.TransferMemory, 128);
            result |= resourceLimit.SetLimitValue(LimitableResource.Session,        894);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed setting resource limit values.");

                return false;
            }

            KProcess process = new KProcess(context);

            MemoryRegion memoryRegion = (MemoryRegion)((metaData.Acid.Flags >> 2) & 0xf);

            if (memoryRegion > MemoryRegion.NvServices)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed due to invalid ACID flags.");

                return false;
            }

            var processContextFactory = new ArmProcessContextFactory();

            result = process.Initialize(
                creationInfo,
                metaData.Aci0.KernelAccessControl.Capabilities,
                resourceLimit,
                memoryRegion,
                processContextFactory);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            for (int index = 0; index < executables.Length; index++)
            {
                Logger.Info?.Print(LogClass.Loader, $"Loading image {index} at 0x{nsoBase[index]:x16}...");

                result = LoadIntoMemory(process, executables[index], nsoBase[index]);

                if (result != KernelResult.Success)
                {
                    Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                    return false;
                }
            }

            process.DefaultCpuCore = metaData.DefaultCpuId;

            result = process.Start(metaData.MainThreadPriority, (ulong)metaData.MainThreadStackSize);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process start returned error \"{result}\".");

                return false;
            }

            context.Processes.TryAdd(process.Pid, process);

            return true;
        }

        private static KernelResult LoadIntoMemory(KProcess process, IExecutable image, ulong baseAddress)
        {
            ulong textStart = baseAddress + (ulong)image.TextOffset;
            ulong roStart   = baseAddress + (ulong)image.RoOffset;
            ulong dataStart = baseAddress + (ulong)image.DataOffset;
            ulong bssStart  = baseAddress + (ulong)image.BssOffset;

            ulong end = dataStart + (ulong)image.Data.Length;

            if (image.BssSize != 0)
            {
                end = bssStart + (ulong)image.BssSize;
            }

            process.CpuMemory.Write(textStart, image.Text);
            process.CpuMemory.Write(roStart,   image.Ro);
            process.CpuMemory.Write(dataStart, image.Data);

            process.CpuMemory.Fill(bssStart, image.BssSize, 0);

            KernelResult SetProcessMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
            {
                if (size == 0)
                {
                    return KernelResult.Success;
                }

                size = BitUtils.AlignUp(size, KMemoryManager.PageSize);

                return process.MemoryManager.SetProcessMemoryPermission(address, size, permission);
            }

            KernelResult result = SetProcessMemoryPermission(textStart, (ulong)image.Text.Length, KMemoryPermission.ReadAndExecute);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = SetProcessMemoryPermission(roStart, (ulong)image.Ro.Length, KMemoryPermission.Read);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return SetProcessMemoryPermission(dataStart, end - dataStart, KMemoryPermission.ReadAndWrite);
        }
    }
}