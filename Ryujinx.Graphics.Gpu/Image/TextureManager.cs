using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture manager.
    /// </summary>
    class TextureManager : IDisposable
    {
        private const int OverlapsBufferInitialCapacity = 10;
        private const int OverlapsBufferMaxCapacity     = 10000;

        private readonly GpuContext _context;

        private readonly TextureBindingsManager _cpBindingsManager;
        private readonly TextureBindingsManager _gpBindingsManager;

        private readonly Texture[] _rtColors;

        private Texture _rtDepthStencil;

        private readonly ITexture[] _rtHostColors;

        private ITexture _rtHostDs;

        private readonly RangeList<Texture> _textures;

        private Texture[] _textureOverlaps;

        private readonly AutoDeleteCache _cache;

        private readonly HashSet<Texture> _modified;
        private readonly HashSet<Texture> _modifiedLinear;

        /// <summary>
        /// Constructs a new instance of the texture manager.
        /// </summary>
        /// <param name="context">The GPU context that the texture manager belongs to</param>
        public TextureManager(GpuContext context)
        {
            _context = context;

            TexturePoolCache texturePoolCache = new TexturePoolCache(context);

            _cpBindingsManager = new TextureBindingsManager(context, texturePoolCache, isCompute: true);
            _gpBindingsManager = new TextureBindingsManager(context, texturePoolCache, isCompute: false);

            _rtColors = new Texture[Constants.TotalRenderTargets];

            _rtHostColors = new ITexture[Constants.TotalRenderTargets];

            _textures = new RangeList<Texture>();

            _textureOverlaps = new Texture[OverlapsBufferInitialCapacity];

            _cache = new AutoDeleteCache();

            _modified = new HashSet<Texture>(new ReferenceEqualityComparer<Texture>());
            _modifiedLinear = new HashSet<Texture>(new ReferenceEqualityComparer<Texture>());
        }

        /// <summary>
        /// Sets texture bindings on the compute pipeline.
        /// </summary>
        /// <param name="bindings">The texture bindings</param>
        public void SetComputeTextures(TextureBindingInfo[] bindings)
        {
            _cpBindingsManager.SetTextures(0, bindings);
        }

        /// <summary>
        /// Sets texture bindings on the graphics pipeline.
        /// </summary>
        /// <param name="stage">The index of the shader stage to bind the textures</param>
        /// <param name="bindings">The texture bindings</param>
        public void SetGraphicsTextures(int stage, TextureBindingInfo[] bindings)
        {
            _gpBindingsManager.SetTextures(stage, bindings);
        }

        /// <summary>
        /// Sets image bindings on the compute pipeline.
        /// </summary>
        /// <param name="bindings">The image bindings</param>
        public void SetComputeImages(TextureBindingInfo[] bindings)
        {
            _cpBindingsManager.SetImages(0, bindings);
        }

        /// <summary>
        /// Sets image bindings on the graphics pipeline.
        /// </summary>
        /// <param name="stage">The index of the shader stage to bind the images</param>
        /// <param name="bindings">The image bindings</param>
        public void SetGraphicsImages(int stage, TextureBindingInfo[] bindings)
        {
            _gpBindingsManager.SetImages(stage, bindings);
        }

        /// <summary>
        /// Sets the texture constant buffer index on the compute pipeline.
        /// </summary>
        /// <param name="index">The texture constant buffer index</param>
        public void SetComputeTextureBufferIndex(int index)
        {
            _cpBindingsManager.SetTextureBufferIndex(index);
        }

        /// <summary>
        /// Sets the texture constant buffer index on the graphics pipeline.
        /// </summary>
        /// <param name="index">The texture constant buffer index</param>
        public void SetGraphicsTextureBufferIndex(int index)
        {
            _gpBindingsManager.SetTextureBufferIndex(index);
        }

        /// <summary>
        /// Sets the current sampler pool on the compute pipeline.
        /// </summary>
        /// <param name="gpuVa">The start GPU virtual address of the sampler pool</param>
        /// <param name="maximumId">The maximum ID of the sampler pool</param>
        /// <param name="samplerIndex">The indexing type of the sampler pool</param>
        public void SetComputeSamplerPool(ulong gpuVa, int maximumId, SamplerIndex samplerIndex)
        {
            _cpBindingsManager.SetSamplerPool(gpuVa, maximumId, samplerIndex);
        }

        /// <summary>
        /// Sets the current sampler pool on the graphics pipeline.
        /// </summary>
        /// <param name="gpuVa">The start GPU virtual address of the sampler pool</param>
        /// <param name="maximumId">The maximum ID of the sampler pool</param>
        /// <param name="samplerIndex">The indexing type of the sampler pool</param>
        public void SetGraphicsSamplerPool(ulong gpuVa, int maximumId, SamplerIndex samplerIndex)
        {
            _gpBindingsManager.SetSamplerPool(gpuVa, maximumId, samplerIndex);
        }

        /// <summary>
        /// Sets the current texture pool on the compute pipeline.
        /// </summary>
        /// <param name="gpuVa">The start GPU virtual address of the texture pool</param>
        /// <param name="maximumId">The maximum ID of the texture pool</param>
        public void SetComputeTexturePool(ulong gpuVa, int maximumId)
        {
            _cpBindingsManager.SetTexturePool(gpuVa, maximumId);
        }

        /// <summary>
        /// Sets the current texture pool on the graphics pipeline.
        /// </summary>
        /// <param name="gpuVa">The start GPU virtual address of the texture pool</param>
        /// <param name="maximumId">The maximum ID of the texture pool</param>
        public void SetGraphicsTexturePool(ulong gpuVa, int maximumId)
        {
            _gpBindingsManager.SetTexturePool(gpuVa, maximumId);
        }

        /// <summary>
        /// Sets the render target color buffer.
        /// </summary>
        /// <param name="index">The index of the color buffer to set (up to 8)</param>
        /// <param name="color">The color buffer texture</param>
        public void SetRenderTargetColor(int index, Texture color)
        {
            _rtColors[index] = color;
        }

        /// <summary>
        /// Sets the render target depth-stencil buffer.
        /// </summary>
        /// <param name="depthStencil">The depth-stencil buffer texture</param>
        public void SetRenderTargetDepthStencil(Texture depthStencil)
        {
            _rtDepthStencil = depthStencil;
        }

        /// <summary>
        /// Commits bindings on the compute pipeline.
        /// </summary>
        public void CommitComputeBindings()
        {
            // Every time we switch between graphics and compute work,
            // we must rebind everything.
            // Since compute work happens less often, we always do that
            // before and after the compute dispatch.
            _cpBindingsManager.Rebind();
            _cpBindingsManager.CommitBindings();
            _gpBindingsManager.Rebind();
        }

        /// <summary>
        /// Commits bindings on the graphics pipeline.
        /// </summary>
        public void CommitGraphicsBindings()
        {
            _gpBindingsManager.CommitBindings();

            UpdateRenderTargets();
        }

        /// <summary>
        /// Gets a texture descriptor used on the compute pipeline.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="handle">Shader "fake" handle of the texture</param>
        /// <returns>The texture descriptor</returns>
        public TextureDescriptor GetComputeTextureDescriptor(GpuState state, int handle)
        {
            return _cpBindingsManager.GetTextureDescriptor(state, 0, handle);
        }

        /// <summary>
        /// Gets a texture descriptor used on the graphics pipeline.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage where the texture is bound</param>
        /// <param name="handle">Shader "fake" handle of the texture</param>
        /// <returns>The texture descriptor</returns>
        public TextureDescriptor GetGraphicsTextureDescriptor(GpuState state, int stageIndex, int handle)
        {
            return _gpBindingsManager.GetTextureDescriptor(state, stageIndex, handle);
        }

        /// <summary>
        /// Update host framebuffer attachments based on currently bound render target buffers.
        /// </summary>
        private void UpdateRenderTargets()
        {
            bool anyChanged = false;

            if (_rtHostDs != _rtDepthStencil?.HostTexture)
            {
                _rtHostDs = _rtDepthStencil?.HostTexture;

                anyChanged = true;
            }

            for (int index = 0; index < _rtColors.Length; index++)
            {
                ITexture hostTexture = _rtColors[index]?.HostTexture;

                if (_rtHostColors[index] != hostTexture)
                {
                    _rtHostColors[index] = hostTexture;

                    anyChanged = true;
                }
            }

            if (anyChanged)
            {
                _context.Renderer.Pipeline.SetRenderTargets(_rtHostColors, _rtHostDs);
            }
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="copyTexture">Copy texture to find or create</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(CopyTexture copyTexture)
        {
            ulong address = _context.MemoryManager.Translate(copyTexture.Address.Pack());

            if (address == MemoryManager.BadAddress)
            {
                return null;
            }

            int gobBlocksInY = copyTexture.MemoryLayout.UnpackGobBlocksInY();
            int gobBlocksInZ = copyTexture.MemoryLayout.UnpackGobBlocksInZ();

            FormatInfo formatInfo = copyTexture.Format.Convert();

            int width;

            if (copyTexture.LinearLayout)
            {
                width = copyTexture.Stride / formatInfo.BytesPerPixel;
            }
            else
            {
                width = copyTexture.Width;
            }

            TextureInfo info = new TextureInfo(
                address,
                width,
                copyTexture.Height,
                copyTexture.Depth,
                1,
                1,
                1,
                copyTexture.Stride,
                copyTexture.LinearLayout,
                gobBlocksInY,
                gobBlocksInZ,
                1,
                Target.Texture2D,
                formatInfo);

            Texture texture = FindOrCreateTexture(info, TextureSearchFlags.IgnoreMs);

            texture.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="colorState">Color buffer texture to find or create</param>
        /// <param name="samplesInX">Number of samples in the X direction, for MSAA</param>
        /// <param name="samplesInY">Number of samples in the Y direction, for MSAA</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(RtColorState colorState, int samplesInX, int samplesInY)
        {
            ulong address = _context.MemoryManager.Translate(colorState.Address.Pack());

            if (address == MemoryManager.BadAddress)
            {
                return null;
            }

            bool isLinear = colorState.MemoryLayout.UnpackIsLinear();

            int gobBlocksInY = colorState.MemoryLayout.UnpackGobBlocksInY();
            int gobBlocksInZ = colorState.MemoryLayout.UnpackGobBlocksInZ();

            Target target;

            if (colorState.MemoryLayout.UnpackIsTarget3D())
            {
                target = Target.Texture3D;
            }
            else if ((samplesInX | samplesInY) != 1)
            {
                target = colorState.Depth > 1
                    ? Target.Texture2DMultisampleArray
                    : Target.Texture2DMultisample;
            }
            else
            {
                target = colorState.Depth > 1
                    ? Target.Texture2DArray
                    : Target.Texture2D;
            }

            FormatInfo formatInfo = colorState.Format.Convert();

            int width, stride;

            // For linear textures, the width value is actually the stride.
            // We can easily get the width by dividing the stride by the bpp,
            // since the stride is the total number of bytes occupied by a
            // line. The stride should also meet alignment constraints however,
            // so the width we get here is the aligned width.
            if (isLinear)
            {
                width  = colorState.WidthOrStride / formatInfo.BytesPerPixel;
                stride = colorState.WidthOrStride;
            }
            else
            {
                width  = colorState.WidthOrStride;
                stride = 0;
            }

            TextureInfo info = new TextureInfo(
                address,
                width,
                colorState.Height,
                colorState.Depth,
                1,
                samplesInX,
                samplesInY,
                stride,
                isLinear,
                gobBlocksInY,
                gobBlocksInZ,
                1,
                target,
                formatInfo);

            Texture texture = FindOrCreateTexture(info);

            texture.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="dsState">Depth-stencil buffer texture to find or create</param>
        /// <param name="size">Size of the depth-stencil texture</param>
        /// <param name="samplesInX">Number of samples in the X direction, for MSAA</param>
        /// <param name="samplesInY">Number of samples in the Y direction, for MSAA</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(RtDepthStencilState dsState, Size3D size, int samplesInX, int samplesInY)
        {
            ulong address = _context.MemoryManager.Translate(dsState.Address.Pack());

            if (address == MemoryManager.BadAddress)
            {
                return null;
            }

            int gobBlocksInY = dsState.MemoryLayout.UnpackGobBlocksInY();
            int gobBlocksInZ = dsState.MemoryLayout.UnpackGobBlocksInZ();

            Target target = (samplesInX | samplesInY) != 1
                ? Target.Texture2DMultisample
                : Target.Texture2D;

            FormatInfo formatInfo = dsState.Format.Convert();

            TextureInfo info = new TextureInfo(
                address,
                size.Width,
                size.Height,
                size.Depth,
                1,
                samplesInX,
                samplesInY,
                0,
                false,
                gobBlocksInY,
                gobBlocksInZ,
                1,
                target,
                formatInfo);

            Texture texture = FindOrCreateTexture(info);

            texture.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="info">Texture information of the texture to be found or created</param>
        /// <param name="flags">The texture search flags, defines texture comparison rules</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(TextureInfo info, TextureSearchFlags flags = TextureSearchFlags.None)
        {
            bool isSamplerTexture = (flags & TextureSearchFlags.Sampler) != 0;

            // Try to find a perfect texture match, with the same address and parameters.
            int sameAddressOverlapsCount = _textures.FindOverlaps(info.Address, ref _textureOverlaps);

            for (int index = 0; index < sameAddressOverlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];

                if (overlap.IsPerfectMatch(info, flags))
                {
                    if (!isSamplerTexture)
                    {
                        // If not a sampler texture, it is managed by the auto delete
                        // cache, ensure that it is on the "top" of the list to avoid
                        // deletion.
                        _cache.Lift(overlap);
                    }
                    else if (!overlap.SizeMatches(info))
                    {
                        // If this is used for sampling, the size must match,
                        // otherwise the shader would sample garbage data.
                        // To fix that, we create a new texture with the correct
                        // size, and copy the data from the old one to the new one.
                        overlap.ChangeSize(info.Width, info.Height, info.DepthOrLayers);
                    }

                    return overlap;
                }
            }

            // Calculate texture sizes, used to find all overlapping textures.
            SizeInfo sizeInfo;

            if (info.Target == Target.TextureBuffer)
            {
                sizeInfo = new SizeInfo(info.Width * info.FormatInfo.BytesPerPixel);
            }
            else if (info.IsLinear)
            {
                sizeInfo = SizeCalculator.GetLinearTextureSize(
                    info.Stride,
                    info.Height,
                    info.FormatInfo.BlockHeight);
            }
            else
            {
                sizeInfo = SizeCalculator.GetBlockLinearTextureSize(
                    info.Width,
                    info.Height,
                    info.GetDepth(),
                    info.Levels,
                    info.GetLayers(),
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel,
                    info.GobBlocksInY,
                    info.GobBlocksInZ,
                    info.GobBlocksInTileX);
            }

            // Find view compatible matches.
            ulong size = (ulong)sizeInfo.TotalSize;

            int overlapsCount = _textures.FindOverlaps(info.Address, size, ref _textureOverlaps);

            Texture texture = null;

            for (int index = 0; index < overlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];

                if (overlap.IsViewCompatible(info, size, out int firstLayer, out int firstLevel))
                {
                    if (!isSamplerTexture)
                    {
                        info = AdjustSizes(overlap, info, firstLevel);
                    }

                    texture = overlap.CreateView(info, sizeInfo, firstLayer, firstLevel);

                    if (IsTextureModified(overlap))
                    {
                        CacheTextureModified(texture);
                    }

                    // The size only matters (and is only really reliable) when the
                    // texture is used on a sampler, because otherwise the size will be
                    // aligned.
                    if (!overlap.SizeMatches(info, firstLevel) && isSamplerTexture)
                    {
                        texture.ChangeSize(info.Width, info.Height, info.DepthOrLayers);
                    }

                    break;
                }
            }

            // No match, create a new texture.
            if (texture == null)
            {
                texture = new Texture(_context, info, sizeInfo);

                // We need to synchronize before copying the old view data to the texture,
                // otherwise the copied data would be overwritten by a future synchronization.
                texture.SynchronizeMemory();

                for (int index = 0; index < overlapsCount; index++)
                {
                    Texture overlap = _textureOverlaps[index];

                    if (texture.IsViewCompatible(overlap.Info, overlap.Size, out int firstLayer, out int firstLevel))
                    {
                        TextureInfo overlapInfo = AdjustSizes(texture, overlap.Info, firstLevel);

                        TextureCreateInfo createInfo = GetCreateInfo(overlapInfo, _context.Capabilities);

                        ITexture newView = texture.HostTexture.CreateView(createInfo, firstLayer, firstLevel);

                        overlap.HostTexture.CopyTo(newView, 0, 0);

                        // Inherit modification from overlapping texture, do that before replacing
                        // the view since the replacement operation removes it from the list.
                        if (IsTextureModified(overlap))
                        {
                            CacheTextureModified(texture);
                        }

                        overlap.ReplaceView(texture, overlapInfo, newView);
                    }
                }

                // If the texture is a 3D texture, we need to additionally copy any slice
                // of the 3D texture to the newly created 3D texture.
                if (info.Target == Target.Texture3D)
                {
                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Texture overlap = _textureOverlaps[index];

                        if (texture.IsViewCompatible(
                            overlap.Info,
                            overlap.Size,
                            isCopy: true,
                            out int firstLayer,
                            out int firstLevel))
                        {
                            overlap.HostTexture.CopyTo(texture.HostTexture, firstLayer, firstLevel);

                            if (IsTextureModified(overlap))
                            {
                                CacheTextureModified(texture);
                            }
                        }
                    }
                }
            }

            // Sampler textures are managed by the texture pool, all other textures
            // are managed by the auto delete cache.
            if (!isSamplerTexture)
            {
                _cache.Add(texture);
                texture.Modified += CacheTextureModified;
                texture.Disposed += CacheTextureDisposed;
            }

            _textures.Add(texture);

            ShrinkOverlapsBufferIfNeeded();

            return texture;
        }

        /// <summary>
        /// Checks if a texture was modified by the host GPU.
        /// </summary>
        /// <param name="texture">Texture to be checked</param>
        /// <returns>True if the texture was modified by the host GPU, false otherwise</returns>
        public bool IsTextureModified(Texture texture)
        {
            return _modified.Contains(texture);
        }

        /// <summary>
        /// Signaled when a cache texture is modified, and adds it to a set to be enumerated when flushing textures.
        /// </summary>
        /// <param name="texture">The texture that was modified.</param>
        private void CacheTextureModified(Texture texture)
        {
            _modified.Add(texture);

            if (texture.Info.IsLinear)
            {
                _modifiedLinear.Add(texture);
            }
        }

        /// <summary>
        /// Signaled when a cache texture is disposed, so it can be removed from the set of modified textures if present.
        /// </summary>
        /// <param name="texture">The texture that was diosposed.</param>
        private void CacheTextureDisposed(Texture texture)
        {
            _modified.Remove(texture);

            if (texture.Info.IsLinear)
            {
                _modifiedLinear.Remove(texture);
            }
        }

        /// <summary>
        /// Resizes the temporary buffer used for range list intersection results, if it has grown too much.
        /// </summary>
        private void ShrinkOverlapsBufferIfNeeded()
        {
            if (_textureOverlaps.Length > OverlapsBufferMaxCapacity)
            {
                Array.Resize(ref _textureOverlaps, OverlapsBufferMaxCapacity);
            }
        }

        /// <summary>
        /// Adjusts the size of the texture information for a given mipmap level,
        /// based on the size of a parent texture.
        /// </summary>
        /// <param name="parent">The parent texture</param>
        /// <param name="info">The texture information to be adjusted</param>
        /// <param name="firstLevel">The first level of the texture view</param>
        /// <returns>The adjusted texture information with the new size</returns>
        private static TextureInfo AdjustSizes(Texture parent, TextureInfo info, int firstLevel)
        {
            // When the texture is used as view of another texture, we must
            // ensure that the sizes are valid, otherwise data uploads would fail
            // (and the size wouldn't match the real size used on the host API).
            // Given a parent texture from where the view is created, we have the
            // following rules:
            // - The view size must be equal to the parent size, divided by (2 ^ l),
            // where l is the first mipmap level of the view. The division result must
            // be rounded down, and the result must be clamped to 1.
            // - If the parent format is compressed, and the view format isn't, the
            // view size is calculated as above, but the width and height of the
            // view must be also divided by the compressed format block width and height.
            // - If the parent format is not compressed, and the view is, the view
            // size is calculated as described on the first point, but the width and height
            // of the view must be also multiplied by the block width and height.
            int width  = Math.Max(1, parent.Info.Width  >> firstLevel);
            int height = Math.Max(1, parent.Info.Height >> firstLevel);

            if (parent.Info.FormatInfo.IsCompressed && !info.FormatInfo.IsCompressed)
            {
                width  = BitUtils.DivRoundUp(width,  parent.Info.FormatInfo.BlockWidth);
                height = BitUtils.DivRoundUp(height, parent.Info.FormatInfo.BlockHeight);
            }
            else if (!parent.Info.FormatInfo.IsCompressed && info.FormatInfo.IsCompressed)
            {
                width  *= info.FormatInfo.BlockWidth;
                height *= info.FormatInfo.BlockHeight;
            }

            int depthOrLayers;

            if (info.Target == Target.Texture3D)
            {
                depthOrLayers = Math.Max(1, parent.Info.DepthOrLayers >> firstLevel);
            }
            else
            {
                depthOrLayers = info.DepthOrLayers;
            }

            return new TextureInfo(
                info.Address,
                width,
                height,
                depthOrLayers,
                info.Levels,
                info.SamplesInX,
                info.SamplesInY,
                info.Stride,
                info.IsLinear,
                info.GobBlocksInY,
                info.GobBlocksInZ,
                info.GobBlocksInTileX,
                info.Target,
                info.FormatInfo,
                info.DepthStencilMode,
                info.SwizzleR,
                info.SwizzleG,
                info.SwizzleB,
                info.SwizzleA);
        }


        /// <summary>
        /// Gets a texture creation information from texture information.
        /// This can be used to create new host textures.
        /// </summary>
        /// <param name="info">Texture information</param>
        /// <param name="caps">GPU capabilities</param>
        /// <returns>The texture creation information</returns>
        public static TextureCreateInfo GetCreateInfo(TextureInfo info, Capabilities caps)
        {
            FormatInfo formatInfo = info.FormatInfo;

            if (!caps.SupportsAstcCompression)
            {
                if (formatInfo.Format.IsAstcUnorm())
                {
                    formatInfo = new FormatInfo(Format.R8G8B8A8Unorm, 1, 1, 4);
                }
                else if (formatInfo.Format.IsAstcSrgb())
                {
                    formatInfo = new FormatInfo(Format.R8G8B8A8Srgb, 1, 1, 4);
                }
            }

            int width  = info.Width  / info.SamplesInX;
            int height = info.Height / info.SamplesInY;

            int depth = info.GetDepth() * info.GetLayers();

            return new TextureCreateInfo(
                width,
                height,
                depth,
                info.Levels,
                info.Samples,
                formatInfo.BlockWidth,
                formatInfo.BlockHeight,
                formatInfo.BytesPerPixel,
                formatInfo.Format,
                info.DepthStencilMode,
                info.Target,
                info.SwizzleR,
                info.SwizzleG,
                info.SwizzleB,
                info.SwizzleA);
        }

        /// <summary>
        /// Flushes all the textures in the cache that have been modified since the last call.
        /// </summary>
        public void Flush()
        {
            foreach (Texture texture in _modifiedLinear)
            {
                texture.Flush();
            }

            _modifiedLinear.Clear();
        }

        /// <summary>
        /// Flushes the textures in the cache inside a given range that have been modified since the last call.
        /// </summary>
        /// <param name="address">The range start address</param>
        /// <param name="size">The range size</param>
        public void Flush(ulong address, ulong size)
        {
            foreach (Texture texture in _modified)
            {
                if (texture.OverlapsWith(address, size))
                {
                    texture.Flush();
                }
            }
        }

        /// <summary>
        /// Removes a texture from the cache.
        /// </summary>
        /// <remarks>
        /// This only removes the texture from the internal list, not from the auto-deletion cache.
        /// It may still have live references after the removal.
        /// </remarks>
        /// <param name="texture">The texture to be removed</param>
        public void RemoveTextureFromCache(Texture texture)
        {
            _textures.Remove(texture);
        }

        /// <summary>
        /// Disposes all textures in the cache.
        /// It's an error to use the texture manager after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (Texture texture in _textures)
            {
                _modified.Remove(texture);
                texture.Dispose();
            }
        }
    }
}