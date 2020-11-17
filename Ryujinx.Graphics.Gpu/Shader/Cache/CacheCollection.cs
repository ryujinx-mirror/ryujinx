using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    /// <summary>
    /// Represent a cache collection handling one shader cache.
    /// </summary>
    class CacheCollection : IDisposable
    {
        /// <summary>
        /// Possible operation to do on the <see cref="_fileWriterWorkerQueue"/>.
        /// </summary>
        private enum CacheFileOperation
        {
            /// <summary>
            /// Save a new entry in the temp cache.
            /// </summary>
            SaveTempEntry,

            /// <summary>
            /// Save the hash manifest.
            /// </summary>
            SaveManifest,

            /// <summary>
            /// Remove entries from the hash manifest and save it.
            /// </summary>
            RemoveManifestEntries,

            /// <summary>
            /// Flush temporary cache to archive.
            /// </summary>
            FlushToArchive,

            /// <summary>
            /// Signal when hitting this point. This is useful to know if all previous operations were performed.
            /// </summary>
            Synchronize
        }

        /// <summary>
        /// Represent an operation to perform on the <see cref="_fileWriterWorkerQueue"/>.
        /// </summary>
        private class CacheFileOperationTask
        {
            /// <summary>
            /// The type of operation to perform.
            /// </summary>
            public CacheFileOperation Type;

            /// <summary>
            /// The data associated to this operation or null.
            /// </summary>
            public object Data;
        }

        /// <summary>
        /// Data associated to the <see cref="CacheFileOperation.SaveTempEntry"/> operation.
        /// </summary>
        private class CacheFileSaveEntryTaskData
        {
            /// <summary>
            /// The key of the entry to cache.
            /// </summary>
            public Hash128 Key;

            /// <summary>
            /// The value of the entry to cache.
            /// </summary>
            public byte[] Value;
        }

        /// <summary>
        /// The directory of the shader cache.
        /// </summary>
        private readonly string _cacheDirectory;

        /// <summary>
        /// The version of the cache.
        /// </summary>
        private readonly ulong _version;

        /// <summary>
        /// The hash type of the cache.
        /// </summary>
        private readonly CacheHashType _hashType;

        /// <summary>
        /// The graphics API of the cache.
        /// </summary>
        private readonly CacheGraphicsApi _graphicsApi;

        /// <summary>
        /// The table of all the hash registered in the cache.
        /// </summary>
        private HashSet<Hash128> _hashTable;

        /// <summary>
        /// The queue of operations to be performed by the file writer worker.
        /// </summary>
        private AsyncWorkQueue<CacheFileOperationTask> _fileWriterWorkerQueue;

        /// <summary>
        /// Main storage of the cache collection.
        /// </summary>
        private ZipArchive _cacheArchive;

        /// <summary>
        /// Immutable copy of the hash table.
        /// </summary>
        public ReadOnlySpan<Hash128> HashTable => _hashTable.ToArray();

        /// <summary>
        /// Get the temp path to the cache data directory.
        /// </summary>
        /// <returns>The temp path to the cache data directory</returns>
        private string GetCacheTempDataPath() => Path.Combine(_cacheDirectory, "temp");

        /// <summary>
        /// The path to the cache archive file.
        /// </summary>
        /// <returns>The path to the cache archive file</returns>
        private string GetArchivePath() => Path.Combine(_cacheDirectory, "cache.zip");

        /// <summary>
        /// The path to the cache manifest file.
        /// </summary>
        /// <returns>The path to the cache manifest file</returns>
        private string GetManifestPath() => Path.Combine(_cacheDirectory, "cache.info");

        /// <summary>
        /// Create a new temp path to the given cached file via its hash.
        /// </summary>
        /// <param name="key">The hash of the cached data</param>
        /// <returns>New path to the given cached file</returns>
        private string GenCacheTempFilePath(Hash128 key) => Path.Combine(GetCacheTempDataPath(), key.ToString());

        /// <summary>
        /// Create a new cache collection.
        /// </summary>
        /// <param name="baseCacheDirectory">The directory of the shader cache</param>
        /// <param name="hashType">The hash type of the shader cache</param>
        /// <param name="graphicsApi">The graphics api of the shader cache</param>
        /// <param name="shaderProvider">The shader provider name of the shader cache</param>
        /// <param name="cacheName">The name of the cache</param>
        /// <param name="version">The version of the cache</param>
        public CacheCollection(string baseCacheDirectory, CacheHashType hashType, CacheGraphicsApi graphicsApi, string shaderProvider, string cacheName, ulong version)
        {
            if (hashType != CacheHashType.XxHash128)
            {
                throw new NotImplementedException($"{hashType}");
            }

            _cacheDirectory = GenerateCachePath(baseCacheDirectory, graphicsApi, shaderProvider, cacheName);
            _graphicsApi = graphicsApi;
            _hashType = hashType;
            _version = version;
            _hashTable = new HashSet<Hash128>();

            Load();

            _fileWriterWorkerQueue = new AsyncWorkQueue<CacheFileOperationTask>(HandleCacheTask, $"CacheCollection.Worker.{cacheName}");
        }

        /// <summary>
        /// Load the cache manifest file and recreate it if invalid.
        /// </summary>
        private void Load()
        {
            bool isInvalid = false;

            if (!Directory.Exists(_cacheDirectory))
            {
                isInvalid = true;
            }
            else
            {
                string manifestPath = GetManifestPath();

                if (File.Exists(manifestPath))
                {
                    Memory<byte> rawManifest = File.ReadAllBytes(manifestPath);

                    if (MemoryMarshal.TryRead(rawManifest.Span, out CacheManifestHeader manifestHeader))
                    {
                        Memory<byte> hashTableRaw = rawManifest.Slice(Unsafe.SizeOf<CacheManifestHeader>());

                        isInvalid = !manifestHeader.IsValid(_version, _graphicsApi, _hashType, hashTableRaw.Span);

                        if (!isInvalid)
                        {
                            ReadOnlySpan<Hash128> hashTable = MemoryMarshal.Cast<byte, Hash128>(hashTableRaw.Span);

                            foreach (Hash128 hash in hashTable)
                            {
                                _hashTable.Add(hash);
                            }
                        }
                    }
                }
                else
                {
                    isInvalid = true;
                }
            }

            if (isInvalid)
            {
                Logger.Warning?.Print(LogClass.Gpu, $"Shader collection \"{_cacheDirectory}\" got invalidated, cache will need to be rebuilt.");

                if (Directory.Exists(_cacheDirectory))
                {
                    Directory.Delete(_cacheDirectory, true);
                }

                Directory.CreateDirectory(_cacheDirectory);

                SaveManifest();
            }

            FlushToArchive();
        }

        /// <summary>
        /// Queue a task to remove entries from the hash manifest.
        /// </summary>
        /// <param name="entries">Entries to remove from the manifest</param>
        public void RemoveManifestEntriesAsync(HashSet<Hash128> entries)
        {
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask
            {
                Type = CacheFileOperation.RemoveManifestEntries,
                Data = entries
            });
        }

        /// <summary>
        /// Remove given entries from the manifest.
        /// </summary>
        /// <param name="entries">Entries to remove from the manifest</param>
        private void RemoveManifestEntries(HashSet<Hash128> entries)
        {
            lock (_hashTable)
            {
                foreach (Hash128 entry in entries)
                {
                    _hashTable.Remove(entry);
                }

                SaveManifest();
            }
        }

        /// <summary>
        /// Queue a task to flush temporary files to the archive on the worker.
        /// </summary>
        public void FlushToArchiveAsync()
        {
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask
            {
                Type = CacheFileOperation.FlushToArchive
            });
        }

        /// <summary>
        /// Wait for all tasks before this given point to be done.
        /// </summary>
        public void Synchronize()
        {
            using (ManualResetEvent evnt = new ManualResetEvent(false))
            {
                _fileWriterWorkerQueue.Add(new CacheFileOperationTask
                {
                    Type = CacheFileOperation.Synchronize,
                    Data = evnt
                });

                evnt.WaitOne();
            }
        }

        /// <summary>
        /// Flush temporary files to the archive.
        /// </summary>
        /// <remarks>This dispose <see cref="_cacheArchive"/> if not null and reinstantiate it.</remarks>
        private void FlushToArchive()
        {
            EnsureArchiveUpToDate();

            // Open the zip in readonly to avoid anyone modifying/corrupting it during normal operations.
            _cacheArchive = ZipFile.Open(GetArchivePath(), ZipArchiveMode.Read);
        }

        /// <summary>
        /// Save temporary files not in archive.
        /// </summary>
        /// <remarks>This dispose <see cref="_cacheArchive"/> if not null.</remarks>
        public void EnsureArchiveUpToDate()
        {
            // First close previous opened instance if found.
            if (_cacheArchive != null)
            {
                _cacheArchive.Dispose();
            }

            string archivePath = GetArchivePath();

            // Open the zip in read/write.
            _cacheArchive = ZipFile.Open(archivePath, ZipArchiveMode.Update);

            Logger.Info?.Print(LogClass.Gpu, $"Updating cache collection archive {archivePath}...");

            // Update the content of the zip.
            lock (_hashTable)
            {
                foreach (Hash128 hash in _hashTable)
                {
                    string cacheTempFilePath = GenCacheTempFilePath(hash);

                    if (File.Exists(cacheTempFilePath))
                    {
                        string cacheHash = $"{hash}";

                        ZipArchiveEntry entry = _cacheArchive.GetEntry(cacheHash);

                        entry?.Delete();

                        _cacheArchive.CreateEntryFromFile(cacheTempFilePath, cacheHash);
                        File.Delete(cacheTempFilePath);
                    }
                }

                // Close the instance to force a flush.
                _cacheArchive.Dispose();
                _cacheArchive = null;

                string cacheTempDataPath = GetCacheTempDataPath();

                // Create the cache data path if missing.
                if (!Directory.Exists(cacheTempDataPath))
                {
                    Directory.CreateDirectory(cacheTempDataPath);
                }
            }

            Logger.Info?.Print(LogClass.Gpu, $"Updated cache collection archive {archivePath}.");
        }

        /// <summary>
        /// Save the manifest file.
        /// </summary>
        private void SaveManifest()
        {
            CacheManifestHeader manifestHeader = new CacheManifestHeader(_version, _graphicsApi, _hashType);

            byte[] data;

            lock (_hashTable)
            {
                data = new byte[Unsafe.SizeOf<CacheManifestHeader>() + _hashTable.Count * Unsafe.SizeOf<Hash128>()];

                // CacheManifestHeader has the same size as a Hash128.
                Span<Hash128> dataSpan = MemoryMarshal.Cast<byte, Hash128>(data.AsSpan()).Slice(1);

                int i = 0;

                foreach (Hash128 hash in _hashTable)
                {
                    dataSpan[i++] = hash;
                }
            }

            manifestHeader.UpdateChecksum(data.AsSpan().Slice(Unsafe.SizeOf<CacheManifestHeader>()));

            MemoryMarshal.Write(data, ref manifestHeader);

            File.WriteAllBytes(GetManifestPath(), data);
        }

        /// <summary>
        /// Generate the path to the cache directory.
        /// </summary>
        /// <param name="baseCacheDirectory">The base of the cache directory</param>
        /// <param name="graphicsApi">The graphics api in use</param>
        /// <param name="shaderProvider">The name of the shader provider in use</param>
        /// <param name="cacheName">The name of the cache</param>
        /// <returns>The path to the cache directory</returns>
        private static string GenerateCachePath(string baseCacheDirectory, CacheGraphicsApi graphicsApi, string shaderProvider, string cacheName)
        {
            string graphicsApiName = graphicsApi switch
            {
                CacheGraphicsApi.OpenGL => "opengl",
                CacheGraphicsApi.OpenGLES => "opengles",
                CacheGraphicsApi.Vulkan => "vulkan",
                CacheGraphicsApi.DirectX => "directx",
                CacheGraphicsApi.Metal => "metal",
                CacheGraphicsApi.Guest => "guest",
                _ => throw new NotImplementedException(graphicsApi.ToString()),
            };

            return Path.Combine(baseCacheDirectory, graphicsApiName, shaderProvider, cacheName);
        }

        /// <summary>
        /// Get a cached file with the given hash.
        /// </summary>
        /// <param name="keyHash">The given hash</param>
        /// <returns>The cached file if present or null</returns>
        public byte[] GetValueRaw(ref Hash128 keyHash)
        {
            return GetValueRawFromArchive(ref keyHash) ?? GetValueRawFromFile(ref keyHash);
        }

        /// <summary>
        /// Get a cached file with the given hash that is present in the archive.
        /// </summary>
        /// <param name="keyHash">The given hash</param>
        /// <returns>The cached file if present or null</returns>
        private byte[] GetValueRawFromArchive(ref Hash128 keyHash)
        {
            bool found;

            lock (_hashTable)
            {
                found = _hashTable.Contains(keyHash);
            }

            if (found)
            {
                ZipArchiveEntry archiveEntry = _cacheArchive.GetEntry($"{keyHash}");

                if (archiveEntry != null)
                {
                    try
                    {
                        byte[] result = new byte[archiveEntry.Length];

                        using (Stream archiveStream = archiveEntry.Open())
                        {
                            archiveStream.Read(result);

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error?.Print(LogClass.Gpu, $"Cannot load cache file {keyHash} from archive");
                        Logger.Error?.Print(LogClass.Gpu, e.ToString());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a cached file with the given hash that is not present in the archive.
        /// </summary>
        /// <param name="keyHash">The given hash</param>
        /// <returns>The cached file if present or null</returns>
        private byte[] GetValueRawFromFile(ref Hash128 keyHash)
        {
            bool found;

            lock (_hashTable)
            {
                found = _hashTable.Contains(keyHash);
            }

            if (found)
            {
                string cacheTempFilePath = GenCacheTempFilePath(keyHash);

                try
                {
                    return File.ReadAllBytes(GenCacheTempFilePath(keyHash));
                }
                catch (Exception e)
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Cannot load cache file at {cacheTempFilePath}");
                    Logger.Error?.Print(LogClass.Gpu, e.ToString());
                }
            }

            return null;
        }

        private void HandleCacheTask(CacheFileOperationTask task)
        {
            switch (task.Type)
            {
                case CacheFileOperation.SaveTempEntry:
                    SaveTempEntry((CacheFileSaveEntryTaskData)task.Data);
                    break;
                case CacheFileOperation.SaveManifest:
                    SaveManifest();
                    break;
                case CacheFileOperation.RemoveManifestEntries:
                    RemoveManifestEntries((HashSet<Hash128>)task.Data);
                    break;
                case CacheFileOperation.FlushToArchive:
                    FlushToArchive();
                    break;
                case CacheFileOperation.Synchronize:
                    ((ManualResetEvent)task.Data).Set();
                    break;
                default:
                    throw new NotImplementedException($"{task.Type}");
            }

        }

        /// <summary>
        /// Save a new entry in the temp cache.
        /// </summary>
        /// <param name="entry">The entry to save in the temp cache</param>
        private void SaveTempEntry(CacheFileSaveEntryTaskData entry)
        {
            string tempPath = GenCacheTempFilePath(entry.Key);

            File.WriteAllBytes(tempPath, entry.Value);
        }

        /// <summary>
        /// Add a new value in the cache with a given hash.
        /// </summary>
        /// <param name="keyHash">The hash to use for the value in the cache</param>
        /// <param name="value">The value to cache</param>
        public void AddValue(ref Hash128 keyHash, byte[] value)
        {
            Debug.Assert(value != null);
            Debug.Assert(GetValueRaw(ref keyHash) != null);

            bool isAlreadyPresent;

            lock (_hashTable)
            {
                isAlreadyPresent = !_hashTable.Add(keyHash);
            }

            if (isAlreadyPresent)
            {
                // NOTE: Used for debug
                File.WriteAllBytes(GenCacheTempFilePath(new Hash128()), value);

                throw new InvalidOperationException($"Cache collision found on {GenCacheTempFilePath(keyHash)}");
            }

            // Queue file change operations
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask
            {
                Type = CacheFileOperation.SaveTempEntry,
                Data = new CacheFileSaveEntryTaskData
                {
                    Key = keyHash,
                    Value = value
                }
            });

            // Save the manifest changes
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask
            {
                Type = CacheFileOperation.SaveManifest,
            });
        }

        /// <summary>
        /// Replace a value at the given hash in the cache.
        /// </summary>
        /// <param name="keyHash">The hash to use for the value in the cache</param>
        /// <param name="value">The value to cache</param>
        public void ReplaceValue(ref Hash128 keyHash, byte[] value)
        {
            Debug.Assert(value != null);

            // Only queue file change operations
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask
            {
                Type = CacheFileOperation.SaveTempEntry,
                Data = new CacheFileSaveEntryTaskData
                {
                    Key = keyHash,
                    Value = value
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Make sure all operations on _fileWriterWorkerQueue are done.
                Synchronize();

                _fileWriterWorkerQueue.Dispose();
                EnsureArchiveUpToDate();
            }
        }
    }
}
