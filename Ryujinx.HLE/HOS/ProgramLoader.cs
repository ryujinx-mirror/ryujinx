using ARMeilleure.Translation.PTC;
using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Util;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Npdm = LibHac.Loader.Npdm;

namespace Ryujinx.HLE.HOS
{
    struct ProgramInfo
    {
        public string Name;
        public ulong ProgramId;
        public bool AllowCodeMemoryForJit;

        public ProgramInfo(in Npdm npdm, bool allowCodeMemoryForJit)
        {
            Name = StringUtils.Utf8ZToString(npdm.Meta.Value.ProgramName);
            ProgramId = npdm.Aci.Value.ProgramId.Value;
            AllowCodeMemoryForJit = allowCodeMemoryForJit;
        }
    }

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

            uint codeSize = BitUtils.AlignUp(kip.TextOffset + endOffset, KPageTableBase.PageSize);

            int codePagesCount = (int)(codeSize / KPageTableBase.PageSize);

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

            KMemoryRegionManager region = context.MemoryManager.MemoryRegions[(int)memoryRegion];

            KernelResult result = region.AllocatePages((ulong)codePagesCount, false, out KPageList pageList);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            KProcess process = new KProcess(context);

            var processContextFactory = new ArmProcessContextFactory(context.Device.Gpu);

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

        public static bool LoadNsos(
            KernelContext context,
            out ProcessTamperInfo tamperInfo,
            MetaLoader metaData,
            ProgramInfo programInfo,
            byte[] arguments = null,
            params IExecutable[] executables)
        {
            LibHac.Result rc = metaData.GetNpdm(out var npdm);

            if (rc.IsFailure())
            {
                tamperInfo = null;
                return false;
            }

            ref readonly var meta = ref npdm.Meta.Value;

            ulong argsStart = 0;
            uint  argsSize  = 0;
            ulong codeStart = (meta.Flags & 1) != 0 ? 0x8000000UL : 0x200000UL;
            uint  codeSize  = 0;

            var buildIds = executables.Select(e => (e switch
            {
                NsoExecutable nso => BitConverter.ToString(nso.BuildId.ItemsRo.ToArray()),
                NroExecutable nro => BitConverter.ToString(nro.Header.BuildId),
                _ => ""
            }).Replace("-", "").ToUpper());

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

                nsoSize = BitUtils.AlignUp(nsoSize, KPageTableBase.PageSize);

                nsoBase[index] = codeStart + (ulong)codeSize;

                codeSize += nsoSize;

                if (arguments != null && argsSize == 0)
                {
                    argsStart = (ulong)codeSize;

                    argsSize = (uint)BitUtils.AlignDown(arguments.Length * 2 + ArgsTotalSize - 1, KPageTableBase.PageSize);

                    codeSize += argsSize;
                }
            }

            PtcProfiler.StaticCodeStart = codeStart;
            PtcProfiler.StaticCodeSize  = (ulong)codeSize;

            int codePagesCount = (int)(codeSize / KPageTableBase.PageSize);

            int personalMmHeapPagesCount = (int)(meta.SystemResourceSize / KPageTableBase.PageSize);

            ProcessCreationInfo creationInfo = new ProcessCreationInfo(
                programInfo.Name,
                (int)meta.Version,
                programInfo.ProgramId,
                codeStart,
                codePagesCount,
                (ProcessCreationFlags)meta.Flags | ProcessCreationFlags.IsApplication,
                0,
                personalMmHeapPagesCount);

            context.Device.System.LibHacHorizonManager.InitializeApplicationClient(new ProgramId(programInfo.ProgramId), in npdm);

            KernelResult result;

            KResourceLimit resourceLimit = new KResourceLimit(context);

            long applicationRgSize = (long)context.MemoryManager.MemoryRegions[(int)MemoryRegion.Application].Size;

            result  = resourceLimit.SetLimitValue(LimitableResource.Memory,         applicationRgSize);
            result |= resourceLimit.SetLimitValue(LimitableResource.Thread,         608);
            result |= resourceLimit.SetLimitValue(LimitableResource.Event,          700);
            result |= resourceLimit.SetLimitValue(LimitableResource.TransferMemory, 128);
            result |= resourceLimit.SetLimitValue(LimitableResource.Session,        894);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed setting resource limit values.");

                tamperInfo = null;

                return false;
            }

            KProcess process = new KProcess(context, programInfo.AllowCodeMemoryForJit);

            MemoryRegion memoryRegion = (MemoryRegion)((npdm.Acid.Value.Flags >> 2) & 0xf);

            if (memoryRegion > MemoryRegion.NvServices)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed due to invalid ACID flags.");

                tamperInfo = null;

                return false;
            }

            var processContextFactory = new ArmProcessContextFactory(context.Device.Gpu);

            result = process.Initialize(
                creationInfo,
                MemoryMarshal.Cast<byte, int>(npdm.KernelCapabilityData).ToArray(),
                resourceLimit,
                memoryRegion,
                processContextFactory);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                tamperInfo = null;

                return false;
            }

            for (int index = 0; index < executables.Length; index++)
            {
                Logger.Info?.Print(LogClass.Loader, $"Loading image {index} at 0x{nsoBase[index]:x16}...");

                result = LoadIntoMemory(process, executables[index], nsoBase[index]);

                if (result != KernelResult.Success)
                {
                    Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                    tamperInfo = null;

                    return false;
                }
            }

            process.DefaultCpuCore = meta.DefaultCpuId;

            result = process.Start(meta.MainThreadPriority, meta.MainThreadStackSize);

            if (result != KernelResult.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process start returned error \"{result}\".");

                tamperInfo = null;

                return false;
            }

            context.Processes.TryAdd(process.Pid, process);

            // Keep the build ids because the tamper machine uses them to know which process to associate a
            // tamper to and also keep the starting address of each executable inside a process because some
            // memory modifications are relative to this address.
            tamperInfo = new ProcessTamperInfo(process, buildIds, nsoBase, process.MemoryManager.HeapRegionStart,
                process.MemoryManager.AliasRegionStart, process.MemoryManager.CodeRegionStart);

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

                size = BitUtils.AlignUp(size, KPageTableBase.PageSize);

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