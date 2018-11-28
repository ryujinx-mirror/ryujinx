using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Utilities;
using System;
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

        public NrrInfo(long NrrAddress, NrrHeader Header, List<byte[]> Hashes)
        {
            this.NrrAddress = NrrAddress;
            this.Header     = Header;
            this.Hashes     = Hashes;
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
            NxRelocatableObject Executable,
            byte[]              Hash,
            ulong               NroAddress,
            ulong               NroSize,
            ulong               BssAddress,
            ulong               BssSize,
            ulong               TotalSize)
        {
            this.Executable = Executable;
            this.Hash       = Hash;
            this.NroAddress = NroAddress;
            this.NroSize    = NroSize;
            this.BssAddress = BssAddress;
            this.BssSize    = BssSize;
            this.TotalSize  = TotalSize;
        }
    }

    class IRoInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private const int MaxNrr = 0x40;
        private const int MaxNro = 0x40;

        private const uint NrrMagic = 0x3052524E;
        private const uint NroMagic = 0x304F524E;

        private List<NrrInfo> NrrInfos;
        private List<NroInfo> NroInfos;

        private bool IsInitialized;

        public IRoInterface()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, LoadNro    },
                { 1, UnloadNro  },
                { 2, LoadNrr    },
                { 3, UnloadNrr  },
                { 4, Initialize },
            };

            NrrInfos = new List<NrrInfo>(MaxNrr);
            NroInfos = new List<NroInfo>(MaxNro);
        }

        private long ParseNrr(out NrrInfo NrrInfo, ServiceCtx Context, long NrrAddress, long NrrSize)
        {
            NrrInfo = null;

            if (NrrSize == 0 || NrrAddress + NrrSize <= NrrAddress || (NrrSize & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if ((NrrAddress & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
            }

            StructReader Reader = new StructReader(Context.Memory, NrrAddress);
            NrrHeader    Header = Reader.Read<NrrHeader>();

            if (Header.Magic != NrrMagic)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNrr);
            }
            else if (Header.NrrSize != NrrSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }

            List<byte[]> Hashes = new List<byte[]>();

            for (int i = 0; i < Header.HashCount; i++)
            {
                Hashes.Add(Context.Memory.ReadBytes(NrrAddress + Header.HashOffset + (i * 0x20), 0x20));
            }

            NrrInfo = new NrrInfo(NrrAddress, Header, Hashes);

            return 0;
        }

        public bool IsNroHashPresent(byte[] NroHash)
        {
            foreach (NrrInfo Info in NrrInfos)
            {
                foreach (byte[] Hash in Info.Hashes)
                {
                    if (Hash.SequenceEqual(NroHash))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsNroLoaded(byte[] NroHash)
        {
            foreach (NroInfo Info in NroInfos)
            {
                if (Info.Hash.SequenceEqual(NroHash))
                {
                    return true;
                }
            }

            return false;
        }

        public long ParseNro(out NroInfo Res, ServiceCtx Context, ulong NroAddress, ulong NroSize, ulong BssAddress, ulong BssSize)
        {
            Res = null;

            if (NroInfos.Count >= MaxNro)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.MaxNro);
            }
            else if (NroSize == 0 || NroAddress + NroSize <= NroAddress || (NroSize & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if (BssSize != 0 && BssAddress + BssSize <= BssAddress)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.BadSize);
            }
            else if ((NroAddress & 0xFFF) != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
            }

            uint Magic       = Context.Memory.ReadUInt32((long)NroAddress + 0x10);
            uint NroFileSize = Context.Memory.ReadUInt32((long)NroAddress + 0x18);

            if (Magic != NroMagic || NroSize != NroFileSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            byte[] NroData = Context.Memory.ReadBytes((long)NroAddress, (long)NroSize);
            byte[] NroHash = null;

            MemoryStream Stream = new MemoryStream(NroData);

            using (SHA256 Hasher = SHA256.Create())
            {
                NroHash = Hasher.ComputeHash(Stream);
            }

            if (!IsNroHashPresent(NroHash))
            {
                return MakeError(ErrorModule.Loader, LoaderErr.NroHashNotPresent);
            }

            if (IsNroLoaded(NroHash))
            {
                return MakeError(ErrorModule.Loader, LoaderErr.NroAlreadyLoaded);
            }

            Stream.Position = 0;

            NxRelocatableObject Executable = new NxRelocatableObject(Stream, NroAddress, BssAddress);

            // check if everything is page align.
            if ((Executable.Text.Length & 0xFFF) != 0 || (Executable.RO.Length & 0xFFF) != 0 ||
                (Executable.Data.Length & 0xFFF) != 0 || (Executable.BssSize & 0xFFF)   != 0)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            // check if everything is contiguous.
            if (Executable.ROOffset   != Executable.TextOffset + Executable.Text.Length ||
                Executable.DataOffset != Executable.ROOffset   + Executable.RO.Length   ||
                NroFileSize           != Executable.DataOffset + Executable.Data.Length)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            // finally check the bss size match.
            if ((ulong)Executable.BssSize != BssSize)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidNro);
            }

            int TotalSize = Executable.Text.Length + Executable.RO.Length + Executable.Data.Length + Executable.BssSize;

            Res = new NroInfo(
                Executable,
                NroHash,
                NroAddress,
                NroSize,
                BssAddress,
                BssSize,
                (ulong)TotalSize);

            return 0;
        }

        private long MapNro(ServiceCtx Context, NroInfo Info, out ulong NroMappedAddress)
        {
            NroMappedAddress = 0;

            KMemoryManager MemMgr = Context.Process.MemoryManager;

            ulong TargetAddress = MemMgr.GetAddrSpaceBaseAddr();

            while (true)
            {
                if (TargetAddress + Info.TotalSize >= MemMgr.AddrSpaceEnd)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
                }

                KMemoryInfo MemInfo = MemMgr.QueryMemory(TargetAddress);

                if (MemInfo.State == MemoryState.Unmapped && MemInfo.Size >= Info.TotalSize)
                {
                    if (!MemMgr.InsideHeapRegion (TargetAddress, Info.TotalSize) &&
                        !MemMgr.InsideAliasRegion(TargetAddress, Info.TotalSize))
                    {
                        break;
                    }
                }

                TargetAddress += MemInfo.Size;
            }

            KernelResult Result = MemMgr.MapProcessCodeMemory(TargetAddress, Info.NroAddress, Info.NroSize);

            if (Result != KernelResult.Success)
            {
                return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
            }

            ulong BssTargetAddress = TargetAddress + Info.NroSize;

            if (Info.BssSize != 0)
            {
                Result = MemMgr.MapProcessCodeMemory(BssTargetAddress, Info.BssAddress, Info.BssSize);

                if (Result != KernelResult.Success)
                {
                    MemMgr.UnmapProcessCodeMemory(TargetAddress, Info.NroAddress, Info.NroSize);

                    return MakeError(ErrorModule.Loader, LoaderErr.InvalidMemoryState);
                }
            }

            Result = LoadNroIntoMemory(Context.Process, Info.Executable, TargetAddress);

            if (Result != KernelResult.Success)
            {
                MemMgr.UnmapProcessCodeMemory(TargetAddress, Info.NroAddress, Info.NroSize);

                if (Info.BssSize != 0)
                {
                    MemMgr.UnmapProcessCodeMemory(BssTargetAddress, Info.BssAddress, Info.BssSize);
                }

                return 0;
            }

            Info.NroMappedAddress = TargetAddress;
            NroMappedAddress      = TargetAddress;

            return 0;
        }

        private KernelResult LoadNroIntoMemory(KProcess Process, IExecutable RelocatableObject, ulong BaseAddress)
        {
            ulong TextStart = BaseAddress + (ulong)RelocatableObject.TextOffset;
            ulong ROStart   = BaseAddress + (ulong)RelocatableObject.ROOffset;
            ulong DataStart = BaseAddress + (ulong)RelocatableObject.DataOffset;

            ulong BssStart = DataStart + (ulong)RelocatableObject.Data.Length;

            ulong BssEnd = BitUtils.AlignUp(BssStart + (ulong)RelocatableObject.BssSize, KMemoryManager.PageSize);

            Process.CpuMemory.WriteBytes((long)TextStart, RelocatableObject.Text);
            Process.CpuMemory.WriteBytes((long)ROStart,   RelocatableObject.RO);
            Process.CpuMemory.WriteBytes((long)DataStart, RelocatableObject.Data);

            MemoryHelper.FillWithZeros(Process.CpuMemory, (long)BssStart, (int)(BssEnd - BssStart));

            KernelResult Result;

            Result = Process.MemoryManager.SetProcessMemoryPermission(TextStart, ROStart - TextStart, MemoryPermission.ReadAndExecute);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            Result = Process.MemoryManager.SetProcessMemoryPermission(ROStart, DataStart - ROStart, MemoryPermission.Read);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            return Process.MemoryManager.SetProcessMemoryPermission(DataStart, BssEnd - DataStart, MemoryPermission.ReadAndWrite);
        }

        private long RemoveNrrInfo(long NrrAddress)
        {
            foreach (NrrInfo Info in NrrInfos)
            {
                if (Info.NrrAddress == NrrAddress)
                {
                    NrrInfos.Remove(Info);

                    return 0;
                }
            }

            return MakeError(ErrorModule.Loader, LoaderErr.BadNrrAddress);
        }

        private long RemoveNroInfo(ServiceCtx Context, ulong NroMappedAddress)
        {
            foreach (NroInfo Info in NroInfos)
            {
                if (Info.NroMappedAddress == NroMappedAddress)
                {
                    NroInfos.Remove(Info);

                    ulong TextSize = (ulong)Info.Executable.Text.Length;
                    ulong ROSize   = (ulong)Info.Executable.RO.Length;
                    ulong DataSize = (ulong)Info.Executable.Data.Length;
                    ulong BssSize  = (ulong)Info.Executable.BssSize;

                    KernelResult Result = KernelResult.Success;

                    if (Info.Executable.BssSize != 0)
                    {
                        Result = Context.Process.MemoryManager.UnmapProcessCodeMemory(
                            Info.NroMappedAddress + TextSize + ROSize + DataSize,
                            Info.Executable.BssAddress,
                            BssSize);
                    }

                    if (Result == KernelResult.Success)
                    {
                        Result = Context.Process.MemoryManager.UnmapProcessCodeMemory(
                            Info.NroMappedAddress         + TextSize + ROSize,
                            Info.Executable.SourceAddress + TextSize + ROSize,
                            DataSize);

                        if (Result == KernelResult.Success)
                        {
                            Result = Context.Process.MemoryManager.UnmapProcessCodeMemory(
                                Info.NroMappedAddress,
                                Info.Executable.SourceAddress,
                                TextSize + ROSize);
                        }
                    }

                    return (long)Result;
                }
            }

            return MakeError(ErrorModule.Loader, LoaderErr.BadNroAddress);
        }

        // LoadNro(u64, u64, u64, u64, u64, pid) -> u64
        public long LoadNro(ServiceCtx Context)
        {
            long Result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            Context.RequestData.ReadUInt64();

            ulong NroHeapAddress = Context.RequestData.ReadUInt64();
            ulong NroSize        = Context.RequestData.ReadUInt64();
            ulong BssHeapAddress = Context.RequestData.ReadUInt64();
            ulong BssSize        = Context.RequestData.ReadUInt64();

            ulong NroMappedAddress = 0;

            if (IsInitialized)
            {
                NroInfo Info;

                Result = ParseNro(out Info, Context, NroHeapAddress, NroSize, BssHeapAddress, BssSize);

                if (Result == 0)
                {
                    Result = MapNro(Context, Info, out NroMappedAddress);

                    if (Result == 0)
                    {
                        NroInfos.Add(Info);
                    }
                }
            }

            Context.ResponseData.Write(NroMappedAddress);

            return Result;
        }

        // UnloadNro(u64, u64, pid)
        public long UnloadNro(ServiceCtx Context)
        {
            long Result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            Context.RequestData.ReadUInt64();

            ulong NroMappedAddress = Context.RequestData.ReadUInt64();

            if (IsInitialized)
            {
                if ((NroMappedAddress & 0xFFF) != 0)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
                }

                Result = RemoveNroInfo(Context, NroMappedAddress);
            }

            return Result;
        }

        // LoadNrr(u64, u64, u64, pid)
        public long LoadNrr(ServiceCtx Context)
        {
            long Result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            Context.RequestData.ReadUInt64();

            long NrrAddress = Context.RequestData.ReadInt64();
            long NrrSize    = Context.RequestData.ReadInt64();

            if (IsInitialized)
            {
                NrrInfo Info;
                Result = ParseNrr(out Info, Context, NrrAddress, NrrSize);

                if(Result == 0)
                {
                    if (NrrInfos.Count >= MaxNrr)
                    {
                        Result = MakeError(ErrorModule.Loader, LoaderErr.MaxNrr);
                    }
                    else
                    {
                        NrrInfos.Add(Info);
                    }
                }
            }

            return Result;
        }

        // UnloadNrr(u64, u64, pid)
        public long UnloadNrr(ServiceCtx Context)
        {
            long Result = MakeError(ErrorModule.Loader, LoaderErr.BadInitialization);

            // Zero
            Context.RequestData.ReadUInt64();

            long NrrHeapAddress = Context.RequestData.ReadInt64();

            if (IsInitialized)
            {
                if ((NrrHeapAddress & 0xFFF) != 0)
                {
                    return MakeError(ErrorModule.Loader, LoaderErr.UnalignedAddress);
                }

                Result = RemoveNrrInfo(NrrHeapAddress);
            }

            return Result;
        }

        // Initialize(u64, pid, KObject)
        public long Initialize(ServiceCtx Context)
        {
            // TODO: we actually ignore the pid and process handle receive, we will need to use them when we will have multi process support.
            IsInitialized = true;

            return 0;
        }
    }
}
