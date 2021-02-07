using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    /// <summary>
    /// Helper to manipulate the disk shader cache.
    /// </summary>
    static class CacheHelper
    {
        /// <summary>
        /// Try to read the manifest header from a given file path.
        /// </summary>
        /// <param name="manifestPath">The path to the manifest file</param>
        /// <param name="header">The manifest header read</param>
        /// <returns>Return true if the manifest header was read</returns>
        public static bool TryReadManifestHeader(string manifestPath, out CacheManifestHeader header)
        {
            header = default;

            if (File.Exists(manifestPath))
            {
                Memory<byte> rawManifest = File.ReadAllBytes(manifestPath);

                if (MemoryMarshal.TryRead(rawManifest.Span, out header))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to read the manifest from a given file path.
        /// </summary>
        /// <param name="manifestPath">The path to the manifest file</param>
        /// <param name="graphicsApi">The graphics api used by the cache</param>
        /// <param name="hashType">The hash type of the cache</param>
        /// <param name="header">The manifest header read</param>
        /// <param name="entries">The entries read from the cache manifest</param>
        /// <returns>Return true if the manifest was read</returns>
        public static bool TryReadManifestFile(string manifestPath, CacheGraphicsApi graphicsApi, CacheHashType hashType, out CacheManifestHeader header, out HashSet<Hash128> entries)
        {
            header = default;
            entries = new HashSet<Hash128>();

            if (File.Exists(manifestPath))
            {
                Memory<byte> rawManifest = File.ReadAllBytes(manifestPath);

                if (MemoryMarshal.TryRead(rawManifest.Span, out header))
                {
                    Memory<byte> hashTableRaw = rawManifest.Slice(Unsafe.SizeOf<CacheManifestHeader>());

                    bool isValid = header.IsValid(graphicsApi, hashType, hashTableRaw.Span);

                    if (isValid)
                    {
                        ReadOnlySpan<Hash128> hashTable = MemoryMarshal.Cast<byte, Hash128>(hashTableRaw.Span);

                        foreach (Hash128 hash in hashTable)
                        {
                            entries.Add(hash);
                        }
                    }

                    return isValid;
                }
            }

            return false;
        }

        /// <summary>
        /// Compute a cache manifest from runtime data.
        /// </summary>
        /// <param name="version">The version of the cache</param>
        /// <param name="graphicsApi">The graphics api used by the cache</param>
        /// <param name="hashType">The hash type of the cache</param>
        /// <param name="entries">The entries in the cache</param>
        /// <returns>The cache manifest from runtime data</returns>
        public static byte[] ComputeManifest(ulong version, CacheGraphicsApi graphicsApi, CacheHashType hashType, HashSet<Hash128> entries)
        {
            if (hashType != CacheHashType.XxHash128)
            {
                throw new NotImplementedException($"{hashType}");
            }

            CacheManifestHeader manifestHeader = new CacheManifestHeader(version, graphicsApi, hashType);

            byte[] data = new byte[Unsafe.SizeOf<CacheManifestHeader>() + entries.Count * Unsafe.SizeOf<Hash128>()];

            // CacheManifestHeader has the same size as a Hash128.
            Span<Hash128> dataSpan = MemoryMarshal.Cast<byte, Hash128>(data.AsSpan()).Slice(1);

            int i = 0;

            foreach (Hash128 hash in entries)
            {
                dataSpan[i++] = hash;
            }

            manifestHeader.UpdateChecksum(data.AsSpan().Slice(Unsafe.SizeOf<CacheManifestHeader>()));

            MemoryMarshal.Write(data, ref manifestHeader);

            return data;
        }

        /// <summary>
        /// Get the base directory of the shader cache for a given title id.
        /// </summary>
        /// <param name="titleId">The title id of the target application</param>
        /// <returns>The base directory of the shader cache for a given title id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetBaseCacheDirectory(string titleId) => Path.Combine(AppDataManager.GamesDirPath, titleId, "cache", "shader");

        /// <summary>
        /// Get the temp path to the cache data directory.
        /// </summary>
        /// <param name="cacheDirectory">The cache directory</param>
        /// <returns>The temp path to the cache data directory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCacheTempDataPath(string cacheDirectory) => Path.Combine(cacheDirectory, "temp");

        /// <summary>
        /// The path to the cache archive file.
        /// </summary>
        /// <param name="cacheDirectory">The cache directory</param>
        /// <returns>The path to the cache archive file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetArchivePath(string cacheDirectory) => Path.Combine(cacheDirectory, "cache.zip");

        /// <summary>
        /// The path to the cache manifest file.
        /// </summary>
        /// <param name="cacheDirectory">The cache directory</param>
        /// <returns>The path to the cache manifest file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetManifestPath(string cacheDirectory) => Path.Combine(cacheDirectory, "cache.info");

        /// <summary>
        /// Create a new temp path to the given cached file via its hash.
        /// </summary>
        /// <param name="cacheDirectory">The cache directory</param>
        /// <param name="key">The hash of the cached data</param>
        /// <returns>New path to the given cached file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenCacheTempFilePath(string cacheDirectory, Hash128 key) => Path.Combine(GetCacheTempDataPath(cacheDirectory), key.ToString());

        /// <summary>
        /// Generate the path to the cache directory.
        /// </summary>
        /// <param name="baseCacheDirectory">The base of the cache directory</param>
        /// <param name="graphicsApi">The graphics api in use</param>
        /// <param name="shaderProvider">The name of the shader provider in use</param>
        /// <param name="cacheName">The name of the cache</param>
        /// <returns>The path to the cache directory</returns>
        public static string GenerateCachePath(string baseCacheDirectory, CacheGraphicsApi graphicsApi, string shaderProvider, string cacheName)
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
        /// Read a cached file with the given hash that is present in the archive.
        /// </summary>
        /// <param name="archive">The archive in use</param>
        /// <param name="entry">The given hash</param>
        /// <returns>The cached file if present or null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadFromArchive(ZipArchive archive, Hash128 entry)
        {
            if (archive != null)
            {
                ZipArchiveEntry archiveEntry = archive.GetEntry($"{entry}");

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
                        Logger.Error?.Print(LogClass.Gpu, $"Cannot load cache file {entry} from archive");
                        Logger.Error?.Print(LogClass.Gpu, e.ToString());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Read a cached file with the given hash that is not present in the archive.
        /// </summary>
        /// <param name="cacheDirectory">The cache directory</param>
        /// <param name="entry">The given hash</param>
        /// <returns>The cached file if present or null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadFromFile(string cacheDirectory, Hash128 entry)
        {
            string cacheTempFilePath = GenCacheTempFilePath(cacheDirectory, entry);

            try
            {
                return File.ReadAllBytes(cacheTempFilePath);
            }
            catch (Exception e)
            {
                Logger.Error?.Print(LogClass.Gpu, $"Cannot load cache file at {cacheTempFilePath}");
                Logger.Error?.Print(LogClass.Gpu, e.ToString());
            }

            return null;
        }

        /// <summary>
        /// Compute the guest program code for usage while dumping to disk or hash.
        /// </summary>
        /// <param name="cachedShaderEntries">The guest shader entries to use</param>
        /// <param name="tfd">The transform feedback descriptors</param>
        /// <param name="forHashCompute">Used to determine if the guest program code is generated for hashing</param>
        /// <returns>The guest program code for usage while dumping to disk or hash</returns>
        private static byte[] ComputeGuestProgramCode(ReadOnlySpan<GuestShaderCacheEntry> cachedShaderEntries, TransformFeedbackDescriptor[] tfd, bool forHashCompute = false)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                foreach (GuestShaderCacheEntry cachedShaderEntry in cachedShaderEntries)
                {
                    if (cachedShaderEntry != null)
                    {
                        // Code (and Code A if present)
                        stream.Write(cachedShaderEntry.Code);

                        if (forHashCompute)
                        {
                            // Guest GPU accessor header (only write this for hashes, already present in the header for dumps)
                            writer.WriteStruct(cachedShaderEntry.Header.GpuAccessorHeader);
                        }

                        // Texture descriptors
                        foreach (GuestTextureDescriptor textureDescriptor in cachedShaderEntry.TextureDescriptors.Values)
                        {
                            writer.WriteStruct(textureDescriptor);
                        }
                    }
                }

                // Transform feedback
                if (tfd != null)
                {
                    foreach (TransformFeedbackDescriptor transform in tfd)
                    {
                        writer.WriteStruct(new GuestShaderCacheTransformFeedbackHeader(transform.BufferIndex, transform.Stride, transform.VaryingLocations.Length));
                        writer.Write(transform.VaryingLocations);
                    }
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Compute a guest hash from shader entries.
        /// </summary>
        /// <param name="cachedShaderEntries">The guest shader entries to use</param>
        /// <param name="tfd">The optional transform feedback descriptors</param>
        /// <returns>A guest hash from shader entries</returns>
        public static Hash128 ComputeGuestHashFromCache(ReadOnlySpan<GuestShaderCacheEntry> cachedShaderEntries, TransformFeedbackDescriptor[] tfd = null)
        {
            return XXHash128.ComputeHash(ComputeGuestProgramCode(cachedShaderEntries, tfd, true));
        }

        /// <summary>
        /// Read transform feedback descriptors from guest.
        /// </summary>
        /// <param name="data">The raw guest transform feedback descriptors</param>
        /// <param name="header">The guest shader program header</param>
        /// <returns>The transform feedback descriptors read from guest</returns>
        public static TransformFeedbackDescriptor[] ReadTransformFeedbackInformation(ref ReadOnlySpan<byte> data, GuestShaderCacheHeader header)
        {
            if (header.TransformFeedbackCount != 0)
            {
                TransformFeedbackDescriptor[] result = new TransformFeedbackDescriptor[header.TransformFeedbackCount];

                for (int i = 0; i < result.Length; i++)
                {
                    GuestShaderCacheTransformFeedbackHeader feedbackHeader = MemoryMarshal.Read<GuestShaderCacheTransformFeedbackHeader>(data);

                    result[i] = new TransformFeedbackDescriptor(feedbackHeader.BufferIndex, feedbackHeader.Stride, data.Slice(Unsafe.SizeOf<GuestShaderCacheTransformFeedbackHeader>(), feedbackHeader.VaryingLocationsLength).ToArray());

                    data = data.Slice(Unsafe.SizeOf<GuestShaderCacheTransformFeedbackHeader>() + feedbackHeader.VaryingLocationsLength);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Builds gpu state flags using information from the given gpu accessor.
        /// </summary>
        /// <param name="gpuAccessor">The gpu accessor</param>
        /// <returns>The gpu state flags</returns>
        private static GuestGpuStateFlags GetGpuStateFlags(IGpuAccessor gpuAccessor)
        {
            GuestGpuStateFlags flags = 0;

            if (gpuAccessor.QueryEarlyZForce())
            {
                flags |= GuestGpuStateFlags.EarlyZForce;
            }

            return flags;
        }

        /// <summary>
        /// Create a new instance of <see cref="GuestGpuAccessorHeader"/> from an gpu accessor.
        /// </summary>
        /// <param name="gpuAccessor">The gpu accessor</param>
        /// <returns>A new instance of <see cref="GuestGpuAccessorHeader"/></returns>
        public static GuestGpuAccessorHeader CreateGuestGpuAccessorCache(IGpuAccessor gpuAccessor)
        {
            return new GuestGpuAccessorHeader
            {
                ComputeLocalSizeX = gpuAccessor.QueryComputeLocalSizeX(),
                ComputeLocalSizeY = gpuAccessor.QueryComputeLocalSizeY(),
                ComputeLocalSizeZ = gpuAccessor.QueryComputeLocalSizeZ(),
                ComputeLocalMemorySize = gpuAccessor.QueryComputeLocalMemorySize(),
                ComputeSharedMemorySize = gpuAccessor.QueryComputeSharedMemorySize(),
                PrimitiveTopology = gpuAccessor.QueryPrimitiveTopology(),
                StateFlags = GetGpuStateFlags(gpuAccessor)
            };
        }

        /// <summary>
        /// Create guest shader cache entries from the runtime contexts.
        /// </summary>
        /// <param name="memoryManager">The GPU memory manager in use</param>
        /// <param name="shaderContexts">The runtime contexts</param>
        /// <returns>Guest shader cahe entries from the runtime contexts</returns>
        public static GuestShaderCacheEntry[] CreateShaderCacheEntries(MemoryManager memoryManager, ReadOnlySpan<TranslatorContext> shaderContexts)
        {
            int startIndex = shaderContexts.Length > 1 ? 1 : 0;

            GuestShaderCacheEntry[] entries = new GuestShaderCacheEntry[shaderContexts.Length - startIndex];

            for (int i = startIndex; i < shaderContexts.Length; i++)
            {
                TranslatorContext context = shaderContexts[i];

                if (context == null)
                {
                    continue;
                }

                TranslatorContext translatorContext2 = i == 1 ? shaderContexts[0] : null;

                int sizeA = translatorContext2 != null ? translatorContext2.Size : 0;

                byte[] code = new byte[context.Size + sizeA];

                memoryManager.GetSpan(context.Address, context.Size).CopyTo(code);

                if (translatorContext2 != null)
                {
                    memoryManager.GetSpan(translatorContext2.Address, sizeA).CopyTo(code.AsSpan().Slice(context.Size, sizeA));
                }

                GuestGpuAccessorHeader gpuAccessorHeader = CreateGuestGpuAccessorCache(context.GpuAccessor);

                if (context.GpuAccessor is GpuAccessor)
                {
                    gpuAccessorHeader.TextureDescriptorCount = context.TextureHandlesForCache.Count;
                }

                GuestShaderCacheEntryHeader header = new GuestShaderCacheEntryHeader(context.Stage, context.Size, sizeA, gpuAccessorHeader);

                GuestShaderCacheEntry entry = new GuestShaderCacheEntry(header, code);

                if (context.GpuAccessor is GpuAccessor gpuAccessor)
                {
                    foreach (int textureHandle in context.TextureHandlesForCache)
                    {
                        GuestTextureDescriptor textureDescriptor = ((Image.TextureDescriptor)gpuAccessor.GetTextureDescriptor(textureHandle)).ToCache();

                        textureDescriptor.Handle = (uint)textureHandle;

                        entry.TextureDescriptors.Add(textureHandle, textureDescriptor);
                    }
                }

                entries[i - startIndex] = entry;
            }

            return entries;
        }

        /// <summary>
        /// Create a guest shader program.
        /// </summary>
        /// <param name="shaderCacheEntries">The entries composing the guest program dump</param>
        /// <param name="tfd">The transform feedback descriptors in use</param>
        /// <returns>The resulting guest shader program</returns>
        public static byte[] CreateGuestProgramDump(GuestShaderCacheEntry[] shaderCacheEntries, TransformFeedbackDescriptor[] tfd = null)
        {
            using (MemoryStream resultStream = new MemoryStream())
            {
                BinaryWriter resultStreamWriter = new BinaryWriter(resultStream);

                byte transformFeedbackCount = 0;

                if (tfd != null)
                {
                    transformFeedbackCount = (byte)tfd.Length;
                }

                // Header
                resultStreamWriter.WriteStruct(new GuestShaderCacheHeader((byte)shaderCacheEntries.Length, transformFeedbackCount));

                // Write all entries header
                foreach (GuestShaderCacheEntry entry in shaderCacheEntries)
                {
                    if (entry == null)
                    {
                        resultStreamWriter.WriteStruct(new GuestShaderCacheEntryHeader());
                    }
                    else
                    {
                        resultStreamWriter.WriteStruct(entry.Header);
                    }
                }

                // Finally, write all program code and all transform feedback information.
                resultStreamWriter.Write(ComputeGuestProgramCode(shaderCacheEntries, tfd));

                return resultStream.ToArray();
            }
        }

        /// <summary>
        /// Save temporary files not in archive.
        /// </summary>
        /// <param name="baseCacheDirectory">The base of the cache directory</param>
        /// <param name="archive">The archive to use</param>
        /// <param name="entries">The entries in the cache</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureArchiveUpToDate(string baseCacheDirectory, ZipArchive archive, HashSet<Hash128> entries)
        {
            foreach (Hash128 hash in entries)
            {
                string cacheTempFilePath = GenCacheTempFilePath(baseCacheDirectory, hash);

                if (File.Exists(cacheTempFilePath))
                {
                    string cacheHash = $"{hash}";

                    ZipArchiveEntry entry = archive.GetEntry(cacheHash);

                    entry?.Delete();

                    archive.CreateEntryFromFile(cacheTempFilePath, cacheHash);

                    File.Delete(cacheTempFilePath);
                }
            }
        }

        public static bool IsArchiveReadOnly(string archivePath)
        {
            FileInfo info = new FileInfo(archivePath);

            if (!info.Exists)
            {
                return false;
            }

            try
            {
                using (FileStream stream = info.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
