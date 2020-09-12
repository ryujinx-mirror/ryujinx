using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.Unwinding;
using ARMeilleure.Memory;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace ARMeilleure.Translation.PTC
{
    public static class Ptc
    {
        private const string HeaderMagic = "PTChd";

        private const int InternalVersion = 1535; //! To be incremented manually for each change to the ARMeilleure project.

        private const string ActualDir = "0";
        private const string BackupDir = "1";

        private const string TitleIdTextDefault = "0000000000000000";
        private const string DisplayVersionDefault = "0";

        internal const int PageTablePointerIndex = -1; // Must be a negative value.
        internal const int JumpPointerIndex = -2; // Must be a negative value.
        internal const int DynamicPointerIndex = -3; // Must be a negative value.

        private const CompressionLevel SaveCompressionLevel = CompressionLevel.Fastest;

        private static readonly MemoryStream _infosStream;
        private static readonly MemoryStream _codesStream;
        private static readonly MemoryStream _relocsStream;
        private static readonly MemoryStream _unwindInfosStream;

        private static readonly BinaryWriter _infosWriter;

        private static readonly BinaryFormatter _binaryFormatter;

        private static readonly ManualResetEvent _waitEvent;

        private static readonly AutoResetEvent _loggerEvent;

        private static readonly object _lock;

        private static bool _disposed;

        private static volatile int _translateCount;
        private static volatile int _rejitCount;

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

            _binaryFormatter = new BinaryFormatter();

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

            PtcProfiler.Stop();
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
                if (!Load(fileNameActual))
                {
                    if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
                    {
                        Load(fileNameBackup);
                    }
                }
            }
            else if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
            {
                Load(fileNameBackup);
            }
        }

        private static bool Load(string fileName)
        {
            using (FileStream compressedStream = new FileStream(fileName, FileMode.Open))
            using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true))
            using (MemoryStream stream = new MemoryStream())
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                int hashSize = md5.HashSize / 8;

                deflateStream.CopyTo(stream);

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
                    PtcJumpTable = (PtcJumpTable)_binaryFormatter.Deserialize(stream);
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

                return true;
            }
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

                header.CacheFileVersion = headerReader.ReadInt32();
                header.FeatureInfo = headerReader.ReadUInt64();

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
            using (MemoryStream stream = new MemoryStream())
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                int hashSize = md5.HashSize / 8;

                stream.Seek((long)hashSize, SeekOrigin.Begin);

                WriteHeader(stream);

                _infosStream.WriteTo(stream);
                _codesStream.WriteTo(stream);
                _relocsStream.WriteTo(stream);
                _unwindInfosStream.WriteTo(stream);

                _binaryFormatter.Serialize(stream, PtcJumpTable);

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
        }

        private static void WriteHeader(MemoryStream stream)
        {
            using (BinaryWriter headerWriter = new BinaryWriter(stream, EncodingCache.UTF8NoBOM, true))
            {
                headerWriter.Write((string)HeaderMagic); // Header.Magic

                headerWriter.Write((int)InternalVersion); // Header.CacheFileVersion
                headerWriter.Write((ulong)GetFeatureInfo()); // Header.FeatureInfo

                headerWriter.Write((int)_infosStream.Length); // Header.InfosLen
                headerWriter.Write((int)_codesStream.Length); // Header.CodesLen
                headerWriter.Write((int)_relocsStream.Length); // Header.RelocsLen
                headerWriter.Write((int)_unwindInfosStream.Length); // Header.UnwindInfosLen
            }
        }

        internal static void LoadTranslations(ConcurrentDictionary<ulong, TranslatedFunction> funcs, IntPtr pageTablePointer, JumpTable jumpTable)
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
                int infosEntriesCount = (int)_infosStream.Length / InfoEntry.Stride;

                for (int i = 0; i < infosEntriesCount; i++)
                {
                    InfoEntry infoEntry = ReadInfo(infosReader);

                    byte[] code = ReadCode(codesReader, infoEntry.CodeLen);

                    if (infoEntry.RelocEntriesCount != 0)
                    {
                        RelocEntry[] relocEntries = GetRelocEntries(relocsReader, infoEntry.RelocEntriesCount);

                        PatchCode(code, relocEntries, pageTablePointer, jumpTable);
                    }

                    UnwindInfo unwindInfo = ReadUnwindInfo(unwindInfosReader);

                    TranslatedFunction func = FastTranslate(code, unwindInfo, infoEntry.HighCq);

                    funcs.AddOrUpdate((ulong)infoEntry.Address, func, (key, oldFunc) => func.HighCq && !oldFunc.HighCq ? func : oldFunc);
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
        }

        private static InfoEntry ReadInfo(BinaryReader infosReader)
        {
            InfoEntry infoEntry = new InfoEntry();

            infoEntry.Address = infosReader.ReadInt64();
            infoEntry.HighCq = infosReader.ReadBoolean();
            infoEntry.CodeLen = infosReader.ReadInt32();
            infoEntry.RelocEntriesCount = infosReader.ReadInt32();

            return infoEntry;
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

        private static TranslatedFunction FastTranslate(byte[] code, UnwindInfo unwindInfo, bool highCq)
        {
            CompiledFunction cFunc = new CompiledFunction(code, unwindInfo);

            IntPtr codePtr = JitCache.Map(cFunc);

            GuestFunction gFunc = Marshal.GetDelegateForFunctionPointer<GuestFunction>(codePtr);

            TranslatedFunction tFunc = new TranslatedFunction(gFunc, highCq);

            return tFunc;
        }

        internal static void MakeAndSaveTranslations(ConcurrentDictionary<ulong, TranslatedFunction> funcs, IMemoryManager memory, JumpTable jumpTable)
        {
            if (PtcProfiler.ProfiledFuncs.Count == 0)
            {
                return;
            }

            _translateCount = 0;
            _rejitCount = 0;

            ThreadPool.QueueUserWorkItem(TranslationLogger, (funcs.Count, PtcProfiler.ProfiledFuncs.Count));

            int maxDegreeOfParallelism = (Environment.ProcessorCount * 3) / 4;

            Parallel.ForEach(PtcProfiler.ProfiledFuncs, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (item, state) =>
            {
                ulong address = item.Key;

                Debug.Assert(PtcProfiler.IsAddressInStaticCodeRange(address));

                if (!funcs.ContainsKey(address))
                {
                    TranslatedFunction func = Translator.Translate(memory, jumpTable, address, item.Value.mode, item.Value.highCq);

                    funcs.TryAdd(address, func);

                    if (func.HighCq)
                    {
                        jumpTable.RegisterFunction(address, func);
                    }

                    Interlocked.Increment(ref _translateCount);
                }
                else if (item.Value.highCq && !funcs[address].HighCq)
                {
                    TranslatedFunction func = Translator.Translate(memory, jumpTable, address, item.Value.mode, highCq: true);

                    funcs[address] = func;

                    jumpTable.RegisterFunction(address, func);

                    Interlocked.Increment(ref _rejitCount);
                }

                if (State != PtcState.Enabled)
                {
                    state.Stop();
                }
            });

            _loggerEvent.Set();

            if (_translateCount != 0 || _rejitCount != 0)
            {
                PtcJumpTable.Initialize(jumpTable);

                PtcJumpTable.ReadJumpTable(jumpTable);
                PtcJumpTable.ReadDynamicTable(jumpTable);

                ThreadPool.QueueUserWorkItem(PreSave);
            }
        }

        private static void TranslationLogger(object state)
        {
            const int refreshRate = 1; // Seconds.

            (int funcsCount, int ProfiledFuncsCount) = ((int, int))state;

            do
            {
                Logger.Info?.Print(LogClass.Ptc, $"{funcsCount + _translateCount} of {ProfiledFuncsCount} functions to translate - {_rejitCount} functions rejited");
            }
            while (!_loggerEvent.WaitOne(refreshRate * 1000));

            Logger.Info?.Print(LogClass.Ptc, $"{funcsCount + _translateCount} of {ProfiledFuncsCount} functions to translate - {_rejitCount} functions rejited");
        }

        internal static void WriteInfoCodeReloc(long address, bool highCq, PtcInfo ptcInfo)
        {
            lock (_lock)
            {
                // WriteInfo.
                _infosWriter.Write((long)address); // InfoEntry.Address
                _infosWriter.Write((bool)highCq); // InfoEntry.HighCq
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
            ulong featureInfo = 0ul;

            featureInfo |= (Sse3.IsSupported      ? 1ul : 0ul) << 0;
            featureInfo |= (Pclmulqdq.IsSupported ? 1ul : 0ul) << 1;
            featureInfo |= (Ssse3.IsSupported     ? 1ul : 0ul) << 9;
            featureInfo |= (Fma.IsSupported       ? 1ul : 0ul) << 12;
            featureInfo |= (Sse41.IsSupported     ? 1ul : 0ul) << 19;
            featureInfo |= (Sse42.IsSupported     ? 1ul : 0ul) << 20;
            featureInfo |= (Popcnt.IsSupported    ? 1ul : 0ul) << 23;
            featureInfo |= (Aes.IsSupported       ? 1ul : 0ul) << 25;
            featureInfo |= (Avx.IsSupported       ? 1ul : 0ul) << 28;
            featureInfo |= (Sse.IsSupported       ? 1ul : 0ul) << 57;
            featureInfo |= (Sse2.IsSupported      ? 1ul : 0ul) << 58;

            return featureInfo;
        }

        private struct Header
        {
            public string Magic;

            public int CacheFileVersion;
            public ulong FeatureInfo;

            public int InfosLen;
            public int CodesLen;
            public int RelocsLen;
            public int UnwindInfosLen;
        }

        private struct InfoEntry
        {
            public const int Stride = 17; // Bytes.

            public long Address;
            public bool HighCq;
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
