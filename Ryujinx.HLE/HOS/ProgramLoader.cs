using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;

namespace Ryujinx.HLE.HOS
{
    class ProgramLoader
    {
        private const bool AslrEnabled = true;

        private const int ArgsHeaderSize = 8;
        private const int ArgsDataSize   = 0x9000;
        private const int ArgsTotalSize  = ArgsHeaderSize + ArgsDataSize;

        public static bool LoadKernelInitalProcess(Horizon system, KernelInitialProcess kip)
        {
            int endOffset = kip.DataOffset + kip.Data.Length;

            if (kip.BssSize != 0)
            {
                endOffset = kip.BssOffset + kip.BssSize;
            }

            int codeSize = BitUtils.AlignUp(kip.TextOffset + endOffset, KMemoryManager.PageSize);

            int codePagesCount = codeSize / KMemoryManager.PageSize;

            ulong codeBaseAddress = kip.Addr39Bits ? 0x8000000UL : 0x200000UL;

            ulong codeAddress = codeBaseAddress + (ulong)kip.TextOffset;

            int mmuFlags = 0;

            if (AslrEnabled)
            {
                // TODO: Randomization.

                mmuFlags |= 0x20;
            }

            if (kip.Addr39Bits)
            {
                mmuFlags |= (int)AddressSpaceType.Addr39Bits << 1;
            }

            if (kip.Is64Bits)
            {
                mmuFlags |= 1;
            }

            ProcessCreationInfo creationInfo = new ProcessCreationInfo(
                kip.Name,
                kip.ProcessCategory,
                kip.TitleId,
                codeAddress,
                codePagesCount,
                mmuFlags,
                0,
                0);

            MemoryRegion memoryRegion = kip.IsService
                ? MemoryRegion.Service
                : MemoryRegion.Application;

            KMemoryRegionManager region = system.MemoryRegions[(int)memoryRegion];

            KernelResult result = region.AllocatePages((ulong)codePagesCount, false, out KPageList pageList);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            KProcess process = new KProcess(system);

            result = process.InitializeKip(
                creationInfo,
                kip.Capabilities,
                pageList,
                system.ResourceLimit,
                memoryRegion);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            result = LoadIntoMemory(process, kip, codeBaseAddress);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            process.DefaultCpuCore = kip.DefaultProcessorId;

            result = process.Start(kip.MainThreadPriority, (ulong)kip.MainThreadStackSize);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process start returned error \"{result}\".");

                return false;
            }

            system.Processes.Add(process.Pid, process);

            return true;
        }

        public static bool LoadStaticObjects(
            Horizon       system,
            Npdm          metaData,
            IExecutable[] staticObjects,
            byte[]        arguments = null)
        {
            if (!metaData.Is64Bits)
            {
                Logger.PrintWarning(LogClass.Loader, "32-bits application detected!");
            }

            ulong argsStart = 0;
            int   argsSize  = 0;
            ulong codeStart = metaData.Is64Bits ? 0x8000000UL : 0x200000UL;
            int   codeSize  = 0;

            ulong[] nsoBase = new ulong[staticObjects.Length];

            for (int index = 0; index < staticObjects.Length; index++)
            {
                IExecutable staticObject = staticObjects[index];

                int textEnd = staticObject.TextOffset + staticObject.Text.Length;
                int roEnd   = staticObject.RoOffset   + staticObject.Ro.Length;
                int dataEnd = staticObject.DataOffset + staticObject.Data.Length + staticObject.BssSize;

                int nsoSize = textEnd;

                if ((uint)nsoSize < (uint)roEnd)
                {
                    nsoSize = roEnd;
                }

                if ((uint)nsoSize < (uint)dataEnd)
                {
                    nsoSize = dataEnd;
                }

                nsoSize = BitUtils.AlignUp(nsoSize, KMemoryManager.PageSize);

                nsoBase[index] = codeStart + (ulong)codeSize;

                codeSize += nsoSize;

                if (arguments != null && argsSize == 0)
                {
                    argsStart = (ulong)codeSize;

                    argsSize = BitUtils.AlignDown(arguments.Length * 2 + ArgsTotalSize - 1, KMemoryManager.PageSize);

                    codeSize += argsSize;
                }
            }

            int codePagesCount = codeSize / KMemoryManager.PageSize;

            int personalMmHeapPagesCount = metaData.PersonalMmHeapSize / KMemoryManager.PageSize;

            ProcessCreationInfo creationInfo = new ProcessCreationInfo(
                metaData.TitleName,
                metaData.ProcessCategory,
                metaData.Aci0.TitleId,
                codeStart,
                codePagesCount,
                metaData.MmuFlags,
                0,
                personalMmHeapPagesCount);

            KernelResult result;

            KResourceLimit resourceLimit = new KResourceLimit(system);

            long applicationRgSize = (long)system.MemoryRegions[(int)MemoryRegion.Application].Size;

            result  = resourceLimit.SetLimitValue(LimitableResource.Memory,         applicationRgSize);
            result |= resourceLimit.SetLimitValue(LimitableResource.Thread,         608);
            result |= resourceLimit.SetLimitValue(LimitableResource.Event,          700);
            result |= resourceLimit.SetLimitValue(LimitableResource.TransferMemory, 128);
            result |= resourceLimit.SetLimitValue(LimitableResource.Session,        894);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization failed setting resource limit values.");

                return false;
            }

            KProcess process = new KProcess(system);

            MemoryRegion memoryRegion = (MemoryRegion)((metaData.Acid.Flags >> 2) & 0xf);

            if (memoryRegion > MemoryRegion.NvServices)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization failed due to invalid ACID flags.");

                return false;
            }

            result = process.Initialize(
                creationInfo,
                metaData.Aci0.KernelAccessControl.Capabilities,
                resourceLimit,
                memoryRegion);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            for (int index = 0; index < staticObjects.Length; index++)
            {
                Logger.PrintInfo(LogClass.Loader, $"Loading image {index} at 0x{nsoBase[index]:x16}...");

                result = LoadIntoMemory(process, staticObjects[index], nsoBase[index]);

                if (result != KernelResult.Success)
                {
                    Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                    return false;
                }
            }

            process.DefaultCpuCore = metaData.DefaultCpuId;

            result = process.Start(metaData.MainThreadPriority, (ulong)metaData.MainThreadStackSize);

            if (result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process start returned error \"{result}\".");

                return false;
            }

            system.Processes.Add(process.Pid, process);

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

            process.CpuMemory.WriteBytes((long)textStart, image.Text);
            process.CpuMemory.WriteBytes((long)roStart,   image.Ro);
            process.CpuMemory.WriteBytes((long)dataStart, image.Data);

            MemoryHelper.FillWithZeros(process.CpuMemory, (long)bssStart, image.BssSize);

            KernelResult SetProcessMemoryPermission(ulong address, ulong size, MemoryPermission permission)
            {
                if (size == 0)
                {
                    return KernelResult.Success;
                }

                size = BitUtils.AlignUp(size, KMemoryManager.PageSize);

                return process.MemoryManager.SetProcessMemoryPermission(address, size, permission);
            }

            KernelResult result = SetProcessMemoryPermission(textStart, (ulong)image.Text.Length, MemoryPermission.ReadAndExecute);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = SetProcessMemoryPermission(roStart, (ulong)image.Ro.Length, MemoryPermission.Read);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return SetProcessMemoryPermission(dataStart, end - dataStart, MemoryPermission.ReadAndWrite);
        }
    }
}