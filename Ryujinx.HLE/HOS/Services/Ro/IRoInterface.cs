using Ryujinx.Common;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    [Service("ldr:ro")]
    [Service("ro:1")] // 7.0.0+
    class IRoInterface : IpcService, IDisposable
    {
        private const int MaxNrr         = 0x40;
        private const int MaxNro         = 0x40;
        private const int MaxMapRetries  = 0x200;
        private const int GuardPagesSize = 0x4000;

        private const uint NrrMagic = 0x3052524E;
        private const uint NroMagic = 0x304F524E;

        private List<NrrInfo> _nrrInfos;
        private List<NroInfo> _nroInfos;

        private KProcess _owner;

        private static Random _random = new Random();

        public IRoInterface(ServiceCtx context)
        {
            _nrrInfos = new List<NrrInfo>(MaxNrr);
            _nroInfos = new List<NroInfo>(MaxNro);
            _owner    = null;
        }

        private ResultCode ParseNrr(out NrrInfo nrrInfo, ServiceCtx context, long nrrAddress, long nrrSize)
        {
            nrrInfo = null;

            if (nrrSize == 0 || nrrAddress + nrrSize <= nrrAddress || (nrrSize & 0xFFF) != 0)
            {
                return ResultCode.InvalidSize;
            }
            else if ((nrrAddress & 0xFFF) != 0)
            {
                return ResultCode.InvalidAddress;
            }

            StructReader reader = new StructReader(context.Memory, nrrAddress);
            NrrHeader    header = reader.Read<NrrHeader>();

            if (header.Magic != NrrMagic)
            {
                return ResultCode.InvalidNrr;
            }
            else if (header.NrrSize != nrrSize)
            {
                return ResultCode.InvalidSize;
            }

            List<byte[]> hashes = new List<byte[]>();

            for (int i = 0; i < header.HashCount; i++)
            {
                byte[] temp = new byte[0x20];

                context.Memory.Read((ulong)(nrrAddress + header.HashOffset + (i * 0x20)), temp);

                hashes.Add(temp);
            }

            nrrInfo = new NrrInfo(nrrAddress, header, hashes);

            return ResultCode.Success;
        }

        public bool IsNroHashPresent(byte[] nroHash)
        {
            foreach (NrrInfo info in _nrrInfos)
            {
                foreach (byte[] hash in info.Hashes)
                {
                    if (hash.SequenceEqual(nroHash))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsNroLoaded(byte[] nroHash)
        {
            foreach (NroInfo info in _nroInfos)
            {
                if (info.Hash.SequenceEqual(nroHash))
                {
                    return true;
                }
            }

            return false;
        }

        public ResultCode ParseNro(out NroInfo res, ServiceCtx context, ulong nroAddress, ulong nroSize, ulong bssAddress, ulong bssSize)
        {
            res = null;

            if (_nroInfos.Count >= MaxNro)
            {
                return ResultCode.TooManyNro;
            }
            else if (nroSize == 0 || nroAddress + nroSize <= nroAddress || (nroSize & 0xFFF) != 0)
            {
                return ResultCode.InvalidSize;
            }
            else if (bssSize != 0 && bssAddress + bssSize <= bssAddress)
            {
                return ResultCode.InvalidSize;
            }
            else if ((nroAddress & 0xFFF) != 0)
            {
                return ResultCode.InvalidAddress;
            }

            uint magic       = context.Memory.Read<uint>(nroAddress + 0x10);
            uint nroFileSize = context.Memory.Read<uint>(nroAddress + 0x18);

            if (magic != NroMagic || nroSize != nroFileSize)
            {
                return ResultCode.InvalidNro;
            }

            byte[] nroData = new byte[nroSize];

            context.Memory.Read(nroAddress, nroData);

            byte[] nroHash = null;

            MemoryStream stream = new MemoryStream(nroData);

            using (SHA256 hasher = SHA256.Create())
            {
                nroHash = hasher.ComputeHash(stream);
            }

            if (!IsNroHashPresent(nroHash))
            {
                return ResultCode.NotRegistered;
            }

            if (IsNroLoaded(nroHash))
            {
                return ResultCode.AlreadyLoaded;
            }

            stream.Position = 0;

            NroExecutable executable = new NroExecutable(stream, nroAddress, bssAddress);

            // check if everything is page align.
            if ((executable.Text.Length & 0xFFF) != 0 || (executable.Ro.Length & 0xFFF) != 0 ||
                (executable.Data.Length & 0xFFF) != 0 || (executable.BssSize & 0xFFF)   != 0)
            {
                return ResultCode.InvalidNro;
            }

            // check if everything is contiguous.
            if (executable.RoOffset   != executable.TextOffset + executable.Text.Length ||
                executable.DataOffset != executable.RoOffset   + executable.Ro.Length   ||
                nroFileSize           != executable.DataOffset + executable.Data.Length)
            {
                return ResultCode.InvalidNro;
            }

            // finally check the bss size match.
            if ((ulong)executable.BssSize != bssSize)
            {
                return ResultCode.InvalidNro;
            }

            int totalSize = executable.Text.Length + executable.Ro.Length + executable.Data.Length + executable.BssSize;

            res = new NroInfo(
                executable,
                nroHash,
                nroAddress,
                nroSize,
                bssAddress,
                bssSize,
                (ulong)totalSize);

            return ResultCode.Success;
        }

        private ResultCode MapNro(KProcess process, NroInfo info, out ulong nroMappedAddress)
        {
            KMemoryManager memMgr = process.MemoryManager;

            int retryCount = 0;

            nroMappedAddress = 0;

            while (retryCount++ < MaxMapRetries)
            {
                ResultCode result = MapCodeMemoryInProcess(process, info.NroAddress, info.NroSize, out nroMappedAddress);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                if (info.BssSize > 0)
                {
                    KernelResult bssMappingResult = memMgr.MapProcessCodeMemory(nroMappedAddress + info.NroSize, info.BssAddress, info.BssSize);

                    if (bssMappingResult == KernelResult.InvalidMemState)
                    {
                        memMgr.UnmapProcessCodeMemory(nroMappedAddress + info.NroSize, info.BssAddress, info.BssSize);
                        memMgr.UnmapProcessCodeMemory(nroMappedAddress, info.NroAddress, info.NroSize);

                        continue;
                    }
                    else if (bssMappingResult != KernelResult.Success)
                    {
                        memMgr.UnmapProcessCodeMemory(nroMappedAddress + info.NroSize, info.BssAddress, info.BssSize);
                        memMgr.UnmapProcessCodeMemory(nroMappedAddress, info.NroAddress, info.NroSize);

                        return (ResultCode)bssMappingResult;
                    }
                }

                if (CanAddGuardRegionsInProcess(process, nroMappedAddress, info.TotalSize))
                {
                    return ResultCode.Success;
                }
            }

            return ResultCode.InsufficientAddressSpace;
        }

        private bool CanAddGuardRegionsInProcess(KProcess process, ulong baseAddress, ulong size)
        {
            KMemoryManager memMgr = process.MemoryManager;

            KMemoryInfo memInfo = memMgr.QueryMemory(baseAddress - 1);

            if (memInfo.State == MemoryState.Unmapped && baseAddress - GuardPagesSize >= memInfo.Address)
            {
                memInfo = memMgr.QueryMemory(baseAddress + size);

                if (memInfo.State == MemoryState.Unmapped)
                {
                    return baseAddress + size + GuardPagesSize <= memInfo.Address + memInfo.Size;
                }
            }
            return false;
        }

        private ResultCode MapCodeMemoryInProcess(KProcess process, ulong baseAddress, ulong size, out ulong targetAddress)
        {
            KMemoryManager memMgr = process.MemoryManager;

            targetAddress = 0;

            int retryCount;

            ulong addressSpacePageLimit = (memMgr.GetAddrSpaceSize() - size) >> 12;

            for (retryCount = 0; retryCount < MaxMapRetries; retryCount++)
            {
                while (true)
                {
                    ulong randomOffset = (ulong)(uint)_random.Next(0, (int)addressSpacePageLimit) << 12;

                    targetAddress = memMgr.GetAddrSpaceBaseAddr() + randomOffset;

                    if (memMgr.InsideAddrSpace(targetAddress, size) && !memMgr.InsideHeapRegion(targetAddress, size) && !memMgr.InsideAliasRegion(targetAddress, size))
                    {
                        break;
                    }
                }

                KernelResult result = memMgr.MapProcessCodeMemory(targetAddress, baseAddress, size);

                if (result == KernelResult.InvalidMemState)
                {
                    continue;
                }
                else if (result != KernelResult.Success)
                {
                    return (ResultCode)result;
                }

                if (!CanAddGuardRegionsInProcess(process, targetAddress, size))
                {
                    continue;
                }

                return ResultCode.Success;
            }

            if (retryCount == MaxMapRetries)
            {
                return ResultCode.InsufficientAddressSpace;
            }

            return ResultCode.Success;
        }

        private KernelResult SetNroMemoryPermissions(KProcess process, IExecutable relocatableObject, ulong baseAddress)
        {
            ulong textStart = baseAddress + (ulong)relocatableObject.TextOffset;
            ulong roStart   = baseAddress + (ulong)relocatableObject.RoOffset;
            ulong dataStart = baseAddress + (ulong)relocatableObject.DataOffset;

            ulong bssStart = dataStart + (ulong)relocatableObject.Data.Length;

            ulong bssEnd = BitUtils.AlignUp(bssStart + (ulong)relocatableObject.BssSize, KMemoryManager.PageSize);

            process.CpuMemory.Write(textStart, relocatableObject.Text);
            process.CpuMemory.Write(roStart,   relocatableObject.Ro);
            process.CpuMemory.Write(dataStart, relocatableObject.Data);

            MemoryHelper.FillWithZeros(process.CpuMemory, (long)bssStart, (int)(bssEnd - bssStart));

            KernelResult result;

            result = process.MemoryManager.SetProcessMemoryPermission(textStart, roStart - textStart, MemoryPermission.ReadAndExecute);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = process.MemoryManager.SetProcessMemoryPermission(roStart, dataStart - roStart, MemoryPermission.Read);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return process.MemoryManager.SetProcessMemoryPermission(dataStart, bssEnd - dataStart, MemoryPermission.ReadAndWrite);
        }

        private ResultCode RemoveNrrInfo(long nrrAddress)
        {
            foreach (NrrInfo info in _nrrInfos)
            {
                if (info.NrrAddress == nrrAddress)
                {
                    _nrrInfos.Remove(info);

                    return ResultCode.Success;
                }
            }

            return ResultCode.NotLoaded;
        }

        private ResultCode RemoveNroInfo(ulong nroMappedAddress)
        {
            foreach (NroInfo info in _nroInfos)
            {
                if (info.NroMappedAddress == nroMappedAddress)
                {
                    _nroInfos.Remove(info);

                    return UnmapNroFromInfo(info);
                }
            }

            return ResultCode.NotLoaded;
        }

        private ResultCode UnmapNroFromInfo(NroInfo info)
        {
            ulong textSize = (ulong)info.Executable.Text.Length;
            ulong roSize   = (ulong)info.Executable.Ro.Length;
            ulong dataSize = (ulong)info.Executable.Data.Length;
            ulong bssSize  = (ulong)info.Executable.BssSize;

            KernelResult result = KernelResult.Success;

            if (info.Executable.BssSize != 0)
            {
                result = _owner.MemoryManager.UnmapProcessCodeMemory(
                    info.NroMappedAddress + textSize + roSize + dataSize,
                    info.Executable.BssAddress,
                    bssSize);
            }

            if (result == KernelResult.Success)
            {
                result = _owner.MemoryManager.UnmapProcessCodeMemory(
                    info.NroMappedAddress + textSize + roSize,
                    info.Executable.SourceAddress + textSize + roSize,
                    dataSize);

                if (result == KernelResult.Success)
                {
                    result = _owner.MemoryManager.UnmapProcessCodeMemory(
                        info.NroMappedAddress,
                        info.Executable.SourceAddress,
                        textSize + roSize);
                }
            }

            return (ResultCode)result;
        }

        private ResultCode IsInitialized(KProcess process)
        {
            if (_owner != null && _owner.Pid == process.Pid)
            {
                return ResultCode.Success;
            }

            return ResultCode.InvalidProcess;
        }

        [Command(0)]
        // LoadNro(u64, u64, u64, u64, u64, pid) -> u64
        public ResultCode LoadNro(ServiceCtx context)
        {
            ResultCode result = IsInitialized(context.Process);

            // Zero
            context.RequestData.ReadUInt64();

            ulong nroHeapAddress = context.RequestData.ReadUInt64();
            ulong nroSize        = context.RequestData.ReadUInt64();
            ulong bssHeapAddress = context.RequestData.ReadUInt64();
            ulong bssSize        = context.RequestData.ReadUInt64();

            ulong nroMappedAddress = 0;

            if (result == ResultCode.Success)
            {
                NroInfo info;

                result = ParseNro(out info, context, nroHeapAddress, nroSize, bssHeapAddress, bssSize);

                if (result == ResultCode.Success)
                {
                    result = MapNro(context.Process, info, out nroMappedAddress);

                    if (result == ResultCode.Success)
                    {
                        result = (ResultCode)SetNroMemoryPermissions(context.Process, info.Executable, nroMappedAddress);

                        if (result == ResultCode.Success)
                        {
                            info.NroMappedAddress = nroMappedAddress;

                            _nroInfos.Add(info);
                        }
                    }
                }
            }

            context.ResponseData.Write(nroMappedAddress);

            return result;
        }

        [Command(1)]
        // UnloadNro(u64, u64, pid)
        public ResultCode UnloadNro(ServiceCtx context)
        {
            ResultCode result = IsInitialized(context.Process);

            // Zero
            context.RequestData.ReadUInt64();

            ulong nroMappedAddress = context.RequestData.ReadUInt64();

            if (result == ResultCode.Success)
            {
                if ((nroMappedAddress & 0xFFF) != 0)
                {
                    return ResultCode.InvalidAddress;
                }

                result = RemoveNroInfo(nroMappedAddress);
            }

            return result;
        }

        [Command(2)]
        // LoadNrr(u64, u64, u64, pid)
        public ResultCode LoadNrr(ServiceCtx context)
        {
            ResultCode result = IsInitialized(context.Process);

            // pid placeholder, zero
            context.RequestData.ReadUInt64();

            long nrrAddress = context.RequestData.ReadInt64();
            long nrrSize    = context.RequestData.ReadInt64();

            if (result == ResultCode.Success)
            {
                NrrInfo info;
                result = ParseNrr(out info, context, nrrAddress, nrrSize);

                if (result == ResultCode.Success)
                {
                    if (_nrrInfos.Count >= MaxNrr)
                    {
                        result = ResultCode.NotLoaded;
                    }
                    else
                    {
                        _nrrInfos.Add(info);
                    }
                }
            }

            return result;
        }

        [Command(3)]
        // UnloadNrr(u64, u64, pid)
        public ResultCode UnloadNrr(ServiceCtx context)
        {
            ResultCode result = IsInitialized(context.Process);

            // pid placeholder, zero
            context.RequestData.ReadUInt64();

            long nrrHeapAddress = context.RequestData.ReadInt64();

            if (result == ResultCode.Success)
            {
                if ((nrrHeapAddress & 0xFFF) != 0)
                {
                    return ResultCode.InvalidAddress;
                }

                result = RemoveNrrInfo(nrrHeapAddress);
            }

            return result;
        }

        [Command(4)]
        // Initialize(u64, pid, KObject)
        public ResultCode Initialize(ServiceCtx context)
        {
            if (_owner != null)
            {
                return ResultCode.InvalidSession;
            }

            _owner = context.Process;

            return ResultCode.Success;
        }

        public void Dispose()
        {
            foreach (NroInfo info in _nroInfos)
            {
                UnmapNroFromInfo(info);
            }

            _nroInfos.Clear();
        }
    }
}