using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.Unwinding;
using ARMeilleure.CodeGen.X86;
using ARMeilleure.Memory;
using ARMeilleure.Translation.Cache;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace ARMeilleure.Translation.PTC
{
    public static class Ptc
    {
        private const string HeaderMagic = "PTChd";

        private const int InternalVersion = 1971; //! To be incremented manually for each change to the ARMeilleure project.

        private const string ActualDir = "0";
        private const string BackupDir = "1";

        private const string TitleIdTextDefault = "0000000000000000";
        private const string DisplayVersionDefault = "0";

        internal const int PageTablePointerIndex = -1; // Must be a negative value.
        internal const int JumpPointerIndex = -2; // Must be a negative value.
        internal const int DynamicPointerIndex = -3; // Must be a negative value.

        private const byte FillingByte = 0x00;
        private const CompressionLevel SaveCompressionLevel = CompressionLevel.Fastest;

        private static readonly MemoryStream _infosStream;
        private static readonly MemoryStream _codesStream;
        private static readonly MemoryStream _relocsStream;
        private static readonly MemoryStream _unwindInfosStream;

        private static readonly BinaryWriter _infosWriter;

        private static readonly ManualResetEvent _waitEvent;

        private static readonly AutoResetEvent _loggerEvent;

        private static readonly object _lock;

        private static bool _disposed;

        private static volatile int _translateCount;

        internal static PtcJumpTable PtcJumpTable { get; private set; }

        internal static string TitleIdText { get; private set; }
        internal static string DisplayVersion { get; private set; }

        internal static string CachePathActual { get; private set; }
        internal static string CachePathBackup { get; private set; }

        internal static PtcState State { get; private set; }

        static Ptc()
        {
            _infosStream = new MemoryStream();
            _codesStream = new MemoryStream();
            _relocsStream = new MemoryStream();
            _unwindInfosStream = new MemoryStream();

            _infosWriter = new BinaryWriter(_infosStream, EncodingCache.UTF8NoBOM, true);

            _waitEvent = new ManualResetEvent(true);

            _loggerEvent = new AutoResetEvent(false);

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
            ClearMemoryStreams();
            PtcJumpTable.Clear();

            PtcProfiler.Wait();
            PtcProfiler.ClearEntries();

            if (String.IsNullOrEmpty(titleIdText) || titleIdText == TitleIdTextDefault)
            {
                TitleIdText = TitleIdTextDefault;
                DisplayVersion = DisplayVersionDefault;

                CachePathActual = string.Empty;
                CachePathBackup = string.Empty;

                Disable();

                return;
            }

            Logger.Info?.Print(LogClass.Ptc, $"Initializing Profiled Persistent Translation Cache (enabled: {enabled}).");

            TitleIdText = titleIdText;
            DisplayVersion = !String.IsNullOrEmpty(displayVersion) ? displayVersion : DisplayVersionDefault;

            if (enabled)
            {
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

                Enable();

                PreLoad();
                PtcProfiler.PreLoad();
            }
            else
            {
                CachePathActual = string.Empty;
                CachePathBackup = string.Empty;

                Disable();
            }
        }

        internal static void ClearMemoryStreams()
        {
            _infosStream.SetLength(0L);
            _codesStream.SetLength(0L);
            _relocsStream.SetLength(0L);
            _unwindInfosStream.SetLength(0L);
        }

        private static void PreLoad()
        {
            string fileNameActual = String.Concat(CachePathActual, ".cache");
            string fileNameBackup = String.Concat(CachePathBackup, ".cache");

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

        private static bool Load(string fileName, bool isBackup)
        {
            using (FileStream compressedStream = new FileStream(fileName, FileMode.Open))
            using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true))
            using (MemoryStream stream = new MemoryStream())
            using (MD5 md5 = MD5.Create())
            {
                int hashSize = md5.HashSize / 8;

                try
                {
                    deflateStream.CopyTo(stream);
                }
                catch
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                stream.Seek(0L, SeekOrigin.Begin);

                byte[] currentHash = new byte[hashSize];
                stream.Read(currentHash, 0, hashSize);

                byte[] expectedHash = md5.ComputeHash(stream);

                if (!CompareHash(currentHash, expectedHash))
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                stream.Seek((long)hashSize, SeekOrigin.Begin);

                Header header = ReadHeader(stream);

                if (header.Magic != HeaderMagic)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (header.CacheFileVersion != InternalVersion)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (header.FeatureInfo != GetFeatureInfo())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (header.OSPlatform != GetOSPlatform())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (header.InfosLen % InfoEntry.Stride != 0)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                byte[] infosBuf = new byte[header.InfosLen];
                byte[] codesBuf = new byte[header.CodesLen];
                byte[] relocsBuf = new byte[header.RelocsLen];
                byte[] unwindInfosBuf = new byte[header.UnwindInfosLen];

                stream.Read(infosBuf, 0, header.InfosLen);
                stream.Read(codesBuf, 0, header.CodesLen);
                stream.Read(relocsBuf, 0, header.RelocsLen);
                stream.Read(unwindInfosBuf, 0, header.UnwindInfosLen);

                try
                {
                    PtcJumpTable = PtcJumpTable.Deserialize(stream);
                }
                catch
                {
                    PtcJumpTable = new PtcJumpTable();

                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                _infosStream.Write(infosBuf, 0, header.InfosLen);
                _codesStream.Write(codesBuf, 0, header.CodesLen);
                _relocsStream.Write(relocsBuf, 0, header.RelocsLen);
                _unwindInfosStream.Write(unwindInfosBuf, 0, header.UnwindInfosLen);
            }

            long fileSize = new FileInfo(fileName).Length;

            Logger.Info?.Print(LogClass.Ptc, $"{(isBackup ? "Loaded Backup Translation Cache" : "Loaded Translation Cache")} (size: {fileSize} bytes, translated functions: {GetInfosEntriesCount()}).");

            return true;
        }

        private static bool CompareHash(ReadOnlySpan<byte> currentHash, ReadOnlySpan<byte> expectedHash)
        {
            return currentHash.SequenceEqual(expectedHash);
        }

        private static Header ReadHeader(MemoryStream stream)
        {
            using (BinaryReader headerReader = new BinaryReader(stream, EncodingCache.UTF8NoBOM, true))
            {
                Header header = new Header();

                header.Magic = headerReader.ReadString();

                header.CacheFileVersion = headerReader.ReadUInt32();
                header.FeatureInfo = headerReader.ReadUInt64();
                header.OSPlatform = headerReader.ReadUInt32();

                header.InfosLen = headerReader.ReadInt32();
                header.CodesLen = headerReader.ReadInt32();
                header.RelocsLen = headerReader.ReadInt32();
                header.UnwindInfosLen = headerReader.ReadInt32();

                return header;
            }
        }

        private static void InvalidateCompressedStream(FileStream compressedStream)
        {
            compressedStream.SetLength(0L);
        }

        private static void PreSave(object state)
        {
            _waitEvent.Reset();

            string fileNameActual = String.Concat(CachePathActual, ".cache");
            string fileNameBackup = String.Concat(CachePathBackup, ".cache");

            FileInfo fileInfoActual = new FileInfo(fileNameActual);

            if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
            {
                File.Copy(fileNameActual, fileNameBackup, true);
            }

            Save(fileNameActual);

            _waitEvent.Set();
        }

        private static void Save(string fileName)
        {
            int translatedFuncsCount;

            using (MemoryStream stream = new MemoryStream())
            using (MD5 md5 = MD5.Create())
            {
                int hashSize = md5.HashSize / 8;

                stream.Seek((long)hashSize, SeekOrigin.Begin);

                WriteHeader(stream);

                _infosStream.WriteTo(stream);
                _codesStream.WriteTo(stream);
                _relocsStream.WriteTo(stream);
                _unwindInfosStream.WriteTo(stream);

                PtcJumpTable.Serialize(stream, PtcJumpTable);

                translatedFuncsCount = GetInfosEntriesCount();

                ClearMemoryStreams();
                PtcJumpTable.Clear();

                stream.Seek((long)hashSize, SeekOrigin.Begin);
                byte[] hash = md5.ComputeHash(stream);

                stream.Seek(0L, SeekOrigin.Begin);
                stream.Write(hash, 0, hashSize);

                using (FileStream compressedStream = new FileStream(fileName, FileMode.OpenOrCreate))
                using (DeflateStream deflateStream = new DeflateStream(compressedStream, SaveCompressionLevel, true))
                {
                    try
                    {
                        stream.WriteTo(deflateStream);
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

            long fileSize = new FileInfo(fileName).Length;

            Logger.Info?.Print(LogClass.Ptc, $"Saved Translation Cache (size: {fileSize} bytes, translated functions: {translatedFuncsCount}).");
        }

        private static void WriteHeader(MemoryStream stream)
        {
            using (BinaryWriter headerWriter = new BinaryWriter(stream, EncodingCache.UTF8NoBOM, true))
            {
                headerWriter.Write((string)HeaderMagic); // Header.Magic

                headerWriter.Write((uint)InternalVersion); // Header.CacheFileVersion
                headerWriter.Write((ulong)GetFeatureInfo()); // Header.FeatureInfo
                headerWriter.Write((uint)GetOSPlatform()); // Header.OSPlatform

                headerWriter.Write((int)_infosStream.Length); // Header.InfosLen
                headerWriter.Write((int)_codesStream.Length); // Header.CodesLen
                headerWriter.Write((int)_relocsStream.Length); // Header.RelocsLen
                headerWriter.Write((int)_unwindInfosStream.Length); // Header.UnwindInfosLen
            }
        }

        internal static void LoadTranslations(ConcurrentDictionary<ulong, TranslatedFunction> funcs, IMemoryManager memory, JumpTable jumpTable)
        {
            if ((int)_infosStream.Length == 0 ||
                (int)_codesStream.Length == 0 ||
                (int)_relocsStream.Length == 0 ||
                (int)_unwindInfosStream.Length == 0)
            {
                return;
            }

            Debug.Assert(funcs.Count == 0);

            _infosStream.Seek(0L, SeekOrigin.Begin);
            _codesStream.Seek(0L, SeekOrigin.Begin);
            _relocsStream.Seek(0L, SeekOrigin.Begin);
            _unwindInfosStream.Seek(0L, SeekOrigin.Begin);

            using (BinaryReader infosReader = new BinaryReader(_infosStream, EncodingCache.UTF8NoBOM, true))
            using (BinaryReader codesReader = new BinaryReader(_codesStream, EncodingCache.UTF8NoBOM, true))
            using (BinaryReader relocsReader = new BinaryReader(_relocsStream, EncodingCache.UTF8NoBOM, true))
            using (BinaryReader unwindInfosReader = new BinaryReader(_unwindInfosStream, EncodingCache.UTF8NoBOM, true))
            {
                for (int i = 0; i < GetInfosEntriesCount(); i++)
                {
                    InfoEntry infoEntry = ReadInfo(infosReader);

                    if (infoEntry.Stubbed)
                    {
                        SkipCode(infoEntry.CodeLen);
                        SkipReloc(infoEntry.RelocEntriesCount);
                        SkipUnwindInfo(unwindInfosReader);
                    }
                    else if (infoEntry.HighCq || !PtcProfiler.ProfiledFuncs.TryGetValue(infoEntry.Address, out var value) || !value.highCq)
                    {
                        byte[] code = ReadCode(codesReader, infoEntry.CodeLen);

                        if (infoEntry.RelocEntriesCount != 0)
                        {
                            RelocEntry[] relocEntries = GetRelocEntries(relocsReader, infoEntry.RelocEntriesCount);

                            PatchCode(code, relocEntries, memory.PageTablePointer, jumpTable);
                        }

                        UnwindInfo unwindInfo = ReadUnwindInfo(unwindInfosReader);

                        TranslatedFunction func = FastTranslate(code, infoEntry.GuestSize, unwindInfo, infoEntry.HighCq);

                        bool isAddressUnique = funcs.TryAdd(infoEntry.Address, func);

                        Debug.Assert(isAddressUnique, $"The address 0x{infoEntry.Address:X16} is not unique.");
                    }
                    else
                    {
                        infoEntry.Stubbed = true;
                        UpdateInfo(infoEntry);

                        StubCode(infoEntry.CodeLen);
                        StubReloc(infoEntry.RelocEntriesCount);
                        StubUnwindInfo(unwindInfosReader);
                    }
                }
            }

            if (_infosStream.Position < _infosStream.Length ||
                _codesStream.Position < _codesStream.Length ||
                _relocsStream.Position < _relocsStream.Length ||
                _unwindInfosStream.Position < _unwindInfosStream.Length)
            {
                throw new Exception("Could not reach the end of one or more memory streams.");
            }

            jumpTable.Initialize(PtcJumpTable, funcs);

            PtcJumpTable.WriteJumpTable(jumpTable, funcs);
            PtcJumpTable.WriteDynamicTable(jumpTable);

            Logger.Info?.Print(LogClass.Ptc, $"{funcs.Count} translated functions loaded");
        }

        private static int GetInfosEntriesCount()
        {
            return (int)_infosStream.Length / InfoEntry.Stride;
        }

        private static InfoEntry ReadInfo(BinaryReader infosReader)
        {
            InfoEntry infoEntry = new InfoEntry();

            infoEntry.Address = infosReader.ReadUInt64();
            infoEntry.GuestSize = infosReader.ReadUInt64();
            infoEntry.HighCq = infosReader.ReadBoolean();
            infoEntry.Stubbed = infosReader.ReadBoolean();
            infoEntry.CodeLen = infosReader.ReadInt32();
            infoEntry.RelocEntriesCount = infosReader.ReadInt32();

            return infoEntry;
        }

        private static void SkipCode(int codeLen)
        {
            _codesStream.Seek(codeLen, SeekOrigin.Current);
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

        private static byte[] ReadCode(BinaryReader codesReader, int codeLen)
        {
            byte[] codeBuf = new byte[codeLen];

            codesReader.Read(codeBuf, 0, codeLen);

            return codeBuf;
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

        private static void PatchCode(Span<byte> code, RelocEntry[] relocEntries, IntPtr pageTablePointer, JumpTable jumpTable)
        {
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

        private static TranslatedFunction FastTranslate(byte[] code, ulong guestSize, UnwindInfo unwindInfo, bool highCq)
        {
            CompiledFunction cFunc = new CompiledFunction(code, unwindInfo);

            IntPtr codePtr = JitCache.Map(cFunc);

            GuestFunction gFunc = Marshal.GetDelegateForFunctionPointer<GuestFunction>(codePtr);

            TranslatedFunction tFunc = new TranslatedFunction(gFunc, guestSize, highCq);

            return tFunc;
        }

        private static void UpdateInfo(InfoEntry infoEntry)
        {
            _infosStream.Seek(-InfoEntry.Stride, SeekOrigin.Current);

            // WriteInfo.
            _infosWriter.Write((ulong)infoEntry.Address);
            _infosWriter.Write((ulong)infoEntry.GuestSize);
            _infosWriter.Write((bool)infoEntry.HighCq);
            _infosWriter.Write((bool)infoEntry.Stubbed);
            _infosWriter.Write((int)infoEntry.CodeLen);
            _infosWriter.Write((int)infoEntry.RelocEntriesCount);
        }

        private static void StubCode(int codeLen)
        {
            for (int i = 0; i < codeLen; i++)
            {
                _codesStream.WriteByte(FillingByte);
            }
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

        internal static void MakeAndSaveTranslations(ConcurrentDictionary<ulong, TranslatedFunction> funcs, IMemoryManager memory, JumpTable jumpTable)
        {
            var profiledFuncsToTranslate = PtcProfiler.GetProfiledFuncsToTranslate(funcs);

            if (profiledFuncsToTranslate.Count == 0)
            {
                return;
            }

            _translateCount = 0;

            ThreadPool.QueueUserWorkItem(TranslationLogger, profiledFuncsToTranslate.Count);

            void TranslateFuncs()
            {
                while (profiledFuncsToTranslate.TryDequeue(out var item))
                {
                    ulong address = item.address;

                    Debug.Assert(PtcProfiler.IsAddressInStaticCodeRange(address));

                    TranslatedFunction func = Translator.Translate(memory, jumpTable, address, item.mode, item.highCq);

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
            }

            int maxDegreeOfParallelism = (Environment.ProcessorCount * 3) / 4;

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                Thread thread = new Thread(TranslateFuncs);
                thread.IsBackground = true;

                threads.Add(thread);
            }

            threads.ForEach((thread) => thread.Start());
            threads.ForEach((thread) => thread.Join());

            threads.Clear();

            Translator.ResetPools();

            _loggerEvent.Set();

            PtcJumpTable.Initialize(jumpTable);

            PtcJumpTable.ReadJumpTable(jumpTable);
            PtcJumpTable.ReadDynamicTable(jumpTable);

            ThreadPool.QueueUserWorkItem(PreSave);
        }

        private static void TranslationLogger(object state)
        {
            const int refreshRate = 1; // Seconds.

            int profiledFuncsToTranslateCount = (int)state;

            do
            {
                Logger.Info?.Print(LogClass.Ptc, $"{_translateCount} of {profiledFuncsToTranslateCount} functions translated");
            }
            while (!_loggerEvent.WaitOne(refreshRate * 1000));

            Logger.Info?.Print(LogClass.Ptc, $"{_translateCount} of {profiledFuncsToTranslateCount} functions translated");
        }

        internal static void WriteInfoCodeRelocUnwindInfo(ulong address, ulong guestSize, bool highCq, PtcInfo ptcInfo)
        {
            lock (_lock)
            {
                // WriteInfo.
                _infosWriter.Write((ulong)address); // InfoEntry.Address
                _infosWriter.Write((ulong)guestSize); // InfoEntry.GuestSize
                _infosWriter.Write((bool)highCq); // InfoEntry.HighCq
                _infosWriter.Write((bool)false); // InfoEntry.Stubbed
                _infosWriter.Write((int)ptcInfo.CodeStream.Length); // InfoEntry.CodeLen
                _infosWriter.Write((int)ptcInfo.RelocEntriesCount); // InfoEntry.RelocEntriesCount

                // WriteCode.
                ptcInfo.CodeStream.WriteTo(_codesStream);

                // WriteReloc.
                ptcInfo.RelocStream.WriteTo(_relocsStream);

                // WriteUnwindInfo.
                ptcInfo.UnwindInfoStream.WriteTo(_unwindInfosStream);
            }
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

        private struct Header
        {
            public string Magic;

            public uint CacheFileVersion;
            public ulong FeatureInfo;
            public uint OSPlatform;

            public int InfosLen;
            public int CodesLen;
            public int RelocsLen;
            public int UnwindInfosLen;
        }

        private struct InfoEntry
        {
            public const int Stride = 26; // Bytes.

            public ulong Address;
            public ulong GuestSize;
            public bool HighCq;
            public bool Stubbed;
            public int CodeLen;
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

                _infosWriter.Dispose();

                _infosStream.Dispose();
                _codesStream.Dispose();
                _relocsStream.Dispose();
                _unwindInfosStream.Dispose();
            }
        }
    }
}