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
        private struct OverlapInfo
        {
            public TextureViewCompatibility Compatibility { get; }
            public int FirstLayer { get; }
            public int FirstLevel { get; }

            public OverlapInfo(TextureViewCompatibility compatibility, int firstLayer, int firstLevel)
            {
                Compatibility = compatibility;
                FirstLayer = firstLayer;
                FirstLevel = firstLevel;
            }
        }

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
        private OverlapInfo[] _overlapInfo;

        private readonly AutoDeleteCache _cache;

        private readonly HashSet<Texture> _modified;
        private readonly HashSet<Texture> _modifiedLinear;

        /// <summary>
        /// The scaling factor applied to all currently bound render targets.
        /// </summary>
        public float RenderTargetScale { get; private set; } = 1f;

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
            _overlapInfo = new OverlapInfo[OverlapsBufferInitialCapacity];

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
        /// <returns>True if render target scale must be updated.</returns>
        public bool SetRenderTargetColor(int index, Texture color)
        {
            bool hasValue = color != null;
            bool changesScale = (hasValue != (_rtColors[index] != null)) || (hasValue && RenderTargetScale != color.ScaleFactor);
            _rtColors[index] = color;

            return changesScale || (hasValue && color.ScaleMode != TextureScaleMode.Blacklisted && color.ScaleFactor != GraphicsConfig.ResScale);
        }

        /// <summary>
        /// Updates the Render Target scale, given the currently bound render targets.
        /// This will update scale to match the configured scale, scale textures that are eligible but not scaled,
        /// and propagate blacklisted status from one texture to the ones bound with it.
        /// </summary>
        /// <param name="singleUse">If this is not -1, it indicates that only the given indexed target will be used.</param>
        public void UpdateRenderTargetScale(int singleUse)
        {
            // Make sure all scales for render targets are at the highest they should be. Blacklisted targets should propagate their scale to the other targets.
            bool mismatch = false;
            bool blacklisted = false;
            bool hasUpscaled = false;
            float targetScale = GraphicsConfig.ResScale;

            void ConsiderTarget(Texture target)
            {
                if (target == null) return;
                float scale = target.ScaleFactor;

                switch (target.ScaleMode)
                {
                    case TextureScaleMode.Blacklisted:
                        mismatch |= scale != 1f;
                        blacklisted = true;
                        break;
                    case TextureScaleMode.Eligible:
                        mismatch = true; // We must make a decision.
                        break;
                    case TextureScaleMode.Scaled:
                        hasUpscaled = true;
                        mismatch |= scale != targetScale; // If the target scale has changed, reset the scale for all targets.
                        break;
                }
            }

            if (singleUse != -1)
            {
                // If only one target is in use (by a clear, for example) the others do not need to be checked for mismatching scale.
                ConsiderTarget(_rtColors[singleUse]);
            }
            else
            {
                foreach (Texture color in _rtColors)
                {
                    ConsiderTarget(color);
                }
            }

            ConsiderTarget(_rtDepthStencil);

            mismatch |= blacklisted && hasUpscaled;

            if (blacklisted)
            {
                targetScale = 1f;
            }

            if (mismatch)
            {
                if (blacklisted)
                {
                    // Propagate the blacklisted state to the other textures.
                    foreach (Texture color in _rtColors)
                    {
                        color?.BlacklistScale();
                    }

                    _rtDepthStencil?.BlacklistScale();
                }
                else
                {
                    // Set the scale of the other textures.
                    foreach (Texture color in _rtColors)
                    {
                        color?.SetScale(targetScale);
                    }

                    _rtDepthStencil?.SetScale(targetScale);
                }
            }

            RenderTargetScale = targetScale;
        }

        /// <summary>
        /// Sets the render target depth-stencil buffer.
        /// </summary>
        /// <param name="depthStencil">The depth-stencil buffer texture</param>
        /// <returns>True if render target scale must be updated.</returns>
        public bool SetRenderTargetDepthStencil(Texture depthStencil)
        {
            bool hasValue = depthStencil != null;
            bool changesScale = (hasValue != (_rtDepthStencil != null)) || (hasValue && RenderTargetScale != depthStencil.ScaleFactor);
            _rtDepthStencil = depthStencil;

            return changesScale || (hasValue && depthStencil.ScaleMode != TextureScaleMode.Blacklisted && depthStencil.ScaleFactor != GraphicsConfig.ResScale);
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
        /// Determines if a given texture is eligible for upscaling from its info.
        /// </summary>
        /// <param name="info">The texture info to check</param>
        /// <returns>True if eligible</returns>
        public bool IsUpscaleCompatible(TextureInfo info)
        {
            return (info.Target == Target.Texture2D || info.Target == Target.Texture2DArray) && info.Levels == 1 && !info.FormatInfo.IsCompressed && UpscaleSafeMode(info);
        }

        /// <summary>
        /// Determines if a given texture is "safe" for upscaling from its info.
        /// Note that this is different from being compatible - this elilinates targets that would have detrimental effects when scaled.
        /// </summary>
        /// <param name="info">The texture info to check</param>
        /// <returns>True if safe</returns>
        public bool UpscaleSafeMode(TextureInfo info)
        {
            // While upscaling works for all targets defined by IsUpscaleCompatible, we additionally blacklist targets here that
            // may have undesirable results (upscaling blur textures) or simply waste GPU resources (upscaling texture atlas).

            if (!(info.FormatInfo.Format.IsDepthOrStencil() || info.FormatInfo.Components == 1))
            {
                // Discount square textures that aren't depth-stencil like. (excludes game textures, cubemap faces, most 3D texture LUT, texture atlas)
                // Detect if the texture is possibly square. Widths may be aligned, so to remove the uncertainty we align both the width and height.

                int widthAlignment = (info.IsLinear ? 32 : 64) / info.FormatInfo.BytesPerPixel;

                bool possiblySquare = BitUtils.AlignUp(info.Width, widthAlignment) == BitUtils.AlignUp(info.Height, widthAlignment);

                if (possiblySquare)
                {
                    return false;
                }
            }

            int aspect = (int)Math.Round((info.Width / (float)info.Height) * 9);
            if (aspect == 16 && info.Height < 360)
            {
                // Targets that are roughly 16:9 can only be rescaled if they're equal to or above 360p. (excludes blur and bloom textures)
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles removal of textures written to a memory region being unmapped.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        public void MemoryUnmappedHandler(object sender, UnmapEventArgs e)
        {
            Texture[] overlaps = new Texture[10];
            int overlapCount;

            lock (_textures)
            {
                overlapCount = _textures.FindOverlaps(_context.MemoryManager.Translate(e.Address), e.Size, ref overlaps);
            }

            for (int i = 0; i < overlapCount; i++)
            {
                overlaps[i].Unmapped();
            }
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="copyTexture">Copy texture to find or create</param>
        /// <param name="preferScaling">Indicates if the texture should be scaled from the start</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(CopyTexture copyTexture, bool preferScaling = true)
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

            TextureSearchFlags flags = TextureSearchFlags.ForCopy;

            if (preferScaling)
            {
                flags |= TextureSearchFlags.WithUpscale;
            }

            Texture texture = FindOrCreateTexture(info, flags);

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

            Texture texture = FindOrCreateTexture(info, TextureSearchFlags.WithUpscale);

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

            Texture texture = FindOrCreateTexture(info, TextureSearchFlags.WithUpscale);

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
            bool isSamplerTexture = (flags & TextureSearchFlags.ForSampler) != 0;

            bool isScalable = IsUpscaleCompatible(info);

            TextureScaleMode scaleMode = TextureScaleMode.Blacklisted;
            if (isScalable)
            {
                scaleMode = (flags & TextureSearchFlags.WithUpscale) != 0 ? TextureScaleMode.Scaled : TextureScaleMode.Eligible;
            }

            int sameAddressOverlapsCount;

            lock (_textures)
            {
                // Try to find a perfect texture match, with the same address and parameters.
                sameAddressOverlapsCount = _textures.FindOverlaps(info.Address, ref _textureOverlaps);
            }

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
                    else if (!TextureCompatibility.SizeMatches(overlap.Info, info))
                    {
                        // If this is used for sampling, the size must match,
                        // otherwise the shader would sample garbage data.
                        // To fix that, we create a new texture with the correct
                        // size, and copy the data from the old one to the new one.
                        overlap.ChangeSize(info.Width, info.Height, info.DepthOrLayers);
                    }

                    overlap.SynchronizeMemory();

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
            int overlapsCount;

            lock (_textures)
            {
                overlapsCount = _textures.FindOverlaps(info.Address, size, ref _textureOverlaps);
            }

            Texture texture = null;

            for (int index = 0; index < overlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];

                if (overlap.IsViewCompatible(info, size, out int firstLayer, out int firstLevel) == TextureViewCompatibility.Full)
                {
                    if (!isSamplerTexture)
                    {
                        info = AdjustSizes(overlap, info, firstLevel);
                    }

                    texture = overlap.CreateView(info, sizeInfo, firstLayer, firstLevel);

                    if (IsTextureModified(overlap))
                    {
                        texture.SignalModified();
                    }

                    // The size only matters (and is only really reliable) when the
                    // texture is used on a sampler, because otherwise the size will be
                    // aligned.
                    if (!TextureCompatibility.SizeMatches(overlap.Info, info, firstLevel) && isSamplerTexture)
                    {
                        texture.ChangeSize(info.Width, info.Height, info.DepthOrLayers);
                    }

                    break;
                }
            }

            // No match, create a new texture.
            if (texture == null)
            {
                texture = new Texture(_context, info, sizeInfo, scaleMode);

                // Step 1: Find textures that are view compatible with the new texture.
                // Any textures that are incompatible will contain garbage data, so they should be removed where possible.

                int viewCompatible = 0;
                bool setData = isSamplerTexture || overlapsCount == 0;

                for (int index = 0; index < overlapsCount; index++)
                {
                    Texture overlap = _textureOverlaps[index];
                    bool overlapInCache = overlap.CacheNode != null;

                    TextureViewCompatibility compatibility = texture.IsViewCompatible(overlap.Info, overlap.Size, out int firstLayer, out int firstLevel);

                    if (compatibility != TextureViewCompatibility.Incompatible)
                    {
                        if (_overlapInfo.Length != _textureOverlaps.Length)
                        {
                            Array.Resize(ref _overlapInfo, _textureOverlaps.Length);
                        }

                        _overlapInfo[viewCompatible] = new OverlapInfo(compatibility, firstLayer, firstLevel);
                        _textureOverlaps[viewCompatible++] = overlap;
                    }
                    else if (overlapInCache || !setData)
                    {
                        if (info.GobBlocksInZ > 1 && info.GobBlocksInZ == overlap.Info.GobBlocksInZ)
                        {
                            // Allow overlapping slices of 3D textures. Could be improved in future by making sure the textures don't overlap.
                            continue;
                        }

                        // The overlap texture is going to contain garbage data after we draw, or is generally incompatible.
                        // If the texture cannot be entirely contained in the new address space, and one of its view children is compatible with us,
                        // it must be flushed before removal, so that the data is not lost.

                        // If the texture was modified since its last use, then that data is probably meant to go into this texture.
                        // If the data has been modified by the CPU, then it also shouldn't be flushed.
                        bool modified = overlap.ConsumeModified();

                        bool flush = overlapInCache && !modified && (overlap.Address < texture.Address || overlap.EndAddress > texture.EndAddress) && overlap.HasViewCompatibleChild(texture);

                        setData |= modified || flush;

                        if (overlapInCache)
                        {
                            _cache.Remove(overlap, flush);
                        }
                    }
                }

                // We need to synchronize before copying the old view data to the texture,
                // otherwise the copied data would be overwritten by a future synchronization.
                texture.InitializeData(false, setData);

                for (int index = 0; index < viewCompatible; index++)
                {
                    Texture overlap = _textureOverlaps[index];
                    OverlapInfo oInfo = _overlapInfo[index];

                    if (oInfo.Compatibility != TextureViewCompatibility.Full)
                    {
                        continue; // Copy only compatibilty.
                    }

                    TextureInfo overlapInfo = AdjustSizes(texture, overlap.Info, oInfo.FirstLevel);

                    TextureCreateInfo createInfo = GetCreateInfo(overlapInfo, _context.Capabilities);

                    if (texture.ScaleFactor != overlap.ScaleFactor)
                    {
                        // A bit tricky, our new texture may need to contain an existing texture that is upscaled, but isn't itself.
                        // In that case, we prefer the higher scale only if our format is render-target-like, otherwise we scale the view down before copy.

                        texture.PropagateScale(overlap);
                    }

                    ITexture newView = texture.HostTexture.CreateView(createInfo, oInfo.FirstLayer, oInfo.FirstLevel);

                    overlap.HostTexture.CopyTo(newView, 0, 0);

                    // Inherit modification from overlapping texture, do that before replacing
                    // the view since the replacement operation removes it from the list.
                    if (IsTextureModified(overlap))
                    {
                        texture.SignalModified();
                    }

                    overlap.ReplaceView(texture, overlapInfo, newView, oInfo.FirstLayer, oInfo.FirstLevel);
                }

                // If the texture is a 3D texture, we need to additionally copy any slice
                // of the 3D texture to the newly created 3D texture.
                if (info.Target == Target.Texture3D)
                {
                    for (int index = 0; index < viewCompatible; index++)
                    {
                        Texture overlap = _textureOverlaps[index];
                        OverlapInfo oInfo = _overlapInfo[index];

                        if (oInfo.Compatibility != TextureViewCompatibility.Incompatible)
                        {
                            overlap.BlacklistScale();

                            overlap.HostTexture.CopyTo(texture.HostTexture, oInfo.FirstLayer, oInfo.FirstLevel);

                            if (IsTextureModified(overlap))
                            {
                                texture.SignalModified();
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

            lock (_textures)
            {
                _textures.Add(texture);
            }

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
            texture.IsModified = true;
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
                    formatInfo = new FormatInfo(Format.R8G8B8A8Unorm, 1, 1, 4, 4);
                }
                else if (formatInfo.Format.IsAstcSrgb())
                {
                    formatInfo = new FormatInfo(Format.R8G8B8A8Srgb, 1, 1, 4, 4);
                }
            }

            if (info.Target == Target.TextureBuffer)
            {
                // We assume that the host does not support signed normalized format
                // (as is the case with OpenGL), so we just use a unsigned format.
                // The shader will need the appropriate conversion code to compensate.
                switch (formatInfo.Format)
                {
                    case Format.R8Snorm:           formatInfo = new FormatInfo(Format.R8Sint,           1, 1, 1, 1); break;
                    case Format.R16Snorm:          formatInfo = new FormatInfo(Format.R16Sint,          1, 1, 2, 1); break;
                    case Format.R8G8Snorm:         formatInfo = new FormatInfo(Format.R8G8Sint,         1, 1, 2, 2); break;
                    case Format.R16G16Snorm:       formatInfo = new FormatInfo(Format.R16G16Sint,       1, 1, 4, 2); break;
                    case Format.R8G8B8A8Snorm:     formatInfo = new FormatInfo(Format.R8G8B8A8Sint,     1, 1, 4, 4); break;
                    case Format.R16G16B16A16Snorm: formatInfo = new FormatInfo(Format.R16G16B16A16Sint, 1, 1, 8, 4); break;
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
                if (texture.IsModified)
                {
                    texture.Flush();
                }
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
                if (texture.OverlapsWith(address, size) && texture.IsModified)
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
            lock (_textures)
            {
                _textures.Remove(texture);
            }
        }

        /// <summary>
        /// Disposes all textures in the cache.
        /// It's an error to use the texture manager after disposal.
        /// </summary>
        public void Dispose()
        {
            lock (_textures)
            {
                foreach (Texture texture in _textures)
                {
                    _modified.Remove(texture);
                    texture.Dispose();
                }
            }
        }
    }
}