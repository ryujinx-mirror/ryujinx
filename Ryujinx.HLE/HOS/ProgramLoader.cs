using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Util;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.Horizon.Common;
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
        public readonly string TitleIdText;
        public readonly string DisplayVersion;
        public readonly bool DiskCacheEnabled;
        public readonly bool AllowCodeMemoryForJit;

        public ProgramInfo(in Npdm npdm, string displayVersion, bool diskCacheEnabled, bool allowCodeMemoryForJit)
        {
            ulong programId = npdm.Aci.ProgramId.Value;

            Name = StringUtils.Utf8ZToString(npdm.Meta.ProgramName);
            ProgramId = programId;
            TitleIdText = programId.ToString("x16");
            DisplayVersion = displayVersion;
            DiskCacheEnabled = diskCacheEnabled;
            AllowCodeMemoryForJit = allowCodeMemoryForJit;
        }
    }

    struct ProgramLoadResult
    {
        public static ProgramLoadResult Failed => new ProgramLoadResult(false, null, null, 0);

        public readonly bool Success;
        public readonly ProcessTamperInfo TamperInfo;
        public readonly IDiskCacheLoadState DiskCacheLoadState;
        public readonly ulong ProcessId;

        public ProgramLoadResult(bool success, ProcessTamperInfo tamperInfo, IDiskCacheLoadState diskCacheLoadState, ulong pid)
        {
            Success = success;
            TamperInfo = tamperInfo;
            DiskCacheLoadState = diskCacheLoadState;
            ProcessId = pid;
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

            uint codeSize = BitUtils.AlignUp<uint>(kip.TextOffset + endOffset, KPageTableBase.PageSize);

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

            Result result = region.AllocatePages(out KPageList pageList, (ulong)codePagesCount);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            KProcess process = new KProcess(context);

            var processContextFactory = new ArmProcessContextFactory(
                context.Device.System.TickSource,
                context.Device.Gpu,
                string.Empty,
                string.Empty,
                false,
                codeAddress,
                codeSize);

            result = process.InitializeKip(
                creationInfo,
                kip.Capabilities,
                pageList,
                context.ResourceLimit,
                memoryRegion,
                processContextFactory);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            result = LoadIntoMemory(process, kip, codeBaseAddress);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return false;
            }

            process.DefaultCpuCore = kip.IdealCoreId;

            result = process.Start(kip.Priority, (ulong)kip.StackSize);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process start returned error \"{result}\".");

                return false;
            }

            context.Processes.TryAdd(process.Pid, process);

            return true;
        }

        public static ProgramLoadResult LoadNsos(
            KernelContext context,
            MetaLoader metaData,
            ProgramInfo programInfo,
            byte[] arguments = null,
            params IExecutable[] executables)
        {
            context.Device.System.ServiceTable.WaitServicesReady();

            LibHac.Result rc = metaData.GetNpdm(out var npdm);

            if (rc.IsFailure())
            {
                return ProgramLoadResult.Failed;
            }

            ref readonly var meta = ref npdm.Meta;

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

                nsoSize = BitUtils.AlignUp<uint>(nsoSize, KPageTableBase.PageSize);

                nsoBase[index] = codeStart + (ulong)codeSize;

                codeSize += nsoSize;

                if (arguments != null && argsSize == 0)
                {
                    argsStart = (ulong)codeSize;

                    argsSize = (uint)BitUtils.AlignDown(arguments.Length * 2 + ArgsTotalSize - 1, KPageTableBase.PageSize);

                    codeSize += argsSize;
                }
            }

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

            Result result;

            KResourceLimit resourceLimit = new KResourceLimit(context);

            long applicationRgSize = (long)context.MemoryManager.MemoryRegions[(int)MemoryRegion.Application].Size;

            result = resourceLimit.SetLimitValue(LimitableResource.Memory, applicationRgSize);

            if (result.IsSuccess)
            {
                result = resourceLimit.SetLimitValue(LimitableResource.Thread, 608);
            }

            if (result.IsSuccess)
            {
                result = resourceLimit.SetLimitValue(LimitableResource.Event, 700);
            }

            if (result.IsSuccess)
            {
                result = resourceLimit.SetLimitValue(LimitableResource.TransferMemory, 128);
            }

            if (result.IsSuccess)
            {
                result = resourceLimit.SetLimitValue(LimitableResource.Session, 894);
            }

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed setting resource limit values.");

                return ProgramLoadResult.Failed;
            }

            KProcess process = new KProcess(context, programInfo.AllowCodeMemoryForJit);

            MemoryRegion memoryRegion = (MemoryRegion)((npdm.Acid.Flags >> 2) & 0xf);

            if (memoryRegion > MemoryRegion.NvServices)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization failed due to invalid ACID flags.");

                return ProgramLoadResult.Failed;
            }

            var processContextFactory = new ArmProcessContextFactory(
                context.Device.System.TickSource,
                context.Device.Gpu,
                programInfo.TitleIdText,
                programInfo.DisplayVersion,
                programInfo.DiskCacheEnabled,
                codeStart,
                codeSize);

            result = process.Initialize(
                creationInfo,
                MemoryMarshal.Cast<byte, int>(npdm.KernelCapabilityData).ToArray(),
                resourceLimit,
                memoryRegion,
                processContextFactory);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                return ProgramLoadResult.Failed;
            }

            for (int index = 0; index < executables.Length; index++)
            {
                Logger.Info?.Print(LogClass.Loader, $"Loading image {index} at 0x{nsoBase[index]:x16}...");

                result = LoadIntoMemory(process, executables[index], nsoBase[index]);

                if (result != Result.Success)
                {
                    Logger.Error?.Print(LogClass.Loader, $"Process initialization returned error \"{result}\".");

                    return ProgramLoadResult.Failed;
                }
            }

            process.DefaultCpuCore = meta.DefaultCpuId;

            result = process.Start(meta.MainThreadPriority, meta.MainThreadStackSize);

            if (result != Result.Success)
            {
                Logger.Error?.Print(LogClass.Loader, $"Process start returned error \"{result}\".");

                return ProgramLoadResult.Failed;
            }

            context.Processes.TryAdd(process.Pid, process);

            // Keep the build ids because the tamper machine uses them to know which process to associate a
            // tamper to and also keep the starting address of each executable inside a process because some
            // memory modifications are relative to this address.
            ProcessTamperInfo tamperInfo = new ProcessTamperInfo(
                process,
                buildIds,
                nsoBase,
                process.MemoryManager.HeapRegionStart,
                process.MemoryManager.AliasRegionStart,
                process.MemoryManager.CodeRegionStart);

            return new ProgramLoadResult(true, tamperInfo, processContextFactory.DiskCacheLoadState, process.Pid);
        }

        private static Result LoadIntoMemory(KProcess process, IExecutable image, ulong baseAddress)
        {
            ulong textStart = baseAddress + image.TextOffset;
            ulong roStart   = baseAddress + image.RoOffset;
            ulong dataStart = baseAddress + image.DataOffset;
            ulong bssStart  = baseAddress + image.BssOffset;

            ulong end = dataStart + (ulong)image.Data.Length;

            if (image.BssSize != 0)
            {
                end = bssStart + image.BssSize;
            }

            process.CpuMemory.Write(textStart, image.Text);
            process.CpuMemory.Write(roStart,   image.Ro);
            process.CpuMemory.Write(dataStart, image.Data);

            process.CpuMemory.Fill(bssStart, image.BssSize, 0);

            Result SetProcessMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
            {
                if (size == 0)
                {
                    return Result.Success;
                }

                size = BitUtils.AlignUp<ulong>(size, KPageTableBase.PageSize);

                return process.MemoryManager.SetProcessMemoryPermission(address, size, permission);
            }

            Result result = SetProcessMemoryPermission(textStart, (ulong)image.Text.Length, KMemoryPermission.ReadAndExecute);

            if (result != Result.Success)
            {
                return result;
            }

            result = SetProcessMemoryPermission(roStart, (ulong)image.Ro.Length, KMemoryPermission.Read);

            if (result != Result.Success)
            {
                return result;
            }

            return SetProcessMemoryPermission(dataStart, end - dataStart, KMemoryPermission.ReadAndWrite);
        }
    }
}