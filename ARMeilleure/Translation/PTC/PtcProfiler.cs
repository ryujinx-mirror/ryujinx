using ARMeilleure.State;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using static ARMeilleure.Translation.PTC.PtcFormatter;

namespace ARMeilleure.Translation.PTC
{
    class PtcProfiler
    {
        private const string OuterHeaderMagicString = "Pohd\0\0\0\0";

        private const uint InternalVersion = 1866; //! Not to be incremented manually for each change to the ARMeilleure project.

        private const int SaveInterval = 30; // Seconds.

        private const CompressionLevel SaveCompressionLevel = CompressionLevel.Fastest;

        private readonly Ptc _ptc;

        private readonly System.Timers.Timer _timer;

        private readonly ulong _outerHeaderMagic;

        private readonly ManualResetEvent _waitEvent;

        private readonly object _lock;

        private bool _disposed;

        private Hash128 _lastHash;

        public Dictionary<ulong, FuncProfile> ProfiledFuncs { get; private set; }

        public bool Enabled { get; private set; }

        public ulong StaticCodeStart { get; set; }
        public ulong StaticCodeSize  { get; set; }

        public PtcProfiler(Ptc ptc)
        {
            _ptc = ptc;

            _timer = new System.Timers.Timer((double)SaveInterval * 1000d);
            _timer.Elapsed += PreSave;

            _outerHeaderMagic = BinaryPrimitives.ReadUInt64LittleEndian(EncodingCache.UTF8NoBOM.GetBytes(OuterHeaderMagicString).AsSpan());

            _waitEvent = new ManualResetEvent(true);

            _lock = new object();

            _disposed = false;

            ProfiledFuncs = new Dictionary<ulong, FuncProfile>();

            Enabled = false;
        }

        public void AddEntry(ulong address, ExecutionMode mode, bool highCq)
        {
            if (IsAddressInStaticCodeRange(address))
            {
                Debug.Assert(!highCq);

                lock (_lock)
                {
                    ProfiledFuncs.TryAdd(address, new FuncProfile(mode, highCq: false));
                }
            }
        }

        public void UpdateEntry(ulong address, ExecutionMode mode, bool highCq)
        {
            if (IsAddressInStaticCodeRange(address))
            {
                Debug.Assert(highCq);

                lock (_lock)
                {
                    Debug.Assert(ProfiledFuncs.ContainsKey(address));

                    ProfiledFuncs[address] = new FuncProfile(mode, highCq: true);
                }
            }
        }

        public bool IsAddressInStaticCodeRange(ulong address)
        {
            return address >= StaticCodeStart && address < StaticCodeStart + StaticCodeSize;
        }

        public ConcurrentQueue<(ulong address, FuncProfile funcProfile)> GetProfiledFuncsToTranslate(TranslatorCache<TranslatedFunction> funcs)
        {
            var profiledFuncsToTranslate = new ConcurrentQueue<(ulong address, FuncProfile funcProfile)>();

            foreach (var profiledFunc in ProfiledFuncs)
            {
                if (!funcs.ContainsKey(profiledFunc.Key))
                {
                    profiledFuncsToTranslate.Enqueue((profiledFunc.Key, profiledFunc.Value));
                }
            }

            return profiledFuncsToTranslate;
        }

        public void ClearEntries()
        {
            ProfiledFuncs.Clear();
            ProfiledFuncs.TrimExcess();
        }

        public void PreLoad()
        {
            _lastHash = default;

            string fileNameActual = string.Concat(_ptc.CachePathActual, ".info");
            string fileNameBackup = string.Concat(_ptc.CachePathBackup, ".info");

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

        private bool Load(string fileName, bool isBackup)
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

                if (outerHeader.InfoFileVersion != InternalVersion)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.Endianness != Ptc.GetEndianness())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    Debug.Assert(stream.Seek(0L, SeekOrigin.Begin) == 0L && stream.Length == 0L);

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

                    Hash128 expectedHash = DeserializeStructure<Hash128>(stream);

                    Hash128 actualHash = XXHash128.ComputeHash(GetReadOnlySpan(stream));

                    if (actualHash != expectedHash)
                    {
                        InvalidateCompressedStream(compressedStream);

                        return false;
                    }

                    ProfiledFuncs = Deserialize(stream);

                    Debug.Assert(stream.Position == stream.Length);

                    _lastHash = actualHash;
                }
            }

            long fileSize = new FileInfo(fileName).Length;

            Logger.Info?.Print(LogClass.Ptc, $"{(isBackup ? "Loaded Backup Profiling Info" : "Loaded Profiling Info")} (size: {fileSize} bytes, profiled functions: {ProfiledFuncs.Count}).");

            return true;
        }

        private static Dictionary<ulong, FuncProfile> Deserialize(Stream stream)
        {
            return DeserializeDictionary<ulong, FuncProfile>(stream, (stream) => DeserializeStructure<FuncProfile>(stream));
        }

        private ReadOnlySpan<byte> GetReadOnlySpan(MemoryStream memoryStream)
        {
            return new(memoryStream.GetBuffer(), (int)memoryStream.Position, (int)memoryStream.Length - (int)memoryStream.Position);
        }

        private void InvalidateCompressedStream(FileStream compressedStream)
        {
            compressedStream.SetLength(0L);
        }

        private void PreSave(object source, System.Timers.ElapsedEventArgs e)
        {
            _waitEvent.Reset();

            string fileNameActual = string.Concat(_ptc.CachePathActual, ".info");
            string fileNameBackup = string.Concat(_ptc.CachePathBackup, ".info");

            FileInfo fileInfoActual = new FileInfo(fileNameActual);

            if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
            {
                File.Copy(fileNameActual, fileNameBackup, true);
            }

            Save(fileNameActual);

            _waitEvent.Set();
        }

        private void Save(string fileName)
        {
            int profiledFuncsCount;

            OuterHeader outerHeader = new OuterHeader();

            outerHeader.Magic = _outerHeaderMagic;

            outerHeader.InfoFileVersion = InternalVersion;
            outerHeader.Endianness = Ptc.GetEndianness();

            outerHeader.SetHeaderHash();

            using (MemoryStream stream = new MemoryStream())
            {
                Debug.Assert(stream.Seek(0L, SeekOrigin.Begin) == 0L && stream.Length == 0L);

                stream.Seek((long)Unsafe.SizeOf<Hash128>(), SeekOrigin.Begin);

                lock (_lock)
                {
                    Serialize(stream, ProfiledFuncs);

                    profiledFuncsCount = ProfiledFuncs.Count;
                }

                Debug.Assert(stream.Position == stream.Length);

                stream.Seek((long)Unsafe.SizeOf<Hash128>(), SeekOrigin.Begin);
                Hash128 hash = XXHash128.ComputeHash(GetReadOnlySpan(stream));

                stream.Seek(0L, SeekOrigin.Begin);
                SerializeStructure(stream, hash);

                if (hash == _lastHash)
                {
                    return;
                }

                using (FileStream compressedStream = new(fileName, FileMode.OpenOrCreate))
                using (DeflateStream deflateStream = new(compressedStream, SaveCompressionLevel, true))
                {
                    try
                    {
                        SerializeStructure(compressedStream, outerHeader);

                        stream.WriteTo(deflateStream);

                        _lastHash = hash;
                    }
                    catch
                    {
                        compressedStream.Position = 0L;

                        _lastHash = default;
                    }

                    if (compressedStream.Position < compressedStream.Length)
                    {
                        compressedStream.SetLength(compressedStream.Position);
                    }
                }
            }

            long fileSize = new FileInfo(fileName).Length;

            if (fileSize != 0L)
            {
                Logger.Info?.Print(LogClass.Ptc, $"Saved Profiling Info (size: {fileSize} bytes, profiled functions: {profiledFuncsCount}).");
            }
        }

        private void Serialize(Stream stream, Dictionary<ulong, FuncProfile> profiledFuncs)
        {
            SerializeDictionary(stream, profiledFuncs, (stream, structure) => SerializeStructure(stream, structure));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 29*/)]
        private struct OuterHeader
        {
            public ulong Magic;

            public uint InfoFileVersion;

            public bool Endianness;

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

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 5*/)]
        public struct FuncProfile
        {
            public ExecutionMode Mode;
            public bool HighCq;

            public FuncProfile(ExecutionMode mode, bool highCq)
            {
                Mode = mode;
                HighCq = highCq;
            }
        }

        public void Start()
        {
            if (_ptc.State == PtcState.Enabled ||
                _ptc.State == PtcState.Continuing)
            {
                Enabled = true;

                _timer.Enabled = true;
            }
        }

        public void Stop()
        {
            Enabled = false;

            if (!_disposed)
            {
                _timer.Enabled = false;
            }
        }

        public void Wait()
        {
            _waitEvent.WaitOne();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _timer.Elapsed -= PreSave;
                _timer.Dispose();

                Wait();
                _waitEvent.Dispose();
            }
        }
    }
}