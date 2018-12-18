using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Ldr
{
    [StructLayout(LayoutKind.Explicit, Size = 0x350)]
    unsafe struct NrrHeader
    {
        [FieldOffset(0)]
        public uint  Magic;

        [FieldOffset(0x10)]
        public ulong TitleIdMask;

        [FieldOffset(0x18)]
        public ulong TitleIdPattern;

        [FieldOffset(0x30)]
        public fixed byte Modulus[0x100];

        [FieldOffset(0x130)]
        public fixed byte FixedKeySignature[0x100];

        [FieldOffset(0x230)]
        public fixed byte NrrSignature[0x100];

        [FieldOffset(0x330)]
        public ulong TitleIdMin;

        [FieldOffset(0x338)]
        public uint  NrrSize;

        [FieldOffset(0x340)]
        public uint HashOffset;

        [FieldOffset(0x344)]
        public uint HashCount;
    }

    class NrrInfo
    {
        public NrrHeader    Header     { get; private set; }
        public List<byte[]> Hashes     { get; private set; }
        public long         NrrAddress { get; private set; }

        public NrrInfo(long nrrAddress, NrrHeader header, List<byte[]> hashes)
        {
            NrrAddress = nrrAddress;
            Header     = header;
            Hashes     = hashes;
        }
    }

    class NroInfo
    {
        public NxRelocatableObject Executable { get; private set; }

        public byte[] Hash             { get; private set; }
        public ulong  NroAddress       { get; private set; }
        public ulong  NroSize          { get; private set; }
        public ulong  BssAddress       { get; private set; }
        public ulong  BssSize          { get; private set; }
        public ulong  TotalSize        { get; private set; }
        public ulong  NroMappedAddress { get; set; }

        public NroInfo(
            NxRelocatableObject executable,
            byte[]              hash,
            ulong               nroAddress,
            ulong               nroSize,
            ulong               bssAddress,
            ulong               bssSize,
            ulong               totalSize)
        {
            Executable = executable;
            Hash       = hash;
            NroAddress = nroAddress;
            NroSize    = nroSize;
            BssAddress = bssAddress;
            BssSize    = bssSize;
            TotalSize  = totalSize;
        }
    }

    class IRoInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private const int MaxNrr = 0x40;
        private const int MaxNro = 0x40;

        private const uint NrrMagic = 0x3052524E;
        private const uint NroMagic = 0x304F524E;

        private List<NrrInfo> _nrrInfos;
        private List<NroInfo> _nroInfos;

        private bool _isInitialized;

        public IRoInterface()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, LoadNro    },
                { 1, UnloadNro  },
                { 2, LoadNrr    },
                { 3, UnloadNrr  },
                { 4, Initialize }
            };

            _nrrInfos = new List<NrrInfo>(MaxNrr);
            _nroInfos = new List<NroInfo>(MaxNro);
        }

        private long ParseNrr(out NrrInfo nrrInfo, ServiceCtx context, long nrrAddress, long nrrSize)
        {
            nrrInfo = null;

            if (nrrSize == 0 || nrrAddress + nrrSize <= nrrAddress || (nrrSize & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if ((nrrAddress & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
            }

            StructReader reader = new StructReader(context.Memory, nrrAddress);
            NrrHeader    header = reader.Read<NrrHeader>();

            if (header.Magic != NrrMagic)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNrr);
            }
            else if (header.NrrSize != nrrSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }

            List<byte[]> hashes = new List<byte[]>();

            for (int i = 0; i < header.HashCount; i++)
            {
                hashes.Add(context.Memory.ReadBytes(nrrAddress + header.HashOffset + (i * 0x20), 0x20));
            }

            nrrInfo = new NrrInfo(nrrAddress, header, hashes);

            return 0;
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

        public long ParseNro(out NroInfo res, ServiceCtx context, ulong nroAddress, ulong nroSize, ulong bssAddress, ulong bssSize)
        {
            res = null;

            if (_nroInfos.Count >= MaxNro)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.MaxNro);
            }
            else if (nroSize == 0 || nroAddress + nroSize <= nroAddress || (nroSize & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if (bssSize != 0 && bssAddress + bssSize <= bssAddress)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if ((nroAddress & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
            }

            uint magic       = context.Memory.ReadUInt32((long)nroAddress + 0x10);
            uint nroFileSize = context.Memory.ReadUInt32((long)nroAddress + 0x18);

            if (magic != NroMagic || nroSize != nroFileSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            byte[] nroData = context.Memory.ReadBytes((long)nroAddress, (long)nroSize);
            byte[] nroHash = null;

            MemoryStream stream = new MemoryStream(nroData);

            using (SHA256 hasher = SHA256.Create())
            {
                nroHash = hasher.ComputeHash(stream);
            }

            if (!IsNroHashPresent(nroHash))
            {
                return MakeError(ErrorModule.Loader, LoaderErr.NroHashNotPresent);
            }

            if (IsNroLoaded(nroHash))
            {
                return MakeError(ErrorModule.Loader, LoaderErr.NroAlreadyLoaded);
            }

            stream.Position = 0;

            NxRelocatableObject executable = new NxRelocatableObject(stream, nroAddress, bssAddress);

            // check if everything is page align.
            if ((executable.Text.Length & 0xFFF) != 0 || (executable.Ro.Length & 0xFFF) != 0 ||
                (executable.Data.Length & 0xFFF) != 0 || (executable.BssSize & 0xFFF)   != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            // check if everything is contiguous.
            if (executable.RoOffset   != executable.TextOffset + executable.Text.Length ||
                executable.DataOffset != executable.RoOffset   + executable.Ro.Length   ||
                nroFileSize           != executable.DataOffset + executable.Data.Length)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            // finally check the bss size match.
            if ((ulong)executable.BssSize != bssSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
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

            return 0;
        }

        private long MapNro(ServiceCtx context, NroInfo info, out ulong nroMappedAddress)
        {
            nroMappedAddress = 0;

            KMemoryManager memMgr = context.Process.MemoryManager;

            ulong targetAddress = memMgr.GetAddrSpaceBaseAddr();

            while (true)
            {
                if (targetAddress + info.TotalSize >= memMgr.AddrSpaceEnd)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
                }

                KMemoryInfo memInfo = memMgr.QueryMemory(targetAddress);

                if (memInfo.State == MemoryState.Unmapped && memInfo.Size >= info.TotalSize)
                {
                    if (!memMgr.InsideHeapRegion (targetAddress, info.TotalSize) &&
                        !memMgr.InsideAliasRegion(targetAddress, info.TotalSize))
                    {
                        break;
                    }
                }

                targetAddress += memInfo.Size;
            }

            KernelResult result = memMgr.MapProcessCodeMemory(targetAddress, info.NroAddress, info.NroSize);

            if (result != KernelResult.Success)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
            }

            ulong bssTargetAddress = targetAddress + info.NroSize;

            if (info.BssSize != 0)
            {
                result = memMgr.MapProcessCodeMemory(bssTargetAddress, info.BssAddress, info.BssSize);

                if (result != KernelResult.Success)
                {
                    memMgr.UnmapProcessCodeMemory(targetAddress, info.NroAddress, info.NroSize);

                    return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
                }
            }

            result = LoadNroIntoMemory(context.Process, info.Executable, targetAddress);

            if (result != KernelResult.Success)
            {
                memMgr.UnmapProcessCodeMemory(targetAddress, info.NroAddress, info.NroSize);

                if (info.BssSize != 0)
                {
                    memMgr.UnmapProcessCodeMemory(bssTargetAddress, info.BssAddress, info.BssSize);
                }

                return 0;
            }

            info.NroMappedAddress = targetAddress;
            nroMappedAddress      = targetAddress;

            return 0;
        }

        private KernelResult LoadNroIntoMemory(KProcess process, IExecutable relocatableObject, ulong baseAddress)
        {
            ulong textStart = baseAddress + (ulong)relocatableObject.TextOffset;
            ulong roStart   = baseAddress + (ulong)relocatableObject.RoOffset;
            ulong dataStart = baseAddress + (ulong)relocatableObject.DataOffset;

            ulong bssStart = dataStart + (ulong)relocatableObject.Data.Length;

            ulong bssEnd = BitUtils.AlignUp(bssStart + (ulong)relocatableObject.BssSize, KMemoryManager.PageSize);

            process.CpuMemory.WriteBytes((long)textStart, relocatableObject.Text);
            process.CpuMemory.WriteBytes((long)roStart,   relocatableObject.Ro);
            process.CpuMemory.WriteBytes((long)dataStart, relocatableObject.Data);

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

        private long RemoveNrrInfo(long nrrAddress)
        {
            foreach (NrrInfo info in _nrrInfos)
            {
                if (info.NrrAddress == nrrAddress)
                {
                    _nrrInfos.Remove(info);

                    return 0;
                }
            }

            return MakeError(ErrorModule.Loader, LoaderErr.BadNrrAddress);
        }

        private long RemoveNroInfo(ServiceCtx context, ulong nroMappedAddress)
        {
            foreach (NroInfo info in _nroInfos)
            {
                if (info.NroMappedAddress == nroMappedAddress)
                {
                    _nroInfos.Remove(info);

                    ulong textSize = (ulong)info.Executable.Text.Length;
                    ulong roSize   = (ulong)info.Executable.Ro.Length;
                    ulong dataSize = (ulong)info.Executable.Data.Length;
                    ulong bssSize  = (ulong)info.Executable.BssSize;

                    KernelResult result = KernelResult.Success;

                    if (info.Executable.BssSize != 0)
                    {
                        result = context.Process.MemoryManager.UnmapProcessCodeMemory(
                            info.NroMappedAddress + textSize + roSize + dataSize,
                            info.Executable.BssAddress,
                            bssSize);
                    }

                    if (result == KernelResult.Success)
                    {
                        result = context.Process.MemoryManager.UnmapProcessCodeMemory(
                            info.NroMappedAddress         + textSize + roSize,
                            info.Executable.SourceAddress + textSize + roSize,
                            dataSize);

                        if (result == KernelResult.Success)
                        {
                            result = context.Process.MemoryManager.UnmapProcessCodeMemory(
                                info.NroMappedAddress,
                                info.Executable.SourceAddress,
                                textSize + roSize);
                        }
                    }

                    return (long)result;
                }
            }

            return MakeError(ErrorModule.Loader, LoaderErr.BadNroAddress);
        }

        // LoadNro(u64, u64, u64, u64, u64, pid) -> u64
        public long LoadNro(ServiceCtx context)
        {
            long result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            context.RequestData.ReadUInt64();

            ulong nroHeapAddress = context.RequestData.ReadUInt64();
            ulong nroSize        = context.RequestData.ReadUInt64();
            ulong bssHeapAddress = context.RequestData.ReadUInt64();
            ulong bssSize        = context.RequestData.ReadUInt64();

            ulong nroMappedAddress = 0;

            if (_isInitialized)
            {
                NroInfo info;

                result = ParseNro(out info, context, nroHeapAddress, nroSize, bssHeapAddress, bssSize);

                if (result == 0)
                {
                    result = MapNro(context, info, out nroMappedAddress);

                    if (result == 0)
                    {
                        _nroInfos.Add(info);
                    }
                }
            }

            context.ResponseData.Write(nroMappedAddress);

            return result;
        }

        // UnloadNro(u64, u64, pid)
        public long UnloadNro(ServiceCtx context)
        {
            long result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            context.RequestData.ReadUInt64();

            ulong nroMappedAddress = context.RequestData.ReadUInt64();

            if (_isInitialized)
            {
                if ((nroMappedAddress & 0xFFF) != 0)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
                }

                result = RemoveNroInfo(context, nroMappedAddress);
            }

            return result;
        }

        // LoadNrr(u64, u64, u64, pid)
        public long LoadNrr(ServiceCtx context)
        {
            long result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            context.RequestData.ReadUInt64();

            long nrrAddress = context.RequestData.ReadInt64();
            long nrrSize    = context.RequestData.ReadInt64();

            if (_isInitialized)
            {
                NrrInfo info;
                result = ParseNrr(out info, context, nrrAddress, nrrSize);

                if(result == 0)
                {
                    if (_nrrInfos.Count >= MaxNrr)
                    {
                        result = MakeError(ErrorModule.Loader, LoaderErr.MaxNrr);
                    }
                    else
                    {
                        _nrrInfos.Add(info);
                    }
                }
            }

            return result;
        }

        // UnloadNrr(u64, u64, pid)
        public long UnloadNrr(ServiceCtx context)
        {
            long result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            context.RequestData.ReadUInt64();

            long nrrHeapAddress = context.RequestData.ReadInt64();

            if (_isInitialized)
            {
                if ((nrrHeapAddress & 0xFFF) != 0)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
                }

                result = RemoveNrrInfo(nrrHeapAddress);
            }

            return result;
        }

        // Initialize(u64, pid, KObject)
        public long Initialize(ServiceCtx context)
        {
            // TODO: we actually ignore the pid and process handle receive, we will need to use them when we will have multi process support.
            _isInitialized = true;

            return 0;
        }
    }
}
