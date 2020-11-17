using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    /// <summary>
    /// Global Manager of the shader cache.
    /// </summary>
    class CacheManager : IDisposable
    {
        private CacheGraphicsApi _graphicsApi;
        private CacheHashType _hashType;
        private string _shaderProvider;

        /// <summary>
        /// Cache storing raw Maxwell shaders as programs.
        /// </summary>
        private CacheCollection _guestProgramCache;

        /// <summary>
        /// Cache storing raw host programs.
        /// </summary>
        private CacheCollection _hostProgramCache;

        /// <summary>
        /// Version of the guest cache shader (to increment when guest cache structure change).
        /// </summary>
        private const ulong GuestCacheVersion = 1717;

        /// <summary>
        /// Create a new cache manager instance
        /// </summary>
        /// <param name="graphicsApi">The graphics api in use</param>
        /// <param name="hashType">The hash type in use for the cache</param>
        /// <param name="shaderProvider">The name of the codegen provider</param>
        /// <param name="titleId">The guest application title ID</param>
        /// <param name="shaderCodeGenVersion">Version of the codegen</param>
        public CacheManager(CacheGraphicsApi graphicsApi, CacheHashType hashType, string shaderProvider, string titleId, ulong shaderCodeGenVersion)
        {
            _graphicsApi = graphicsApi;
            _hashType = hashType;
            _shaderProvider = shaderProvider;

            string baseCacheDirectory = Path.Combine(AppDataManager.GamesDirPath, titleId, "cache", "shader");

            _guestProgramCache = new CacheCollection(baseCacheDirectory, _hashType, CacheGraphicsApi.Guest, "", "program", GuestCacheVersion);
            _hostProgramCache = new CacheCollection(baseCacheDirectory, _hashType, _graphicsApi, _shaderProvider, "host", shaderCodeGenVersion);
        }


        /// <summary>
        /// Entries to remove from the manifest.
        /// </summary>
        /// <param name="entries">Entries to remove from the manifest of all caches</param>
        public void RemoveManifestEntries(HashSet<Hash128> entries)
        {
            _guestProgramCache.RemoveManifestEntriesAsync(entries);
            _hostProgramCache.RemoveManifestEntriesAsync(entries);
        }

        /// <summary>
        /// Queue a task to flush temporary files to the archives.
        /// </summary>
        public void FlushToArchive()
        {
            _guestProgramCache.FlushToArchiveAsync();
            _hostProgramCache.FlushToArchiveAsync();
        }

        /// <summary>
        /// Wait for all tasks before this given point to be done.
        /// </summary>
        public void Synchronize()
        {
            _guestProgramCache.Synchronize();
            _hostProgramCache.Synchronize();
        }

        /// <summary>
        /// Computes the hash of some data using the current cache hashing algorithm.
        /// </summary>
        /// <param name="data">Some data to generate a hash for.</param>
        /// <returns>The hash of some data using the current hashing algorithm of the cache</returns>
        public Hash128 ComputeHash(ReadOnlySpan<byte> data)
        {
            return XXHash128.ComputeHash(data);
        }

        /// <summary>
        /// Save a shader program not present in the program cache.
        /// </summary>
        /// <param name="programCodeHash">Target program code hash</param>
        /// <param name="guestProgram">Guest program raw data</param>
        /// <param name="hostProgram">Host program raw data</param>
        public void SaveProgram(ref Hash128 programCodeHash, byte[] guestProgram, byte[] hostProgram)
        {
            _guestProgramCache.AddValue(ref programCodeHash, guestProgram);
            _hostProgramCache.AddValue(ref programCodeHash, hostProgram);
        }

        /// <summary>
        /// Add a host shader program not present in the program cache.
        /// </summary>
        /// <param name="programCodeHash">Target program code hash</param>
        /// <param name="data">Host program raw data</param>
        public void AddHostProgram(ref Hash128 programCodeHash, byte[] data)
        {
            _hostProgramCache.AddValue(ref programCodeHash, data);
        }

        /// <summary>
        /// Replace a host shader program present in the program cache.
        /// </summary>
        /// <param name="programCodeHash">Target program code hash</param>
        /// <param name="data">Host program raw data</param>
        public void ReplaceHostProgram(ref Hash128 programCodeHash, byte[] data)
        {
            _hostProgramCache.ReplaceValue(ref programCodeHash, data);
        }

        /// <summary>
        /// Get all guest program hashes.
        /// </summary>
        /// <returns>All guest program hashes</returns>
        public ReadOnlySpan<Hash128> GetGuestProgramList()
        {
            return _guestProgramCache.HashTable;
        }

        /// <summary>
        /// Get a host program by hash.
        /// </summary>
        /// <param name="hash">The given hash</param>
        /// <returns>The host program if present or null</returns>
        public byte[] GetHostProgramByHash(ref Hash128 hash)
        {
            return _hostProgramCache.GetValueRaw(ref hash);
        }

        /// <summary>
        /// Get a guest program by hash.
        /// </summary>
        /// <param name="hash">The given hash</param>
        /// <returns>The guest program if present or null</returns>
        public byte[] GetGuestProgramByHash(ref Hash128 hash)
        {
            return _guestProgramCache.GetValueRaw(ref hash);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _guestProgramCache.Dispose();
                _hostProgramCache.Dispose();
            }
        }
    }
}
