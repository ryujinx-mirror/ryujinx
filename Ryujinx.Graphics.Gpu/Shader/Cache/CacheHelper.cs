using ICSharpCode.SharpZipLib.Zip;
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

            manifestHeader.UpdateChecksum(data.AsSpan(Unsafe.SizeOf<CacheManifestHeader>()));

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
        public static byte[] ReadFromArchive(ZipFile archive, Hash128 entry)
        {
            if (archive != null)
            {
                ZipEntry archiveEntry = archive.GetEntry($"{entry}");

                if (archiveEntry != null)
                {
                    try
                    {
                        byte[] result = new byte[archiveEntry.Size];

                        using (Stream archiveStream = archive.GetInputStream(archiveEntry))
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
        /// Packs the tessellation parameters from the gpu accessor.
        /// </summary>
        /// <param name="gpuAccessor">The gpu accessor</param>
        /// <returns>The packed tessellation parameters</returns>
        private static byte GetTessellationModePacked(IGpuAccessor gpuAccessor)
        {
            byte value;

            value = (byte)((int)gpuAccessor.QueryTessPatchType() & 3);
            value |= (byte)(((int)gpuAccessor.QueryTessSpacing() & 3) << 2);

            if (gpuAccessor.QueryTessCw())
            {
                value |= 0x10;
            }

            return value;
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
                TessellationModePacked = GetTessellationModePacked(gpuAccessor),
                StateFlags = GetGpuStateFlags(gpuAccessor)
            };
        }

        /// <summary>
        /// Create guest shader cache entries from the runtime contexts.
        /// </summary>
        /// <param name="channel">The GPU channel in use</param>
        /// <param name="shaderContexts">The runtime contexts</param>
        /// <returns>Guest shader cahe entries from the runtime contexts</returns>
        public static GuestShaderCacheEntry[] CreateShaderCacheEntries(GpuChannel channel, ReadOnlySpan<TranslatorContext> shaderContexts)
        {
            MemoryManager memoryManager = channel.MemoryManager;

            int startIndex = shaderContexts.Length > 1 ? 1 : 0;

            GuestShaderCacheEntry[] entries = new GuestShaderCacheEntry[shaderContexts.Length - startIndex];

            for (int i = startIndex; i < shaderContexts.Length; i++)
            {
                TranslatorContext context = shaderContexts[i];

                if (context == null)
                {
                    continue;
                }

                GpuAccessor gpuAccessor = context.GpuAccessor as GpuAccessor;

                ulong cb1DataAddress;
                int cb1DataSize = gpuAccessor?.Cb1DataSize ?? 0;

                if (context.Stage == ShaderStage.Compute)
                {
                    cb1DataAddress = channel.BufferManager.GetComputeUniformBufferAddress(1);
                }
                else
                {
                    int stageIndex = context.Stage switch
                    {
                        ShaderStage.TessellationControl => 1,
                        ShaderStage.TessellationEvaluation => 2,
                        ShaderStage.Geometry => 3,
                        ShaderStage.Fragment => 4,
                        _ => 0
                    };

                    cb1DataAddress = channel.BufferManager.GetGraphicsUniformBufferAddress(stageIndex, 1);
                }

                int size = context.Size;

                TranslatorContext translatorContext2 = i == 1 ? shaderContexts[0] : null;

                int sizeA = translatorContext2 != null ? translatorContext2.Size : 0;

                byte[] code = new byte[size + cb1DataSize + sizeA];

                memoryManager.GetSpan(context.Address, size).CopyTo(code);

                if (cb1DataAddress != 0 && cb1DataSize != 0)
                {
                    memoryManager.Physical.GetSpan(cb1DataAddress, cb1DataSize).CopyTo(code.AsSpan(size, cb1DataSize));
                }

                if (translatorContext2 != null)
                {
                    memoryManager.GetSpan(translatorContext2.Address, sizeA).CopyTo(code.AsSpan(size + cb1DataSize, sizeA));
                }

                GuestGpuAccessorHeader gpuAccessorHeader = CreateGuestGpuAccessorCache(context.GpuAccessor);

                if (gpuAccessor != null)
                {
                    gpuAccessorHeader.TextureDescriptorCount = context.TextureHandlesForCache.Count;
                }

                GuestShaderCacheEntryHeader header = new GuestShaderCacheEntryHeader(
                    context.Stage,
                    size + cb1DataSize,
                    sizeA,
                    cb1DataSize,
                    gpuAccessorHeader);

                GuestShaderCacheEntry entry = new GuestShaderCacheEntry(header, code);

                if (gpuAccessor != null)
                {
                    foreach (int textureHandle in context.TextureHandlesForCache)
                    {
                        GuestTextureDescriptor textureDescriptor = ((Image.TextureDescriptor)gpuAccessor.GetTextureDescriptor(textureHandle, -1)).ToCache();

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
        public static void EnsureArchiveUpToDate(string baseCacheDirectory, ZipFile archive, HashSet<Hash128> entries)
        {
            List<string> filesToDelete = new List<string>();

            archive.BeginUpdate();

            foreach (Hash128 hash in entries)
            {
                string cacheTempFilePath = GenCacheTempFilePath(baseCacheDirectory, hash);

                if (File.Exists(cacheTempFilePath))
                {
                    string cacheHash = $"{hash}";

                    ZipEntry entry = archive.GetEntry(cacheHash);

                    if (entry != null)
                    {
                        archive.Delete(entry);
                    }

                    // We enforce deflate compression here to avoid possible incompatibilities on older version of Ryujinx that use System.IO.Compression.
                    archive.Add(new StaticDiskDataSource(cacheTempFilePath), cacheHash, CompressionMethod.Deflated);
                    filesToDelete.Add(cacheTempFilePath);
                }
            }

            archive.CommitUpdate();

            foreach (string filePath in filesToDelete)
            {
                File.Delete(filePath);
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
