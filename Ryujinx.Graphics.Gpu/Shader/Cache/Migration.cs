using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using Ryujinx.Graphics.Gpu.Shader.DiskCache;
using Ryujinx.Graphics.Shader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    /// <summary>
    /// Class handling shader cache migrations.
    /// </summary>
    static class Migration
    {
        // Last codegen version before the migration to the new cache.
        private const ulong ShaderCodeGenVersion = 3054;

        /// <summary>
        /// Migrates from the old cache format to the new one.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="hostStorage">Disk cache host storage (used to create the new shader files)</param>
        /// <returns>Number of migrated shaders</returns>
        public static int MigrateFromLegacyCache(GpuContext context, DiskCacheHostStorage hostStorage)
        {
            string baseCacheDirectory = CacheHelper.GetBaseCacheDirectory(GraphicsConfig.TitleId);
            string cacheDirectory = CacheHelper.GenerateCachePath(baseCacheDirectory, CacheGraphicsApi.Guest, "", "program");

            // If the directory does not exist, we have no old cache.
            // Exist early as the CacheManager constructor will create the directories.
            if (!Directory.Exists(cacheDirectory))
            {
                return 0;
            }

            if (GraphicsConfig.EnableShaderCache && GraphicsConfig.TitleId != null)
            {
                CacheManager cacheManager = new CacheManager(CacheGraphicsApi.OpenGL, CacheHashType.XxHash128, "glsl", GraphicsConfig.TitleId, ShaderCodeGenVersion);

                bool isReadOnly = cacheManager.IsReadOnly;

                HashSet<Hash128> invalidEntries = null;

                if (isReadOnly)
                {
                    Logger.Warning?.Print(LogClass.Gpu, "Loading shader cache in read-only mode (cache in use by another program!)");
                }
                else
                {
                    invalidEntries = new HashSet<Hash128>();
                }

                ReadOnlySpan<Hash128> guestProgramList = cacheManager.GetGuestProgramList();

                for (int programIndex = 0; programIndex < guestProgramList.Length; programIndex++)
                {
                    Hash128 key = guestProgramList[programIndex];

                    byte[] guestProgram = cacheManager.GetGuestProgramByHash(ref key);

                    if (guestProgram == null)
                    {
                        Logger.Error?.Print(LogClass.Gpu, $"Ignoring orphan shader hash {key} in cache (is the cache incomplete?)");

                        continue;
                    }

                    ReadOnlySpan<byte> guestProgramReadOnlySpan = guestProgram;

                    ReadOnlySpan<GuestShaderCacheEntry> cachedShaderEntries = GuestShaderCacheEntry.Parse(ref guestProgramReadOnlySpan, out GuestShaderCacheHeader fileHeader);

                    if (cachedShaderEntries[0].Header.Stage == ShaderStage.Compute)
                    {
                        Debug.Assert(cachedShaderEntries.Length == 1);

                        GuestShaderCacheEntry entry = cachedShaderEntries[0];

                        byte[] code = entry.Code.AsSpan(0, entry.Header.Size - entry.Header.Cb1DataSize).ToArray();

                        Span<byte> codeSpan = entry.Code;
                        byte[] cb1Data = codeSpan.Slice(codeSpan.Length - entry.Header.Cb1DataSize).ToArray();

                        ShaderProgramInfo info = new ShaderProgramInfo(
                            Array.Empty<BufferDescriptor>(),
                            Array.Empty<BufferDescriptor>(),
                            Array.Empty<TextureDescriptor>(),
                            Array.Empty<TextureDescriptor>(),
                            ShaderStage.Compute,
                            false,
                            false,
                            0,
                            0);

                        GpuChannelComputeState computeState = new GpuChannelComputeState(
                            entry.Header.GpuAccessorHeader.ComputeLocalSizeX,
                            entry.Header.GpuAccessorHeader.ComputeLocalSizeY,
                            entry.Header.GpuAccessorHeader.ComputeLocalSizeZ,
                            entry.Header.GpuAccessorHeader.ComputeLocalMemorySize,
                            entry.Header.GpuAccessorHeader.ComputeSharedMemorySize);

                        ShaderSpecializationState specState = new ShaderSpecializationState(computeState);

                        foreach (var td in entry.TextureDescriptors)
                        {
                            var handle = td.Key;
                            var data = td.Value;

                            specState.RegisterTexture(
                                0,
                                handle,
                                -1,
                                data.UnpackFormat(),
                                data.UnpackSrgb(),
                                data.UnpackTextureTarget(),
                                data.UnpackTextureCoordNormalized());
                        }

                        CachedShaderStage shader = new CachedShaderStage(info, code, cb1Data);
                        CachedShaderProgram program = new CachedShaderProgram(null, specState, shader);

                        hostStorage.AddShader(context, program, ReadOnlySpan<byte>.Empty);
                    }
                    else
                    {
                        Debug.Assert(cachedShaderEntries.Length == Constants.ShaderStages);

                        CachedShaderStage[] shaders = new CachedShaderStage[Constants.ShaderStages + 1];
                        List<ShaderProgram> shaderPrograms = new List<ShaderProgram>();

                        TransformFeedbackDescriptorOld[] tfd = CacheHelper.ReadTransformFeedbackInformation(ref guestProgramReadOnlySpan, fileHeader);

                        GuestShaderCacheEntry[] entries = cachedShaderEntries.ToArray();

                        GuestGpuAccessorHeader accessorHeader = entries[0].Header.GpuAccessorHeader;

                        TessMode tessMode = new TessMode();

                        int tessPatchType = accessorHeader.TessellationModePacked & 3;
                        int tessSpacing = (accessorHeader.TessellationModePacked >> 2) & 3;
                        bool tessCw = (accessorHeader.TessellationModePacked & 0x10) != 0;

                        tessMode.Packed = (uint)tessPatchType;
                        tessMode.Packed |= (uint)(tessSpacing << 4);

                        if (tessCw)
                        {
                            tessMode.Packed |= 0x100;
                        }

                        PrimitiveTopology topology = accessorHeader.PrimitiveTopology switch
                        {
                            InputTopology.Lines => PrimitiveTopology.Lines,
                            InputTopology.LinesAdjacency => PrimitiveTopology.LinesAdjacency,
                            InputTopology.Triangles => PrimitiveTopology.Triangles,
                            InputTopology.TrianglesAdjacency => PrimitiveTopology.TrianglesAdjacency,
                            _ => PrimitiveTopology.Points
                        };

                        GpuChannelGraphicsState graphicsState = new GpuChannelGraphicsState(
                            accessorHeader.StateFlags.HasFlag(GuestGpuStateFlags.EarlyZForce),
                            topology,
                            tessMode);

                        TransformFeedbackDescriptor[] tfdNew = null;

                        if (tfd != null)
                        {
                            tfdNew = new TransformFeedbackDescriptor[tfd.Length];

                            for (int tfIndex = 0; tfIndex < tfd.Length; tfIndex++)
                            {
                                Array32<uint> varyingLocations = new Array32<uint>();
                                Span<byte> varyingLocationsSpan = MemoryMarshal.Cast<uint, byte>(varyingLocations.ToSpan());
                                tfd[tfIndex].VaryingLocations.CopyTo(varyingLocationsSpan.Slice(0, tfd[tfIndex].VaryingLocations.Length));

                                tfdNew[tfIndex] = new TransformFeedbackDescriptor(
                                    tfd[tfIndex].BufferIndex,
                                    tfd[tfIndex].Stride,
                                    tfd[tfIndex].VaryingLocations.Length,
                                    ref varyingLocations);
                            }
                        }

                        ShaderSpecializationState specState = new ShaderSpecializationState(graphicsState, tfdNew);

                        for (int i = 0; i < entries.Length; i++)
                        {
                            GuestShaderCacheEntry entry = entries[i];

                            if (entry == null)
                            {
                                continue;
                            }

                            ShaderProgramInfo info = new ShaderProgramInfo(
                                Array.Empty<BufferDescriptor>(),
                                Array.Empty<BufferDescriptor>(),
                                Array.Empty<TextureDescriptor>(),
                                Array.Empty<TextureDescriptor>(),
                                (ShaderStage)(i + 1),
                                false,
                                false,
                                0,
                                0);

                            // NOTE: Vertex B comes first in the shader cache.
                            byte[] code = entry.Code.AsSpan(0, entry.Header.Size - entry.Header.Cb1DataSize).ToArray();
                            byte[] code2 = entry.Header.SizeA != 0 ? entry.Code.AsSpan(entry.Header.Size, entry.Header.SizeA).ToArray() : null;

                            Span<byte> codeSpan = entry.Code;
                            byte[] cb1Data = codeSpan.Slice(codeSpan.Length - entry.Header.Cb1DataSize).ToArray();

                            shaders[i + 1] = new CachedShaderStage(info, code, cb1Data);

                            if (code2 != null)
                            {
                                shaders[0] = new CachedShaderStage(null, code2, cb1Data);
                            }

                            foreach (var td in entry.TextureDescriptors)
                            {
                                var handle = td.Key;
                                var data = td.Value;

                                specState.RegisterTexture(
                                    i,
                                    handle,
                                    -1,
                                    data.UnpackFormat(),
                                    data.UnpackSrgb(),
                                    data.UnpackTextureTarget(),
                                    data.UnpackTextureCoordNormalized());
                            }
                        }

                        CachedShaderProgram program = new CachedShaderProgram(null, specState, shaders);

                        hostStorage.AddShader(context, program, ReadOnlySpan<byte>.Empty);
                    }
                }

                return guestProgramList.Length;
            }

            return 0;
        }
    }
}