using ICSharpCode.SharpZipLib.Zip;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using Ryujinx.Graphics.Shader;
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
        /// Read transform feedback descriptors from guest.
        /// </summary>
        /// <param name="data">The raw guest transform feedback descriptors</param>
        /// <param name="header">The guest shader program header</param>
        /// <returns>The transform feedback descriptors read from guest</returns>
        public static TransformFeedbackDescriptorOld[] ReadTransformFeedbackInformation(ref ReadOnlySpan<byte> data, GuestShaderCacheHeader header)
        {
            if (header.TransformFeedbackCount != 0)
            {
                TransformFeedbackDescriptorOld[] result = new TransformFeedbackDescriptorOld[header.TransformFeedbackCount];

                for (int i = 0; i < result.Length; i++)
                {
                    GuestShaderCacheTransformFeedbackHeader feedbackHeader = MemoryMarshal.Read<GuestShaderCacheTransformFeedbackHeader>(data);

                    result[i] = new TransformFeedbackDescriptorOld(feedbackHeader.BufferIndex, feedbackHeader.Stride, data.Slice(Unsafe.SizeOf<GuestShaderCacheTransformFeedbackHeader>(), feedbackHeader.VaryingLocationsLength).ToArray());

                    data = data.Slice(Unsafe.SizeOf<GuestShaderCacheTransformFeedbackHeader>() + feedbackHeader.VaryingLocationsLength);
                }

                return result;
            }

            return null;
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
