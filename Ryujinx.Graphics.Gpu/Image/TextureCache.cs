using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Dma;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Engine.Twod;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture cache.
    /// </summary>
    class TextureCache : IDisposable
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
        private readonly PhysicalMemory _physicalMemory;

        private readonly MultiRangeList<Texture> _textures;
        private readonly HashSet<Texture> _partiallyMappedTextures;

        private Texture[] _textureOverlaps;
        private OverlapInfo[] _overlapInfo;

        private readonly AutoDeleteCache _cache;

        /// <summary>
        /// Constructs a new instance of the texture manager.
        /// </summary>
        /// <param name="context">The GPU context that the texture manager belongs to</param>
        /// <param name="physicalMemory">Physical memory where the textures managed by this cache are mapped</param>
        public TextureCache(GpuContext context, PhysicalMemory physicalMemory)
        {
            _context = context;
            _physicalMemory = physicalMemory;

            _textures = new MultiRangeList<Texture>();
            _partiallyMappedTextures = new HashSet<Texture>();

            _textureOverlaps = new Texture[OverlapsBufferInitialCapacity];
            _overlapInfo = new OverlapInfo[OverlapsBufferInitialCapacity];

            _cache = new AutoDeleteCache();
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

            MultiRange unmapped = ((MemoryManager)sender).GetPhysicalRegions(e.Address, e.Size);

            lock (_textures)
            {
                overlapCount = _textures.FindOverlaps(unmapped, ref overlaps);
            }

            for (int i = 0; i < overlapCount; i++)
            {
                overlaps[i].Unmapped(unmapped);
            }

            // If any range was previously unmapped, we also need to purge
            // all partially mapped texture, as they might be fully mapped now.
            for (int i = 0; i < unmapped.Count; i++)
            {
                if (unmapped.GetSubRange(i).Address == MemoryManager.PteUnmapped)
                {
                    lock (_partiallyMappedTextures)
                    {
                        foreach (var texture in _partiallyMappedTextures)
                        {
                            texture.Unmapped(unmapped);
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Determines if a given texture is eligible for upscaling from its info.
        /// </summary>
        /// <param name="info">The texture info to check</param>
        /// <param name="withUpscale">True if the user of the texture would prefer it to be upscaled immediately</param>
        /// <returns>True if eligible</returns>
        private static TextureScaleMode IsUpscaleCompatible(TextureInfo info, bool withUpscale)
        {
            if ((info.Target == Target.Texture2D || info.Target == Target.Texture2DArray) && !info.FormatInfo.IsCompressed)
            {
                return UpscaleSafeMode(info) ? (withUpscale ? TextureScaleMode.Scaled : TextureScaleMode.Eligible) : TextureScaleMode.Undesired;
            }

            return TextureScaleMode.Blacklisted;
        }

        /// <summary>
        /// Determines if a given texture is "safe" for upscaling from its info.
        /// Note that this is different from being compatible - this elilinates targets that would have detrimental effects when scaled.
        /// </summary>
        /// <param name="info">The texture info to check</param>
        /// <returns>True if safe</returns>
        private static bool UpscaleSafeMode(TextureInfo info)
        {
            // While upscaling works for all targets defined by IsUpscaleCompatible, we additionally blacklist targets here that
            // may have undesirable results (upscaling blur textures) or simply waste GPU resources (upscaling texture atlas).

            if (info.Levels > 3)
            {
                // Textures with more than 3 levels are likely to be game textures, rather than render textures.
                // Small textures with full mips are likely to be removed by the next check.
                return false;
            }

            if (info.Width < 8 || info.Height < 8)
            {
                // Discount textures with small dimensions.
                return false;
            }

            int widthAlignment = (info.IsLinear ? Constants.StrideAlignment : Constants.GobAlignment) / info.FormatInfo.BytesPerPixel;

            if (!(info.FormatInfo.Format.IsDepthOrStencil() || info.FormatInfo.Components == 1))
            {
                // Discount square textures that aren't depth-stencil like. (excludes game textures, cubemap faces, most 3D texture LUT, texture atlas)
                // Detect if the texture is possibly square. Widths may be aligned, so to remove the uncertainty we align both the width and height.

                bool possiblySquare = BitUtils.AlignUp(info.Width, widthAlignment) == BitUtils.AlignUp(info.Height, widthAlignment);

                if (possiblySquare)
                {
                    return false;
                }
            }

            if (info.Height < 360)
            {
                int aspectWidth = (int)MathF.Ceiling((info.Height / 9f) * 16f);
                int aspectMaxWidth = BitUtils.AlignUp(aspectWidth, widthAlignment);
                int aspectMinWidth = BitUtils.AlignDown(aspectWidth, widthAlignment);

                if (info.Width >= aspectMinWidth && info.Width <= aspectMaxWidth && info.Height < 360)
                {
                    // Targets that are roughly 16:9 can only be rescaled if they're equal to or above 360p. (excludes blur and bloom textures)
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Lifts the texture to the top of the AutoDeleteCache. This is primarily used to enforce that
        /// data written to a target will be flushed to memory should the texture be deleted, but also
        /// keeps rendered textures alive without a pool reference.
        /// </summary>
        /// <param name="texture">Texture to lift</param>
        public void Lift(Texture texture)
        {
            _cache.Lift(texture);
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the texture is mapped</param>
        /// <param name="copyTexture">Copy texture to find or create</param>
        /// <param name="offset">Offset to be added to the physical texture address</param>
        /// <param name="formatInfo">Format information of the copy texture</param>
        /// <param name="preferScaling">Indicates if the texture should be scaled from the start</param>
        /// <param name="sizeHint">A hint indicating the minimum used size for the texture</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(
            MemoryManager memoryManager,
            TwodTexture copyTexture,
            ulong offset,
            FormatInfo formatInfo,
            bool shouldCreate,
            bool preferScaling = true,
            Size? sizeHint = null)
        {
            int gobBlocksInY = copyTexture.MemoryLayout.UnpackGobBlocksInY();
            int gobBlocksInZ = copyTexture.MemoryLayout.UnpackGobBlocksInZ();

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
                copyTexture.Address.Pack() + offset,
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

            if (!shouldCreate)
            {
                flags |= TextureSearchFlags.NoCreate;
            }

            Texture texture = FindOrCreateTexture(memoryManager, flags, info, 0, sizeHint);

            texture?.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the texture is mapped</param>
        /// <param name="colorState">Color buffer texture to find or create</param>
        /// <param name="layered">Indicates if the texture might be accessed with a non-zero layer index</param>
        /// <param name="samplesInX">Number of samples in the X direction, for MSAA</param>
        /// <param name="samplesInY">Number of samples in the Y direction, for MSAA</param>
        /// <param name="sizeHint">A hint indicating the minimum used size for the texture</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(
            MemoryManager memoryManager,
            RtColorState colorState,
            bool layered,
            int samplesInX,
            int samplesInY,
            Size sizeHint)
        {
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
                target = colorState.Depth > 1 && layered
                    ? Target.Texture2DMultisampleArray
                    : Target.Texture2DMultisample;
            }
            else
            {
                target = colorState.Depth > 1 && layered
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
                colorState.Address.Pack(),
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

            int layerSize = !isLinear ? colorState.LayerSize * 4 : 0;

            Texture texture = FindOrCreateTexture(memoryManager, TextureSearchFlags.WithUpscale, info, layerSize, sizeHint);

            texture?.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the texture is mapped</param>
        /// <param name="dsState">Depth-stencil buffer texture to find or create</param>
        /// <param name="size">Size of the depth-stencil texture</param>
        /// <param name="samplesInX">Number of samples in the X direction, for MSAA</param>
        /// <param name="samplesInY">Number of samples in the Y direction, for MSAA</param>
        /// <param name="sizeHint">A hint indicating the minimum used size for the texture</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(
            MemoryManager memoryManager,
            RtDepthStencilState dsState,
            Size3D size,
            int samplesInX,
            int samplesInY,
            Size sizeHint)
        {
            int gobBlocksInY = dsState.MemoryLayout.UnpackGobBlocksInY();
            int gobBlocksInZ = dsState.MemoryLayout.UnpackGobBlocksInZ();

            Target target = (samplesInX | samplesInY) != 1
                ? Target.Texture2DMultisample
                : Target.Texture2D;

            FormatInfo formatInfo = dsState.Format.Convert();

            TextureInfo info = new TextureInfo(
                dsState.Address.Pack(),
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

            Texture texture = FindOrCreateTexture(memoryManager, TextureSearchFlags.WithUpscale, info, dsState.LayerSize * 4, sizeHint);

            texture?.SynchronizeMemory();

            return texture;
        }

        /// <summary>
        /// Tries to find an existing texture, or create a new one if not found.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the texture is mapped</param>
        /// <param name="flags">The texture search flags, defines texture comparison rules</param>
        /// <param name="info">Texture information of the texture to be found or created</param>
        /// <param name="layerSize">Size in bytes of a single texture layer</param>
        /// <param name="sizeHint">A hint indicating the minimum used size for the texture</param>
        /// <param name="range">Optional ranges of physical memory where the texture data is located</param>
        /// <returns>The texture</returns>
        public Texture FindOrCreateTexture(
            MemoryManager memoryManager,
            TextureSearchFlags flags,
            TextureInfo info,
            int layerSize = 0,
            Size? sizeHint = null,
            MultiRange? range = null)
        {
            bool isSamplerTexture = (flags & TextureSearchFlags.ForSampler) != 0;

            TextureScaleMode scaleMode = IsUpscaleCompatible(info, (flags & TextureSearchFlags.WithUpscale) != 0);

            ulong address;

            if (range != null)
            {
                address = range.Value.GetSubRange(0).Address;
            }
            else
            {
                address = memoryManager.Translate(info.GpuAddress);

                if (address == MemoryManager.PteUnmapped)
                {
                    return null;
                }
            }

            int sameAddressOverlapsCount;

            lock (_textures)
            {
                // Try to find a perfect texture match, with the same address and parameters.
                sameAddressOverlapsCount = _textures.FindOverlaps(address, ref _textureOverlaps);
            }

            Texture texture = null;

            TextureMatchQuality bestQuality = TextureMatchQuality.NoMatch;

            for (int index = 0; index < sameAddressOverlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];

                TextureMatchQuality matchQuality = overlap.IsExactMatch(info, flags);

                if (matchQuality != TextureMatchQuality.NoMatch)
                {
                    // If the parameters match, we need to make sure the texture is mapped to the same memory regions.
                    if (range != null)
                    {
                        // If a range of memory was supplied, just check if the ranges match.
                        if (!overlap.Range.Equals(range.Value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // If no range was supplied, we can check if the GPU virtual address match. If they do,
                        // we know the textures are located at the same memory region.
                        // If they don't, it may still be mapped to the same physical region, so we
                        // do a more expensive check to tell if they are mapped into the same physical regions.
                        // If the GPU VA for the texture has ever been unmapped, then the range must be checked regardless.
                        if ((overlap.Info.GpuAddress != info.GpuAddress || overlap.ChangedMapping) &&
                            !memoryManager.CompareRange(overlap.Range, info.GpuAddress))
                        {
                            continue;
                        }
                    }
                }

                if (matchQuality == TextureMatchQuality.Perfect)
                {
                    texture = overlap;
                    break;
                }
                else if (matchQuality > bestQuality)
                {
                    texture = overlap;
                    bestQuality = matchQuality;
                }
            }

            if (texture != null)
            {
                ChangeSizeIfNeeded(info, texture, isSamplerTexture, sizeHint);

                texture.SynchronizeMemory();

                return texture;
            }
            else if (flags.HasFlag(TextureSearchFlags.NoCreate))
            {
                return null;
            }

            // Calculate texture sizes, used to find all overlapping textures.
            SizeInfo sizeInfo = info.CalculateSizeInfo(layerSize);

            ulong size = (ulong)sizeInfo.TotalSize;
            bool partiallyMapped = false;

            if (range == null)
            {
                range = memoryManager.GetPhysicalRegions(info.GpuAddress, size);

                for (int i = 0; i < range.Value.Count; i++)
                {
                    if (range.Value.GetSubRange(i).Address == MemoryManager.PteUnmapped)
                    {
                        partiallyMapped = true;
                        break;
                    }
                }
            }

            // Find view compatible matches.
            int overlapsCount;

            lock (_textures)
            {
                overlapsCount = _textures.FindOverlaps(range.Value, ref _textureOverlaps);
            }

            if (_overlapInfo.Length != _textureOverlaps.Length)
            {
                Array.Resize(ref _overlapInfo, _textureOverlaps.Length);
            }

            // =============== Find Texture View of Existing Texture ===============

            int fullyCompatible = 0;

            // Evaluate compatibility of overlaps, add temporary references

            for (int index = 0; index < overlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];
                TextureViewCompatibility overlapCompatibility = overlap.IsViewCompatible(
                    info,
                    range.Value,
                    sizeInfo.LayerSize,
                    _context.Capabilities,
                    out int firstLayer,
                    out int firstLevel);

                if (overlapCompatibility == TextureViewCompatibility.Full)
                {
                    if (overlap.IsView)
                    {
                        overlapCompatibility = TextureViewCompatibility.CopyOnly;
                    }
                    else
                    {
                        fullyCompatible++;
                    }
                }

                _overlapInfo[index] = new OverlapInfo(overlapCompatibility, firstLayer, firstLevel);
                overlap.IncrementReferenceCount();
            }

            // Search through the overlaps to find a compatible view and establish any copy dependencies.

            for (int index = 0; index < overlapsCount; index++)
            {
                Texture overlap = _textureOverlaps[index];
                OverlapInfo oInfo = _overlapInfo[index];

                if (oInfo.Compatibility == TextureViewCompatibility.Full)
                {
                    TextureInfo adjInfo = AdjustSizes(overlap, info, oInfo.FirstLevel);

                    if (!isSamplerTexture)
                    {
                        info = adjInfo;
                    }

                    texture = overlap.CreateView(adjInfo, sizeInfo, range.Value, oInfo.FirstLayer, oInfo.FirstLevel);

                    ChangeSizeIfNeeded(info, texture, isSamplerTexture, sizeHint);

                    texture.SynchronizeMemory();
                    break;
                }
                else if (oInfo.Compatibility == TextureViewCompatibility.CopyOnly && fullyCompatible == 0)
                {
                    // Only copy compatible. If there's another choice for a FULLY compatible texture, choose that instead.

                    texture = new Texture(_context, _physicalMemory, info, sizeInfo, range.Value, scaleMode);

                    texture.InitializeGroup(true, true, new List<TextureIncompatibleOverlap>());
                    texture.InitializeData(false, false);

                    overlap.SynchronizeMemory();
                    overlap.CreateCopyDependency(texture, oInfo.FirstLayer, oInfo.FirstLevel, true);
                    break;
                }
            }

            if (texture != null)
            {
                // This texture could be a view of multiple parent textures with different storages, even if it is a view.
                // When a texture is created, make sure all possible dependencies to other textures are created as copies.
                // (even if it could be fulfilled without a copy)

                for (int index = 0; index < overlapsCount; index++)
                {
                    Texture overlap = _textureOverlaps[index];
                    OverlapInfo oInfo = _overlapInfo[index];

                    if (oInfo.Compatibility <= TextureViewCompatibility.LayoutIncompatible)
                    {
                        if (!overlap.IsView && texture.DataOverlaps(overlap, oInfo.Compatibility))
                        {
                            texture.Group.RegisterIncompatibleOverlap(new TextureIncompatibleOverlap(overlap.Group, oInfo.Compatibility), true);
                        }
                    }
                    else if (overlap.Group != texture.Group)
                    {
                        overlap.SynchronizeMemory();
                        overlap.CreateCopyDependency(texture, oInfo.FirstLayer, oInfo.FirstLevel, true);
                    }
                }

                texture.SynchronizeMemory();
            }

            // =============== Create a New Texture ===============

            // No match, create a new texture.
            if (texture == null)
            {
                texture = new Texture(_context, _physicalMemory, info, sizeInfo, range.Value, scaleMode);

                // Step 1: Find textures that are view compatible with the new texture.
                // Any textures that are incompatible will contain garbage data, so they should be removed where possible.

                int viewCompatible = 0;
                fullyCompatible = 0;
                bool setData = isSamplerTexture || overlapsCount == 0 || flags.HasFlag(TextureSearchFlags.ForCopy);

                bool hasLayerViews = false;
                bool hasMipViews = false;

                var incompatibleOverlaps = new List<TextureIncompatibleOverlap>();

                for (int index = 0; index < overlapsCount; index++)
                {
                    Texture overlap = _textureOverlaps[index];
                    bool overlapInCache = overlap.CacheNode != null;

                    TextureViewCompatibility compatibility = texture.IsViewCompatible(
                        overlap.Info,
                        overlap.Range,
                        overlap.LayerSize,
                        _context.Capabilities,
                        out int firstLayer,
                        out int firstLevel);

                    if (overlap.IsView && compatibility == TextureViewCompatibility.Full)
                    {
                        compatibility = TextureViewCompatibility.CopyOnly;
                    }

                    if (compatibility > TextureViewCompatibility.LayoutIncompatible)
                    {
                        _overlapInfo[viewCompatible] = new OverlapInfo(compatibility, firstLayer, firstLevel);
                        _textureOverlaps[index] = _textureOverlaps[viewCompatible];
                        _textureOverlaps[viewCompatible] = overlap;

                        if (compatibility == TextureViewCompatibility.Full)
                        {
                            if (viewCompatible != fullyCompatible)
                            {
                                // Swap overlaps so that the fully compatible views have priority.

                                _overlapInfo[viewCompatible] = _overlapInfo[fullyCompatible];
                                _textureOverlaps[viewCompatible] = _textureOverlaps[fullyCompatible];

                                _overlapInfo[fullyCompatible] = new OverlapInfo(compatibility, firstLayer, firstLevel);
                                _textureOverlaps[fullyCompatible] = overlap;
                            }

                            fullyCompatible++;
                        }

                        viewCompatible++;

                        hasLayerViews |= overlap.Info.GetSlices() < texture.Info.GetSlices();
                        hasMipViews |= overlap.Info.Levels < texture.Info.Levels;
                    }
                    else
                    {
                        bool dataOverlaps = texture.DataOverlaps(overlap, compatibility);

                        if (!overlap.IsView && dataOverlaps && !incompatibleOverlaps.Exists(incompatible => incompatible.Group == overlap.Group))
                        {
                            incompatibleOverlaps.Add(new TextureIncompatibleOverlap(overlap.Group, compatibility));
                        }

                        bool removeOverlap;
                        bool modified = overlap.CheckModified(false);

                        if (overlapInCache || !setData)
                        {
                            if (!dataOverlaps)
                            {
                                // Allow textures to overlap if their data does not actually overlap.
                                // This typically happens when mip level subranges of a layered texture are used. (each texture fills the gaps of the others)
                                continue;
                            }

                            // The overlap texture is going to contain garbage data after we draw, or is generally incompatible.
                            // The texture group will obtain copy dependencies for any subresources that are compatible between the two textures,
                            // but sometimes its data must be flushed regardless.

                            // If the texture was modified since its last use, then that data is probably meant to go into this texture.
                            // If the data has been modified by the CPU, then it also shouldn't be flushed.

                            bool flush = overlapInCache && !modified && overlap.AlwaysFlushOnOverlap;

                            setData |= modified || flush;

                            if (overlapInCache)
                            {
                                _cache.Remove(overlap, flush);
                            }

                            removeOverlap = modified;
                        }
                        else
                        {
                            // If an incompatible overlapping texture has been modified, then it's data is likely destined for this texture,
                            // and the overlapped texture will contain garbage. In this case, it should be removed to save memory.
                            removeOverlap = modified;
                        }

                        if (removeOverlap && overlap.Info.Target != Target.TextureBuffer)
                        {
                            overlap.RemoveFromPools(false);
                        }
                    }
                }

                texture.InitializeGroup(hasLayerViews, hasMipViews, incompatibleOverlaps);

                // We need to synchronize before copying the old view data to the texture,
                // otherwise the copied data would be overwritten by a future synchronization.
                texture.InitializeData(false, setData);

                texture.Group.InitializeOverlaps();

                for (int index = 0; index < viewCompatible; index++)
                {
                    Texture overlap = _textureOverlaps[index];

                    OverlapInfo oInfo = _overlapInfo[index];

                    if (overlap.Group == texture.Group)
                    {
                        // If the texture group is equal, then this texture (or its parent) is already a view.
                        continue;
                    }

                    TextureInfo overlapInfo = AdjustSizes(texture, overlap.Info, oInfo.FirstLevel);

                    if (texture.ScaleFactor != overlap.ScaleFactor)
                    {
                        // A bit tricky, our new texture may need to contain an existing texture that is upscaled, but isn't itself.
                        // In that case, we prefer the higher scale only if our format is render-target-like, otherwise we scale the view down before copy.

                        texture.PropagateScale(overlap);
                    }

                    if (oInfo.Compatibility != TextureViewCompatibility.Full)
                    {
                        // Copy only compatibility, or target texture is already a view.

                        overlap.SynchronizeMemory();
                        texture.CreateCopyDependency(overlap, oInfo.FirstLayer, oInfo.FirstLevel, false);
                    }
                    else
                    {
                        TextureCreateInfo createInfo = GetCreateInfo(overlapInfo, _context.Capabilities, overlap.ScaleFactor);

                        ITexture newView = texture.HostTexture.CreateView(createInfo, oInfo.FirstLayer, oInfo.FirstLevel);

                        overlap.SynchronizeMemory();

                        overlap.HostTexture.CopyTo(newView, 0, 0);

                        overlap.ReplaceView(texture, overlapInfo, newView, oInfo.FirstLayer, oInfo.FirstLevel);
                    }
                }

                texture.SynchronizeMemory();
            }

            // Sampler textures are managed by the texture pool, all other textures
            // are managed by the auto delete cache.
            if (!isSamplerTexture)
            {
                _cache.Add(texture);
            }

            lock (_textures)
            {
                _textures.Add(texture);
            }

            if (partiallyMapped)
            {
                lock (_partiallyMappedTextures)
                {
                    _partiallyMappedTextures.Add(texture);
                }
            }

            ShrinkOverlapsBufferIfNeeded();

            for (int i = 0; i < overlapsCount; i++)
            {
                _textureOverlaps[i].DecrementReferenceCount();
            }

            return texture;
        }

        /// <summary>
        /// Changes a texture's size to match the desired size for samplers,
        /// or increases a texture's size to fit the region indicated by a size hint.
        /// </summary>
        /// <param name="info">The desired texture info</param>
        /// <param name="texture">The texture to resize</param>
        /// <param name="isSamplerTexture">True if the texture will be used for a sampler, false otherwise</param>
        /// <param name="sizeHint">A hint indicating the minimum used size for the texture</param>
        private void ChangeSizeIfNeeded(TextureInfo info, Texture texture, bool isSamplerTexture, Size? sizeHint)
        {
            if (isSamplerTexture)
            {
                // If this is used for sampling, the size must match,
                // otherwise the shader would sample garbage data.
                // To fix that, we create a new texture with the correct
                // size, and copy the data from the old one to the new one.

                if (!TextureCompatibility.SizeMatches(texture.Info, info))
                {
                    texture.ChangeSize(info.Width, info.Height, info.DepthOrLayers);
                }
            }
            else if (sizeHint != null)
            {
                // A size hint indicates that data will be used within that range, at least.
                // If the texture is smaller than the size hint, it must be enlarged to meet it.
                // The maximum size is provided by the requested info, which generally has an aligned size.

                int width = Math.Max(texture.Info.Width, Math.Min(sizeHint.Value.Width, info.Width));
                int height = Math.Max(texture.Info.Height, Math.Min(sizeHint.Value.Height, info.Height));

                if (texture.Info.Width != width || texture.Info.Height != height)
                {
                    texture.ChangeSize(width, height, info.DepthOrLayers);
                }
            }
        }

        /// <summary>
        /// Tries to find an existing texture matching the given buffer copy destination. If none is found, returns null.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the texture is mapped</param>
        /// <param name="tex">The texture information</param>
        /// <param name="gpuVa">GPU virtual address of the texture</param>
        /// <param name="bpp">Bytes per pixel</param>
        /// <param name="stride">If <paramref name="linear"/> is true, should have the texture stride, otherwise ignored</param>
        /// <param name="xCount">Number of pixels to be copied per line</param>
        /// <param name="yCount">Number of lines to be copied</param>
        /// <param name="linear">True if the texture has a linear layout, false otherwise</param>
        /// <returns>A matching texture, or null if there is no match</returns>
        public Texture FindTexture(
            MemoryManager memoryManager,
            DmaTexture tex,
            ulong gpuVa,
            int bpp,
            int stride,
            int xCount,
            int yCount,
            bool linear)
        {
            ulong address = memoryManager.Translate(gpuVa);

            if (address == MemoryManager.PteUnmapped)
            {
                return null;
            }

            int addressMatches = _textures.FindOverlaps(address, ref _textureOverlaps);
            Texture textureMatch = null;

            for (int i = 0; i < addressMatches; i++)
            {
                Texture texture = _textureOverlaps[i];
                FormatInfo format = texture.Info.FormatInfo;

                if (texture.Info.DepthOrLayers > 1 || texture.Info.Levels > 1 || texture.Info.FormatInfo.IsCompressed)
                {
                    // Don't support direct buffer copies to anything that isn't a single 2D image, uncompressed.
                    continue;
                }

                bool match;

                if (linear)
                {
                    // Size is not available for linear textures. Use the stride and end of the copy region instead.

                    match = texture.Info.IsLinear && texture.Info.Stride == stride && tex.RegionY + yCount <= texture.Info.Height;
                }
                else
                {
                    // Bpp may be a mismatch between the target texture and the param.
                    // Due to the way linear strided and block layouts work, widths can be multiplied by Bpp for comparison.
                    // Note: tex.Width is the aligned texture size. Prefer param.XCount, as the destination should be a texture with that exact size.

                    bool sizeMatch = xCount * bpp == texture.Info.Width * format.BytesPerPixel && tex.Height == texture.Info.Height;
                    bool formatMatch = !texture.Info.IsLinear &&
                                        texture.Info.GobBlocksInY == tex.MemoryLayout.UnpackGobBlocksInY() &&
                                        texture.Info.GobBlocksInZ == tex.MemoryLayout.UnpackGobBlocksInZ();

                    match = sizeMatch && formatMatch;
                }

                if (match)
                {
                    if (textureMatch == null)
                    {
                        textureMatch = texture;
                    }
                    else if (texture.Group != textureMatch.Group)
                    {
                        return null; // It's ambiguous which texture should match between multiple choices, so leave it up to the slow path.
                    }
                }
            }

            return textureMatch;
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

            // 2D and 2D multisample textures are not considered compatible.
            // This specific case is required for copies, where the source texture might be multisample.
            // In this case, we inherit the parent texture multisample state.
            Target target = info.Target;
            int samplesInX = info.SamplesInX;
            int samplesInY = info.SamplesInY;

            if (target == Target.Texture2D && parent.Target == Target.Texture2DMultisample)
            {
                target = Target.Texture2DMultisample;
                samplesInX = parent.Info.SamplesInX;
                samplesInY = parent.Info.SamplesInY;
            }

            return new TextureInfo(
                info.GpuAddress,
                width,
                height,
                depthOrLayers,
                info.Levels,
                samplesInX,
                samplesInY,
                info.Stride,
                info.IsLinear,
                info.GobBlocksInY,
                info.GobBlocksInZ,
                info.GobBlocksInTileX,
                target,
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
        /// <param name="scale">Texture scale factor, to be applied to the texture size</param>
        /// <returns>The texture creation information</returns>
        public static TextureCreateInfo GetCreateInfo(TextureInfo info, Capabilities caps, float scale)
        {
            FormatInfo formatInfo = TextureCompatibility.ToHostCompatibleFormat(info, caps);

            if (info.Target == Target.TextureBuffer)
            {
                // We assume that the host does not support signed normalized format
                // (as is the case with OpenGL), so we just use a unsigned format.
                // The shader will need the appropriate conversion code to compensate.
                switch (formatInfo.Format)
                {
                    case Format.R8Snorm:
                        formatInfo = new FormatInfo(Format.R8Sint, 1, 1, 1, 1);
                        break;
                    case Format.R16Snorm:
                        formatInfo = new FormatInfo(Format.R16Sint, 1, 1, 2, 1);
                        break;
                    case Format.R8G8Snorm:
                        formatInfo = new FormatInfo(Format.R8G8Sint, 1, 1, 2, 2);
                        break;
                    case Format.R16G16Snorm:
                        formatInfo = new FormatInfo(Format.R16G16Sint, 1, 1, 4, 2);
                        break;
                    case Format.R8G8B8A8Snorm:
                        formatInfo = new FormatInfo(Format.R8G8B8A8Sint, 1, 1, 4, 4);
                        break;
                    case Format.R16G16B16A16Snorm:
                        formatInfo = new FormatInfo(Format.R16G16B16A16Sint, 1, 1, 8, 4);
                        break;
                }
            }

            int width  = info.Width  / info.SamplesInX;
            int height = info.Height / info.SamplesInY;

            int depth = info.GetDepth() * info.GetLayers();

            if (scale != 1f)
            {
                width  = (int)MathF.Ceiling(width  * scale);
                height = (int)MathF.Ceiling(height * scale);
            }

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

            lock (_partiallyMappedTextures)
            {
                _partiallyMappedTextures.Remove(texture);
            }
        }

        /// <summary>
        /// Disposes all textures and samplers in the cache.
        /// It's an error to use the texture cache after disposal.
        /// </summary>
        public void Dispose()
        {
            lock (_textures)
            {
                foreach (Texture texture in _textures)
                {
                    texture.Dispose();
                }
            }
        }
    }
}
