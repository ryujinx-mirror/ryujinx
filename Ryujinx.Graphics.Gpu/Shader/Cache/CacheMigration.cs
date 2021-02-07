using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    /// <summary>
    /// Class handling shader cache migrations.
    /// </summary>
    static class CacheMigration
    {
        /// <summary>
        /// Check if the given cache version need to recompute its hash.
        /// </summary>
        /// <param name="version">The version in use</param>
        /// <param name="newVersion">The new version after migration</param>
        /// <returns>True if a hash recompute is needed</returns>
        public static bool NeedHashRecompute(ulong version, out ulong newVersion)
        {
            const ulong TargetBrokenVersion = 1717;
            const ulong TargetFixedVersion = 1759;

            newVersion = TargetFixedVersion;

            if (version == TargetBrokenVersion)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move a file with the name of a given hash to another in the cache archive.
        /// </summary>
        /// <param name="archive">The archive in use</param>
        /// <param name="oldKey">The old key</param>
        /// <param name="newKey">The new key</param>
        private static void MoveEntry(ZipArchive archive, Hash128 oldKey, Hash128 newKey)
        {
            ZipArchiveEntry oldGuestEntry = archive.GetEntry($"{oldKey}");

            if (oldGuestEntry != null)
            {
                ZipArchiveEntry newGuestEntry = archive.CreateEntry($"{newKey}");

                using (Stream oldStream = oldGuestEntry.Open())
                using (Stream newStream = newGuestEntry.Open())
                {
                    oldStream.CopyTo(newStream);
                }

                oldGuestEntry.Delete();
            }
        }

        /// <summary>
        /// Recompute all the hashes of a given cache.
        /// </summary>
        /// <param name="guestBaseCacheDirectory">The guest cache directory path</param>
        /// <param name="hostBaseCacheDirectory">The host cache directory path</param>
        /// <param name="graphicsApi">The graphics api in use</param>
        /// <param name="hashType">The hash type in use</param>
        /// <param name="newVersion">The version to write in the host and guest manifest after migration</param>
        private static void RecomputeHashes(string guestBaseCacheDirectory, string hostBaseCacheDirectory, CacheGraphicsApi graphicsApi, CacheHashType hashType, ulong newVersion)
        {
            string guestManifestPath = CacheHelper.GetManifestPath(guestBaseCacheDirectory);
            string hostManifestPath = CacheHelper.GetManifestPath(hostBaseCacheDirectory);

            if (CacheHelper.TryReadManifestFile(guestManifestPath, CacheGraphicsApi.Guest, hashType, out _, out HashSet<Hash128> guestEntries))
            {
                CacheHelper.TryReadManifestFile(hostManifestPath, graphicsApi, hashType, out _, out HashSet<Hash128> hostEntries);

                Logger.Info?.Print(LogClass.Gpu, "Shader cache hashes need to be recomputed, performing migration...");

                string guestArchivePath = CacheHelper.GetArchivePath(guestBaseCacheDirectory);
                string hostArchivePath = CacheHelper.GetArchivePath(hostBaseCacheDirectory);

                ZipArchive guestArchive = ZipFile.Open(guestArchivePath, ZipArchiveMode.Update);
                ZipArchive hostArchive = ZipFile.Open(hostArchivePath, ZipArchiveMode.Update);

                CacheHelper.EnsureArchiveUpToDate(guestBaseCacheDirectory, guestArchive, guestEntries);
                CacheHelper.EnsureArchiveUpToDate(hostBaseCacheDirectory, hostArchive, hostEntries);

                int programIndex = 0;

                HashSet<Hash128> newEntries = new HashSet<Hash128>();

                foreach (Hash128 oldHash in guestEntries)
                {
                    byte[] guestProgram = CacheHelper.ReadFromArchive(guestArchive, oldHash);

                    Logger.Info?.Print(LogClass.Gpu, $"Migrating shader {oldHash} ({programIndex + 1} / {guestEntries.Count})");

                    if (guestProgram != null)
                    {
                        ReadOnlySpan<byte> guestProgramReadOnlySpan = guestProgram;

                        ReadOnlySpan<GuestShaderCacheEntry> cachedShaderEntries = GuestShaderCacheEntry.Parse(ref guestProgramReadOnlySpan, out GuestShaderCacheHeader fileHeader);

                        TransformFeedbackDescriptor[] tfd = CacheHelper.ReadTransformFeedbackInformation(ref guestProgramReadOnlySpan, fileHeader);

                        Hash128 newHash = CacheHelper.ComputeGuestHashFromCache(cachedShaderEntries, tfd);

                        if (newHash != oldHash)
                        {
                            MoveEntry(guestArchive, oldHash, newHash);
                            MoveEntry(hostArchive, oldHash, newHash);
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Gpu, $"Same hashes for shader {oldHash}");
                        }

                        newEntries.Add(newHash);
                    }

                    programIndex++;
                }

                byte[] newGuestManifestContent = CacheHelper.ComputeManifest(newVersion, CacheGraphicsApi.Guest, hashType, newEntries);
                byte[] newHostManifestContent = CacheHelper.ComputeManifest(newVersion, graphicsApi, hashType, newEntries);

                File.WriteAllBytes(guestManifestPath, newGuestManifestContent);
                File.WriteAllBytes(hostManifestPath, newHostManifestContent);

                guestArchive.Dispose();
                hostArchive.Dispose();
            }
        }

        /// <summary>
        /// Check and run cache migration if needed.
        /// </summary>
        /// <param name="baseCacheDirectory">The base path of the cache</param>
        /// <param name="graphicsApi">The graphics api in use</param>
        /// <param name="hashType">The hash type in use</param>
        /// <param name="shaderProvider">The shader provider name of the cache</param>
        public static void Run(string baseCacheDirectory, CacheGraphicsApi graphicsApi, CacheHashType hashType, string shaderProvider)
        {
            string guestBaseCacheDirectory = CacheHelper.GenerateCachePath(baseCacheDirectory, CacheGraphicsApi.Guest, "", "program");
            string hostBaseCacheDirectory = CacheHelper.GenerateCachePath(baseCacheDirectory, graphicsApi, shaderProvider, "host");

            string guestArchivePath = CacheHelper.GetArchivePath(guestBaseCacheDirectory);
            string hostArchivePath = CacheHelper.GetArchivePath(hostBaseCacheDirectory);

            bool isReadOnly = CacheHelper.IsArchiveReadOnly(guestArchivePath) || CacheHelper.IsArchiveReadOnly(hostArchivePath);

            if (!isReadOnly && CacheHelper.TryReadManifestHeader(CacheHelper.GetManifestPath(guestBaseCacheDirectory), out CacheManifestHeader header))
            {
                if (NeedHashRecompute(header.Version, out ulong newVersion))
                {
                    RecomputeHashes(guestBaseCacheDirectory, hostBaseCacheDirectory, graphicsApi, hashType, newVersion);
                }
            }
        }
    }
}
