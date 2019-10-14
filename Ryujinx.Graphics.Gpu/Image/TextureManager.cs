using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    class TextureManager
    {
        private GpuContext    _context;
        private BufferManager _bufferManager;

        private SamplerPool _samplerPool;

        private ulong _texturePoolAddress;
        private int   _texturePoolMaximumId;

        private TexturePoolCache _texturePoolCache;

        private Texture[] _rtColors;
        private Texture   _rtColor3D;

        private Texture _rtDepthStencil;

        private ITexture[] _rtHostColors;

        private ITexture _rtHostDs;

        private RangeList<Texture> _textures;

        private AutoDeleteCache _cache;

        private TextureBindingInfo[][] _bindings;

        private struct TextureStatePerStage
        {
            public ITexture Texture;
            public ISampler Sampler;
        }

        private TextureStatePerStage[][] _textureState;

        private int _textureBufferIndex;

        public TextureManager(GpuContext context, BufferManager bufferManager)
        {
            _context       = context;
            _bufferManager = bufferManager;

            _texturePoolCache = new TexturePoolCache(context, this);

            _rtColors = new Texture[Constants.TotalRenderTargets];

            _rtHostColors = new ITexture[Constants.TotalRenderTargets];

            _textures = new RangeList<Texture>();

            _cache = new AutoDeleteCache();

            _bindings = new TextureBindingInfo[Constants.TotalShaderStages][];

            _textureState = new TextureStatePerStage[Constants.TotalShaderStages][];
        }

        public void BindTextures(int stage, TextureBindingInfo[] bindings)
        {
            _bindings[stage] = bindings;

            _textureState[stage] = new TextureStatePerStage[bindings.Length];
        }

        public void SetTextureBufferIndex(int index)
        {
            _textureBufferIndex = index;
        }

        public void SetSamplerPool(ulong gpuVa, int maximumId)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            if (_samplerPool != null)
            {
                if (_samplerPool.Address == address)
                {
                    return;
                }

                _samplerPool.Dispose();
            }

            _samplerPool = new SamplerPool(_context, address, maximumId);
        }

        public void SetTexturePool(ulong gpuVa, int maximumId)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            _texturePoolAddress   = address;
            _texturePoolMaximumId = maximumId;
        }

        public void SetRenderTargetColor(int index, Texture color)
        {
            _rtColors[index] = color;

            _rtColor3D = null;
        }

        public void SetRenderTargetColor3D(Texture color)
        {
            _rtColor3D = color;
        }

        public void SetRenderTargetDepthStencil(Texture depthStencil)
        {
            _rtDepthStencil = depthStencil;
        }

        public void CommitBindings()
        {
            UpdateTextures();
            UpdateRenderTargets();
        }

        private void UpdateTextures()
        {
            TexturePool texturePool = _texturePoolCache.FindOrCreate(
                _texturePoolAddress,
                _texturePoolMaximumId);

            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                int stageIndex = (int)stage - 1;

                if (_bindings[stageIndex] == null)
                {
                    continue;
                }

                for (int index = 0; index < _bindings[stageIndex].Length; index++)
                {
                    TextureBindingInfo binding = _bindings[stageIndex][index];

                    int packedId = ReadPackedId(stageIndex, binding.Handle);

                    int textureId = (packedId >> 0)  & 0xfffff;
                    int samplerId = (packedId >> 20) & 0xfff;

                    Texture texture = texturePool.Get(textureId);

                    ITexture hostTexture = texture?.GetTargetTexture(binding.Target);

                    if (_textureState[stageIndex][index].Texture != hostTexture)
                    {
                        _textureState[stageIndex][index].Texture = hostTexture;

                        _context.Renderer.GraphicsPipeline.BindTexture(index, stage, hostTexture);
                    }

                    Sampler sampler = _samplerPool.Get(samplerId);

                    ISampler hostSampler = sampler?.HostSampler;

                    if (_textureState[stageIndex][index].Sampler != hostSampler)
                    {
                        _textureState[stageIndex][index].Sampler = hostSampler;

                        _context.Renderer.GraphicsPipeline.BindSampler(index, stage, hostSampler);
                    }
                }
            }
        }

        private void UpdateRenderTargets()
        {
            bool anyChanged = false;

            if (_rtHostDs != _rtDepthStencil?.HostTexture)
            {
                _rtHostDs = _rtDepthStencil?.HostTexture;

                anyChanged = true;
            }

            if (_rtColor3D == null)
            {
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
                    _context.Renderer.GraphicsPipeline.SetRenderTargets(_rtHostColors, _rtHostDs);
                }
            }
            else
            {
                if (_rtHostColors[0] != _rtColor3D.HostTexture)
                {
                    _rtHostColors[0] = _rtColor3D.HostTexture;

                    anyChanged = true;
                }

                if (anyChanged)
                {
                    _context.Renderer.GraphicsPipeline.SetRenderTargets(_rtColor3D.HostTexture, _rtHostDs);
                }
            }
        }

        private int ReadPackedId(int stage, int wordOffset)
        {
            ulong address = _bufferManager.GetGraphicsUniformBufferAddress(stage, _textureBufferIndex);

            address += (uint)wordOffset * 4;

            return BitConverter.ToInt32(_context.PhysicalMemory.Read(address, 4));
        }

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

        public Texture FindOrCreateTexture(TextureInfo info, TextureSearchFlags flags = TextureSearchFlags.None)
        {
            bool isSamplerTexture = (flags & TextureSearchFlags.Sampler) != 0;

            // Try to find a perfect texture match, with the same address and parameters.
            Texture[] sameAddressOverlaps = _textures.FindOverlaps(info.Address);

            foreach (Texture overlap in sameAddressOverlaps)
            {
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

            if (info.IsLinear)
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

            Texture[] overlaps = _textures.FindOverlaps(info.Address, size);

            Texture texture = null;

            foreach (Texture overlap in overlaps)
            {
                if (overlap.IsViewCompatible(info, size, out int firstLayer, out int firstLevel))
                {
                    if (!isSamplerTexture)
                    {
                        info = AdjustSizes(overlap, info, firstLevel);
                    }

                    texture = overlap.CreateView(info, sizeInfo, firstLayer, firstLevel);

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

                foreach (Texture overlap in overlaps)
                {
                    if (texture.IsViewCompatible(overlap.Info, overlap.Size, out int firstLayer, out int firstLevel))
                    {
                        TextureInfo overlapInfo = AdjustSizes(texture, overlap.Info, firstLevel);

                        TextureCreateInfo createInfo = GetCreateInfo(overlapInfo, _context.Capabilities);

                        ITexture newView = texture.HostTexture.CreateView(createInfo, firstLayer, firstLevel);

                        overlap.HostTexture.CopyTo(newView);

                        overlap.ReplaceView(texture, overlapInfo, newView);
                    }
                }
            }

            // Sampler textures are managed by the texture pool, all other textures
            // are managed by the auto delete cache.
            if (!isSamplerTexture)
            {
                _cache.Add(texture);
            }

            _textures.Add(texture);

            return texture;
        }

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

        public Texture Find2(ulong address)
        {
            Texture[] ts = _textures.FindOverlaps(address, 1);

            if (ts.Length == 2)
            {
                return ts[1];
            }

            if (ts.Length == 0)
            {
                ts = _textures.FindOverlaps(address - 1, 2);
            }

            if (ts.Length == 0)
            {
                return null;
            }

            return ts[0];
        }

        public void InvalidateRange(ulong address, ulong size)
        {
            Texture[] overlaps = _textures.FindOverlaps(address, size);

            foreach (Texture overlap in overlaps)
            {
                overlap.Invalidate();
            }

            _samplerPool?.InvalidateRange(address, size);

            _texturePoolCache.InvalidateRange(address, size);
        }

        public void Flush()
        {
            foreach (Texture texture in _cache)
            {
                if (texture.Info.IsLinear && texture.Modified)
                {
                    texture.Flush();

                    texture.Modified = false;
                }
            }
        }

        public void RemoveTextureFromCache(Texture texture)
        {
            _textures.Remove(texture);
        }
    }
}