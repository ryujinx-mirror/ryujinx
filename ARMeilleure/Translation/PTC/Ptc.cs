using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.Unwinding;
using ARMeilleure.CodeGen.X86;
using ARMeilleure.Common;
using ARMeilleure.Memory;
using ARMeilleure.Translation.Cache;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using static ARMeilleure.Translation.PTC.PtcFormatter;

namespace ARMeilleure.Translation.PTC
{
    public static class Ptc
    {
        private const string OuterHeaderMagicString = "PTCohd\0\0";
        private const string InnerHeaderMagicString = "PTCihd\0\0";

        private const uint InternalVersion = 2285; //! To be incremented manually for each change to the ARMeilleure project.

        private const string ActualDir = "0";
        private const string BackupDir = "1";

        private const string TitleIdTextDefault = "0000000000000000";
        private const string DisplayVersionDefault = "0";

        internal const int PageTablePointerIndex = -1; // Must be a negative value.
        internal const int JumpPointerIndex = -2; // Must be a negative value.
        internal const int DynamicPointerIndex = -3; // Must be a negative value.
        internal const int CountTableIndex = -4; // Must be a negative value.

        private const byte FillingByte = 0x00;
        private const CompressionLevel SaveCompressionLevel = CompressionLevel.Fastest;

        // Carriers.
        private static MemoryStream _infosStream;
        private static List<byte[]> _codesList;
        private static MemoryStream _relocsStream;
        private static MemoryStream _unwindInfosStream;

        private static readonly ulong _outerHeaderMagic;
        private static readonly ulong _innerHeaderMagic;

        private static readonly ManualResetEvent _waitEvent;

        private static readonly object _lock;

        private static bool _disposed;

        internal static PtcJumpTable PtcJumpTable { get; private set; }

        internal static string TitleIdText { get; private set; }
        internal static string DisplayVersion { get; private set; }

        internal static string CachePathActual { get; private set; }
        internal static string CachePathBackup { get; private set; }

        internal static PtcState State { get; private set; }

        // Progress reporting helpers.
        private static volatile int _translateCount;
        private static volatile int _translateTotalCount;
        public static event Action<PtcLoadingState, int, int> PtcStateChanged;

        static Ptc()
        {
            InitializeCarriers();

            _outerHeaderMagic = BinaryPrimitives.ReadUInt64LittleEndian(EncodingCache.UTF8NoBOM.GetBytes(OuterHeaderMagicString).AsSpan());
            _innerHeaderMagic = BinaryPrimitives.ReadUInt64LittleEndian(EncodingCache.UTF8NoBOM.GetBytes(InnerHeaderMagicString).AsSpan());

            _waitEvent = new ManualResetEvent(true);

            _lock = new object();

            _disposed = false;

            PtcJumpTable = new PtcJumpTable();

            TitleIdText = TitleIdTextDefault;
            DisplayVersion = DisplayVersionDefault;

            CachePathActual = string.Empty;
            CachePathBackup = string.Empty;

            Disable();
        }

        public static void Initialize(string titleIdText, string displayVersion, bool enabled)
        {
            Wait();

            PtcProfiler.Wait();
            PtcProfiler.ClearEntries();

            Logger.Info?.Print(LogClass.Ptc, $"Initializing Profiled Persistent Translation Cache (enabled: {enabled}).");

            if (!enabled || string.IsNullOrEmpty(titleIdText) || titleIdText == TitleIdTextDefault)
            {
                TitleIdText = TitleIdTextDefault;
                DisplayVersion = DisplayVersionDefault;

                CachePathActual = string.Empty;
                CachePathBackup = string.Empty;

                Disable();

                return;
            }

            TitleIdText = titleIdText;
            DisplayVersion = !string.IsNullOrEmpty(displayVersion) ? displayVersion : DisplayVersionDefault;

            string workPathActual = Path.Combine(AppDataManager.GamesDirPath, TitleIdText, "cache", "cpu", ActualDir);
            string workPathBackup = Path.Combine(AppDataManager.GamesDirPath, TitleIdText, "cache", "cpu", BackupDir);

            if (!Directory.Exists(workPathActual))
            {
                Directory.CreateDirectory(workPathActual);
            }

            if (!Directory.Exists(workPathBackup))
            {
                Directory.CreateDirectory(workPathBackup);
            }

            CachePathActual = Path.Combine(workPathActual, DisplayVersion);
            CachePathBackup = Path.Combine(workPathBackup, DisplayVersion);

            PreLoad();
            PtcProfiler.PreLoad();

            Enable();
        }

        private static void InitializeCarriers()
        {
            _infosStream = new MemoryStream();
            _codesList = new List<byte[]>();
            _relocsStream = new MemoryStream();
            _unwindInfosStream = new MemoryStream();
        }

        private static void DisposeCarriers()
        {
            _infosStream.Dispose();
            _codesList.Clear();
            _relocsStream.Dispose();
            _unwindInfosStream.Dispose();
        }

        private static bool AreCarriersEmpty()
        {
            return _infosStream.Length == 0L && _codesList.Count == 0 && _relocsStream.Length == 0L && _unwindInfosStream.Length == 0L;
        }

        private static void ResetCarriersIfNeeded()
        {
            if (AreCarriersEmpty())
            {
                return;
            }

            DisposeCarriers();

            InitializeCarriers();
        }

        private static void PreLoad()
        {
            string fileNameActual = string.Concat(CachePathActual, ".cache");
            string fileNameBackup = string.Concat(CachePathBackup, ".cache");

            FileInfo fileInfoActual = new FileInfo(fileNameActual);
            FileInfo fileInfoBackup = new FileInfo(fileNameBackup);

            if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
            {
                if (!Load(fileNameActual, false))
                {
                    if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
                    {
                        Load(fileNameBackup, true);
                    }
                }
            }
            else if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
            {
                Load(fileNameBackup, true);
            }
        }

        private static unsafe bool Load(string fileName, bool isBackup)
        {
            using (FileStream compressedStream = new(fileName, FileMode.Open))
            using (DeflateStream deflateStream = new(compressedStream, CompressionMode.Decompress, true))
            {
                OuterHeader outerHeader = DeserializeStructure<OuterHeader>(compressedStream);

                if (!outerHeader.IsHeaderValid())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.Magic != _outerHeaderMagic)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.CacheFileVersion != InternalVersion)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.Endianness != GetEndianness())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.FeatureInfo != GetFeatureInfo())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.OSPlatform != GetOSPlatform())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                IntPtr intPtr = IntPtr.Zero;

                try
                {
                    intPtr = Marshal.AllocHGlobal(new IntPtr(outerHeader.UncompressedStreamSize));

                    using (UnmanagedMemoryStream stream = new((byte*)intPtr.ToPointer(), outerHeader.UncompressedStreamSize, outerHeader.UncompressedStreamSize, FileAccess.ReadWrite))
                    {
                        try
                        {
                            deflateStream.CopyTo(stream);
                        }
                        catch
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        Debug.Assert(stream.Position == stream.Length);

                        stream.Seek(0L, SeekOrigin.Begin);

                        InnerHeader innerHeader = DeserializeStructure<InnerHeader>(stream);

                        if (!innerHeader.IsHeaderValid())
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        if (innerHeader.Magic != _innerHeaderMagic)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        ReadOnlySpan<byte> infosBytes = new(stream.PositionPointer, innerHeader.InfosLength);
                        stream.Seek(innerHeader.InfosLength, SeekOrigin.Current);

                        Hash128 infosHash = XXHash128.ComputeHash(infosBytes);

                        if (innerHeader.InfosHash != infosHash)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        ReadOnlySpan<byte> codesBytes = (int)innerHeader.CodesLength > 0 ? new(stream.PositionPointer, (int)innerHeader.CodesLength) : ReadOnlySpan<byte>.Empty;
                        stream.Seek(innerHeader.CodesLength, SeekOrigin.Current);

                        Hash128 codesHash = XXHash128.ComputeHash(codesBytes);

                        if (innerHeader.CodesHash != codesHash)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        ReadOnlySpan<byte> relocsBytes = new(stream.PositionPointer, innerHeader.RelocsLength);
                        stream.Seek(innerHeader.RelocsLength, SeekOrigin.Current);

                        Hash128 relocsHash = XXHash128.ComputeHash(relocsBytes);

                        if (innerHeader.RelocsHash != relocsHash)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        ReadOnlySpan<byte> unwindInfosBytes = new(stream.PositionPointer, innerHeader.UnwindInfosLength);
                        stream.Seek(innerHeader.UnwindInfosLength, SeekOrigin.Current);

                        Hash128 unwindInfosHash = XXHash128.ComputeHash(unwindInfosBytes);

                        if (innerHeader.UnwindInfosHash != unwindInfosHash)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        ReadOnlySpan<byte> ptcJumpTableBytes = new(stream.PositionPointer, innerHeader.PtcJumpTableLength);
                        stream.Seek(innerHeader.PtcJumpTableLength, SeekOrigin.Current);

                        Debug.Assert(stream.Position == stream.Length);

                        Hash128 ptcJumpTableHash = XXHash128.ComputeHash(ptcJumpTableBytes);

                        if (innerHeader.PtcJumpTableHash != ptcJumpTableHash)
                        {
                            InvalidateCompressedStream(compressedStream);

                            return false;
                        }

                        stream.Seek((long)Unsafe.SizeOf<InnerHeader>(), SeekOrigin.Begin);

                        _infosStream.Write(infosBytes);
                        stream.Seek(innerHeader.InfosLength, SeekOrigin.Current);

                        _codesList.ReadFrom(stream);

                        _relocsStream.Write(relocsBytes);
                        stream.Seek(innerHeader.RelocsLength, SeekOrigin.Current);

                        _unwindInfosStream.Write(unwindInfosBytes);
                        stream.Seek(innerHeader.UnwindInfosLength, SeekOrigin.Current);

                        PtcJumpTable = PtcJumpTable.Deserialize(stream);

                        Debug.Assert(stream.Position == stream.Length);
                    }
                }
                finally
                {
                    if (intPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(intPtr);
                    }
                }
            }

            long fileSize = new FileInfo(fileName).Length;

            Logger.Info?.Print(LogClass.Ptc, $"{(isBackup ? "Loaded Backup Translation Cache" : "Loaded Translation Cache")} (size: {fileSize} bytes, translated functions: {GetEntriesCount()}).");

            return true;
        }

        private static void InvalidateCompressedStream(FileStream compressedStream)
        {
            compressedStream.SetLength(0L);
        }

        private static void PreSave()
        {
            _waitEvent.Reset();

            try
            {
                string fileNameActual = string.Concat(CachePathActual, ".cache");
                string fileNameBackup = string.Concat(CachePathBackup, ".cache");

                FileInfo fileInfoActual = new FileInfo(fileNameActual);

                if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
                {
                    File.Copy(fileNameActual, fileNameBackup, true);
                }

                Save(fileNameActual);
            }
            finally
            {
                ResetCarriersIfNeeded();
                PtcJumpTable.ClearIfNeeded();

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            }

            _waitEvent.Set();
        }

        private static unsafe void Save(string fileName)
        {
            int translatedFuncsCount;

            InnerHeader innerHeader = new InnerHeader();

            innerHeader.Magic = _innerHeaderMagic;

            innerHeader.InfosLength = (int)_infosStream.Length;
            innerHeader.CodesLength = _codesList.Length();
            innerHeader.RelocsLength = (int)_relocsStream.Length;
            innerHeader.UnwindInfosLength = (int)_unwindInfosStream.Length;
            innerHeader.PtcJumpTableLength = PtcJumpTable.GetSerializeSize(PtcJumpTable);

            OuterHeader outerHeader = new OuterHeader();

            outerHeader.Magic = _outerHeaderMagic;

            outerHeader.CacheFileVersion = InternalVersion;
            outerHeader.Endianness = GetEndianness();
            outerHeader.FeatureInfo = GetFeatureInfo();
            outerHeader.OSPlatform = GetOSPlatform();

            outerHeader.UncompressedStreamSize =
                (long)Unsafe.SizeOf<InnerHeader>() +
                innerHeader.InfosLength +
                innerHeader.CodesLength +
                innerHeader.RelocsLength +
                innerHeader.UnwindInfosLength +
                innerHeader.PtcJumpTableLength;

            outerHeader.SetHeaderHash();

            IntPtr intPtr = IntPtr.Zero;

            try
            {
                intPtr = Marshal.AllocHGlobal(new IntPtr(outerHeader.UncompressedStreamSize));

                using (UnmanagedMemoryStream stream = new((byte*)intPtr.ToPointer(), outerHeader.UncompressedStreamSize, outerHeader.UncompressedStreamSize, FileAccess.ReadWrite))
                {
                    stream.Seek((long)Unsafe.SizeOf<InnerHeader>(), SeekOrigin.Begin);

                    ReadOnlySpan<byte> infosBytes = new(stream.PositionPointer, innerHeader.InfosLength);
                    _infosStream.WriteTo(stream);

                    ReadOnlySpan<byte> codesBytes = (int)innerHeader.CodesLength > 0 ? new(stream.PositionPointer, (int)innerHeader.CodesLength) : ReadOnlySpan<byte>.Empty;
                    _codesList.WriteTo(stream);

                    ReadOnlySpan<byte> relocsBytes = new(stream.PositionPointer, innerHeader.RelocsLength);
                    _relocsStream.WriteTo(stream);

                    ReadOnlySpan<byte> unwindInfosBytes = new(stream.PositionPointer, innerHeader.UnwindInfosLength);
                    _unwindInfosStream.WriteTo(stream);

                    ReadOnlySpan<byte> ptcJumpTableBytes = new(stream.PositionPointer, innerHeader.PtcJumpTableLength);
                    PtcJumpTable.Serialize(stream, PtcJumpTable);

                    Debug.Assert(stream.Position == stream.Length);

                    innerHeader.InfosHash = XXHash128.ComputeHash(infosBytes);
                    innerHeader.CodesHash = XXHash128.ComputeHash(codesBytes);
                    innerHeader.RelocsHash = XXHash128.ComputeHash(relocsBytes);
                    innerHeader.UnwindInfosHash = XXHash128.ComputeHash(unwindInfosBytes);
                    innerHeader.PtcJumpTableHash = XXHash128.ComputeHash(ptcJumpTableBytes);

                    innerHeader.SetHeaderHash();

                    stream.Seek(0L, SeekOrigin.Begin);
                    SerializeStructure(stream, innerHeader);

                    translatedFuncsCount = GetEntriesCount();

                    ResetCarriersIfNeeded();
                    PtcJumpTable.ClearIfNeeded();

                    using (FileStream compressedStream = new(fileName, FileMode.OpenOrCreate))
                    using (DeflateStream deflateStream = new(compressedStream, SaveCompressionLevel, true))
                    {
                        try
                        {
                            SerializeStructure(compressedStream, outerHeader);

                            stream.Seek(0L, SeekOrigin.Begin);
                            stream.CopyTo(deflateStream);
                        }
                        catch
                        {
                            compressedStream.Position = 0L;
                        }

                        if (compressedStream.Position < compressedStream.Length)
                        {
                            compressedStream.SetLength(compressedStream.Position);
                        }
                    }
                }
            }
            finally
            {
                if (intPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(intPtr);
                }
            }

            long fileSize = new FileInfo(fileName).Length;

            if (fileSize != 0L)
            {
                Logger.Info?.Print(LogClass.Ptc, $"Saved Translation Cache (size: {fileSize} bytes, translated functions: {translatedFuncsCount}).");
            }
        }

        internal static void LoadTranslations(
            ConcurrentDictionary<ulong, TranslatedFunction> funcs,
            IMemoryManager memory,
            JumpTable jumpTable,
            EntryTable<uint> countTable)
        {
            if (AreCarriersEmpty())
            {
                return;
            }

            long infosStreamLength = _infosStream.Length;
            long relocsStreamLength = _relocsStream.Length;
            long unwindInfosStreamLength = _unwindInfosStream.Length;

            _infosStream.Seek(0L, SeekOrigin.Begin);
            _relocsStream.Seek(0L, SeekOrigin.Begin);
            _unwindInfosStream.Seek(0L, SeekOrigin.Begin);

            using (BinaryReader relocsReader = new(_relocsStream, EncodingCache.UTF8NoBOM, true))
            using (BinaryReader unwindInfosReader = new(_unwindInfosStream, EncodingCache.UTF8NoBOM, true))
            {
                for (int index = 0; index < GetEntriesCount(); index++)
                {
                    InfoEntry infoEntry = DeserializeStructure<InfoEntry>(_infosStream);

                    if (infoEntry.Stubbed)
                    {
                        SkipCode(index, infoEntry.CodeLength);
                        SkipReloc(infoEntry.RelocEntriesCount);
                        SkipUnwindInfo(unwindInfosReader);

                        continue;
                    }

                    bool isEntryChanged = infoEntry.Hash != ComputeHash(memory, infoEntry.Address, infoEntry.GuestSize);

                    if (isEntryChanged || (!infoEntry.HighCq && PtcProfiler.ProfiledFuncs.TryGetValue(infoEntry.Address, out var value) && value.HighCq))
                    {
                        infoEntry.Stubbed = true;
                        infoEntry.CodeLength = 0;
                        UpdateInfo(infoEntry);

                        StubCode(index);
                        StubReloc(infoEntry.RelocEntriesCount);
                        StubUnwindInfo(unwindInfosReader);

                        if (isEntryChanged)
                        {
                            PtcJumpTable.Clean(infoEntry.Address);

                            Logger.Info?.Print(LogClass.Ptc, $"Invalidated translated function (address: 0x{infoEntry.Address:X16})");
                        }

                        continue;
                    }

                    byte[] code = ReadCode(index, infoEntry.CodeLength);

                    Counter<uint> callCounter = null;

                    if (infoEntry.RelocEntriesCount != 0)
                    {
                        RelocEntry[] relocEntries = GetRelocEntries(relocsReader, infoEntry.RelocEntriesCount);

                        PatchCode(code, relocEntries, memory.PageTablePointer, jumpTable, countTable, out callCounter);
                    }

                    UnwindInfo unwindInfo = ReadUnwindInfo(unwindInfosReader);

                    TranslatedFunction func = FastTranslate(code, callCounter, infoEntry.GuestSize, unwindInfo, infoEntry.HighCq);

                    bool isAddressUnique = funcs.TryAdd(infoEntry.Address, func);

                    Debug.Assert(isAddressUnique, $"The address 0x{infoEntry.Address:X16} is not unique.");
                }
            }

            if (_infosStream.Length != infosStreamLength || _infosStream.Position != infosStreamLength ||
                _relocsStream.Length != relocsStreamLength || _relocsStream.Position != relocsStreamLength ||
                _unwindInfosStream.Length != unwindInfosStreamLength || _unwindInfosStream.Position != unwindInfosStreamLength)
            {
                throw new Exception("The length of a memory stream has changed, or its position has not reached or has exceeded its end.");
            }

            jumpTable.Initialize(PtcJumpTable, funcs);

            PtcJumpTable.WriteJumpTable(jumpTable, funcs);
            PtcJumpTable.WriteDynamicTable(jumpTable);

            Logger.Info?.Print(LogClass.Ptc, $"{funcs.Count} translated functions loaded");
        }

        private static int GetEntriesCount()
        {
            return _codesList.Count;
        }

        [Conditional("DEBUG")]
        private static void SkipCode(int index, int codeLength)
        {
            Debug.Assert(_codesList[index].Length == 0);
            Debug.Assert(codeLength == 0);
        }

        private static void SkipReloc(int relocEntriesCount)
        {
            _relocsStream.Seek(relocEntriesCount * RelocEntry.Stride, SeekOrigin.Current);
        }

        private static void SkipUnwindInfo(BinaryReader unwindInfosReader)
        {
            int pushEntriesLength = unwindInfosReader.ReadInt32();

            _unwindInfosStream.Seek(pushEntriesLength * UnwindPushEntry.Stride + UnwindInfo.Stride, SeekOrigin.Current);
        }

        private static byte[] ReadCode(int index, int codeLength)
        {
            Debug.Assert(_codesList[index].Length == codeLength);

            return _codesList[index];
        }

        private static RelocEntry[] GetRelocEntries(BinaryReader relocsReader, int relocEntriesCount)
        {
            RelocEntry[] relocEntries = new RelocEntry[relocEntriesCount];

            for (int i = 0; i < relocEntriesCount; i++)
            {
                int position = relocsReader.ReadInt32();
                int index = relocsReader.ReadInt32();

                relocEntries[i] = new RelocEntry(position, index);
            }

            return relocEntries;
        }

        private static void PatchCode(
            Span<byte> code,
            RelocEntry[] relocEntries,
            IntPtr pageTablePointer,
            JumpTable jumpTable,
            EntryTable<uint> countTable,
            out Counter<uint> callCounter)
        {
            callCounter = null;

            foreach (RelocEntry relocEntry in relocEntries)
            {
                ulong imm;

                if (relocEntry.Index == PageTablePointerIndex)
                {
                    imm = (ulong)pageTablePointer.ToInt64();
                }
                else if (relocEntry.Index == JumpPointerIndex)
                {
                    imm = (ulong)jumpTable.JumpPointer.ToInt64();
                }
                else if (relocEntry.Index == DynamicPointerIndex)
                {
                    imm = (ulong)jumpTable.DynamicPointer.ToInt64();
                }
                else if (relocEntry.Index == CountTableIndex)
                {
                    callCounter = new Counter<uint>(countTable);

                    unsafe { imm = (ulong)Unsafe.AsPointer(ref callCounter.Value); }
                }
                else if (Delegates.TryGetDelegateFuncPtrByIndex(relocEntry.Index, out IntPtr funcPtr))
                {
                    imm = (ulong)funcPtr.ToInt64();
                }
                else
                {
                    throw new Exception($"Unexpected reloc entry {relocEntry}.");
                }

                BinaryPrimitives.WriteUInt64LittleEndian(code.Slice(relocEntry.Position, 8), imm);
            }
        }

        private static UnwindInfo ReadUnwindInfo(BinaryReader unwindInfosReader)
        {
            int pushEntriesLength = unwindInfosReader.ReadInt32();

            UnwindPushEntry[] pushEntries = new UnwindPushEntry[pushEntriesLength];

            for (int i = 0; i < pushEntriesLength; i++)
            {
                int pseudoOp = unwindInfosReader.ReadInt32();
                int prologOffset = unwindInfosReader.ReadInt32();
                int regIndex = unwindInfosReader.ReadInt32();
                int stackOffsetOrAllocSize = unwindInfosReader.ReadInt32();

                pushEntries[i] = new UnwindPushEntry((UnwindPseudoOp)pseudoOp, prologOffset, regIndex, stackOffsetOrAllocSize);
            }

            int prologueSize = unwindInfosReader.ReadInt32();

            return new UnwindInfo(pushEntries, prologueSize);
        }

        private static TranslatedFunction FastTranslate(
            byte[] code,
            Counter<uint> callCounter,
            ulong guestSize,
            UnwindInfo unwindInfo,
            bool highCq)
        {
            CompiledFunction cFunc = new CompiledFunction(code, unwindInfo);

            IntPtr codePtr = JitCache.Map(cFunc);

            GuestFunction gFunc = Marshal.GetDelegateForFunctionPointer<GuestFunction>(codePtr);

            TranslatedFunction tFunc = new TranslatedFunction(gFunc, callCounter, guestSize, highCq);

            return tFunc;
        }

        private static void UpdateInfo(InfoEntry infoEntry)
        {
            _infosStream.Seek(-Unsafe.SizeOf<InfoEntry>(), SeekOrigin.Current);

            SerializeStructure(_infosStream, infoEntry);
        }

        private static void StubCode(int index)
        {
            _codesList[index] = Array.Empty<byte>();
        }

        private static void StubReloc(int relocEntriesCount)
        {
            for (int i = 0; i < relocEntriesCount * RelocEntry.Stride; i++)
            {
                _relocsStream.WriteByte(FillingByte);
            }
        }

        private static void StubUnwindInfo(BinaryReader unwindInfosReader)
        {
            int pushEntriesLength = unwindInfosReader.ReadInt32();

            for (int i = 0; i < pushEntriesLength * UnwindPushEntry.Stride + UnwindInfo.Stride; i++)
            {
                _unwindInfosStream.WriteByte(FillingByte);
            }
        }

        internal static void MakeAndSaveTranslations(
            ConcurrentDictionary<ulong, TranslatedFunction> funcs,
            IMemoryManager memory,
            JumpTable jumpTable,
            EntryTable<uint> countTable)
        {
            var profiledFuncsToTranslate = PtcProfiler.GetProfiledFuncsToTranslate(funcs);

            _translateCount = 0;
            _translateTotalCount = profiledFuncsToTranslate.Count;

            int degreeOfParallelism = new DegreeOfParallelism(4d, 75d, 12.5d).GetDegreeOfParallelism(0, 32);

            if (_translateTotalCount == 0 || degreeOfParallelism == 0)
            {
                ResetCarriersIfNeeded();
                PtcJumpTable.ClearIfNeeded();

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

                return;
            }

            Logger.Info?.Print(LogClass.Ptc, $"{_translateCount} of {_translateTotalCount} functions translated | Thread count: {degreeOfParallelism}");

            PtcStateChanged?.Invoke(PtcLoadingState.Start, _translateCount, _translateTotalCount);

            using AutoResetEvent progressReportEvent = new AutoResetEvent(false);

            Thread progressReportThread = new Thread(ReportProgress)
            {
                Name = "Ptc.ProgressReporter",
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };

            progressReportThread.Start(progressReportEvent);

            void TranslateFuncs()
            {
                while (profiledFuncsToTranslate.TryDequeue(out var item))
                {
                    ulong address = item.address;

                    Debug.Assert(PtcProfiler.IsAddressInStaticCodeRange(address));

                    TranslatedFunction func = Translator.Translate(memory, jumpTable, countTable, address, item.funcProfile.Mode, item.funcProfile.HighCq);

                    bool isAddressUnique = funcs.TryAdd(address, func);

                    Debug.Assert(isAddressUnique, $"The address 0x{address:X16} is not unique.");

                    if (func.HighCq)
                    {
                        jumpTable.RegisterFunction(address, func);
                    }

                    Interlocked.Increment(ref _translateCount);

                    if (State != PtcState.Enabled)
                    {
                        break;
                    }
                }

                Translator.DisposePools();
            }

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                Thread thread = new Thread(TranslateFuncs);
                thread.IsBackground = true;

                threads.Add(thread);
            }

            threads.ForEach((thread) => thread.Start());
            threads.ForEach((thread) => thread.Join());

            threads.Clear();

            progressReportEvent.Set();
            progressReportThread.Join();

            PtcStateChanged?.Invoke(PtcLoadingState.Loaded, _translateCount, _translateTotalCount);

            Logger.Info?.Print(LogClass.Ptc, $"{_translateCount} of {_translateTotalCount} functions translated | Thread count: {degreeOfParallelism}");

            PtcJumpTable.Initialize(jumpTable);

            PtcJumpTable.ReadJumpTable(jumpTable);
            PtcJumpTable.ReadDynamicTable(jumpTable);

            Thread preSaveThread = new Thread(PreSave);
            preSaveThread.IsBackground = true;
            preSaveThread.Start();
        }

        private static void ReportProgress(object state)
        {
            const int refreshRate = 50; // ms.

            AutoResetEvent endEvent = (AutoResetEvent)state;

            int count = 0;

            do
            {
                int newCount = _translateCount;

                if (count != newCount)
                {
                    PtcStateChanged?.Invoke(PtcLoadingState.Loading, newCount, _translateTotalCount);
                    count = newCount;
                }
            }
            while (!endEvent.WaitOne(refreshRate));
        }

        internal static Hash128 ComputeHash(IMemoryManager memory, ulong address, ulong guestSize)
        {
            return XXHash128.ComputeHash(memory.GetSpan(address, checked((int)(guestSize))));
        }

        internal static void WriteInfoCodeRelocUnwindInfo(ulong address, ulong guestSize, Hash128 hash, bool highCq, PtcInfo ptcInfo)
        {
            lock (_lock)
            {
                InfoEntry infoEntry = new InfoEntry();

                infoEntry.Address = address;
                infoEntry.GuestSize = guestSize;
                infoEntry.Hash = hash;
                infoEntry.HighCq = highCq;
                infoEntry.Stubbed = false;
                infoEntry.CodeLength = ptcInfo.Code.Length;
                infoEntry.RelocEntriesCount = ptcInfo.RelocEntriesCount;

                SerializeStructure(_infosStream, infoEntry);

                WriteCode(ptcInfo.Code.AsSpan());

                // WriteReloc.
                ptcInfo.RelocStream.WriteTo(_relocsStream);

                // WriteUnwindInfo.
                ptcInfo.UnwindInfoStream.WriteTo(_unwindInfosStream);
            }
        }

        private static void WriteCode(ReadOnlySpan<byte> code)
        {
            _codesList.Add(code.ToArray());
        }

        internal static bool GetEndianness()
        {
            return BitConverter.IsLittleEndian;
        }

        private static ulong GetFeatureInfo()
        {
            return (ulong)HardwareCapabilities.FeatureInfoEdx << 32 | (uint)HardwareCapabilities.FeatureInfoEcx;
        }

        private static uint GetOSPlatform()
        {
            uint osPlatform = 0u;

            osPlatform |= (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? 1u : 0u) << 0;
            osPlatform |= (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)   ? 1u : 0u) << 1;
            osPlatform |= (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)     ? 1u : 0u) << 2;
            osPlatform |= (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 1u : 0u) << 3;

            return osPlatform;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 49*/)]
        private struct OuterHeader
        {
            public ulong Magic;

            public uint CacheFileVersion;

            public bool Endianness;
            public ulong FeatureInfo;
            public uint OSPlatform;

            public long UncompressedStreamSize;

            public Hash128 HeaderHash;

            public void SetHeaderHash()
            {
                Span<OuterHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                HeaderHash = XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader).Slice(0, Unsafe.SizeOf<OuterHeader>() - Unsafe.SizeOf<Hash128>()));
            }

            public bool IsHeaderValid()
            {
                Span<OuterHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                return XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader).Slice(0, Unsafe.SizeOf<OuterHeader>() - Unsafe.SizeOf<Hash128>())) == HeaderHash;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 128*/)]
        private struct InnerHeader
        {
            public ulong Magic;

            public int InfosLength;
            public long CodesLength;
            public int RelocsLength;
            public int UnwindInfosLength;
            public int PtcJumpTableLength;

            public Hash128 InfosHash;
            public Hash128 CodesHash;
            public Hash128 RelocsHash;
            public Hash128 UnwindInfosHash;
            public Hash128 PtcJumpTableHash;

            public Hash128 HeaderHash;

            public void SetHeaderHash()
            {
                Span<InnerHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                HeaderHash = XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader).Slice(0, Unsafe.SizeOf<InnerHeader>() - Unsafe.SizeOf<Hash128>()));
            }

            public bool IsHeaderValid()
            {
                Span<InnerHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                return XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader).Slice(0, Unsafe.SizeOf<InnerHeader>() - Unsafe.SizeOf<Hash128>())) == HeaderHash;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 42*/)]
        private struct InfoEntry
        {
            public ulong Address;
            public ulong GuestSize;
            public Hash128 Hash;
            public bool HighCq;
            public bool Stubbed;
            public int CodeLength;
            public int RelocEntriesCount;
        }

        private static void Enable()
        {
            State = PtcState.Enabled;
        }

        public static void Continue()
        {
            if (State == PtcState.Enabled)
            {
                State = PtcState.Continuing;
            }
        }

        public static void Close()
        {
            if (State == PtcState.Enabled ||
                State == PtcState.Continuing)
            {
                State = PtcState.Closing;
            }
        }

        internal static void Disable()
        {
            State = PtcState.Disabled;
        }

        private static void Wait()
        {
            _waitEvent.WaitOne();
        }

        public static void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                Wait();
                _waitEvent.Dispose();

                DisposeCarriers();
            }
        }
    }
}
