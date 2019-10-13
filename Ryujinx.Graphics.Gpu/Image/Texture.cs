using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Texture.Astc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    class Texture : IRange<Texture>
    {
        private GpuContext _context;

        private TextureInfo _info;

        private SizeInfo _sizeInfo;

        public Format Format => _info.FormatInfo.Format;

        public TextureInfo Info => _info;

        private int _depth;
        private int _layers;
        private int _firstLayer;
        private int _firstLevel;

        private bool _hasData;

        private ITexture _arrayViewTexture;
        private Target   _arrayViewTarget;

        private Texture _viewStorage;

        private List<Texture> _views;

        public ITexture HostTexture { get; private set; }

        public LinkedListNode<Texture> CacheNode { get; set; }

        public bool Modified { get; set; }

        public ulong Address    => _info.Address;
        public ulong EndAddress => _info.Address + Size;

        public ulong Size => (ulong)_sizeInfo.TotalSize;

        private int _referenceCount;

        private int _sequenceNumber;

        private Texture(
            GpuContext  context,
            TextureInfo info,
            SizeInfo    sizeInfo,
            int         firstLayer,
            int         firstLevel)
        {
            InitializeTexture(context, info, sizeInfo);

            _firstLayer = firstLayer;
            _firstLevel = firstLevel;

            _hasData = true;
        }

        public Texture(GpuContext context, TextureInfo info, SizeInfo sizeInfo)
        {
            InitializeTexture(context, info, sizeInfo);

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(info, context.Capabilities);

            HostTexture = _context.Renderer.CreateTexture(createInfo);
        }

        private void InitializeTexture(GpuContext context, TextureInfo info, SizeInfo sizeInfo)
        {
            _context  = context;
            _sizeInfo = sizeInfo;

            SetInfo(info);

            _viewStorage = this;

            _views = new List<Texture>();
        }

        public Texture CreateView(TextureInfo info, SizeInfo sizeInfo, int firstLayer, int firstLevel)
        {
            Texture texture = new Texture(
                _context,
                info,
                sizeInfo,
                _firstLayer + firstLayer,
                _firstLevel + firstLevel);

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(info, _context.Capabilities);

            texture.HostTexture = HostTexture.CreateView(createInfo, firstLayer, firstLevel);

            _viewStorage.AddView(texture);

            return texture;
        }

        private void AddView(Texture texture)
        {
            _views.Add(texture);

            texture._viewStorage = this;
        }

        private void RemoveView(Texture texture)
        {
            _views.Remove(texture);

            texture._viewStorage = null;
        }

        public void ChangeSize(int width, int height, int depthOrLayers)
        {
            width  <<= _firstLevel;
            height <<= _firstLevel;

            if (_info.Target == Target.Texture3D)
            {
                depthOrLayers <<= _firstLevel;
            }
            else
            {
                depthOrLayers = _viewStorage._info.DepthOrLayers;
            }

            _viewStorage.RecreateStorageOrView(width, height, depthOrLayers);

            foreach (Texture view in _viewStorage._views)
            {
                int viewWidth  = Math.Max(1, width  >> view._firstLevel);
                int viewHeight = Math.Max(1, height >> view._firstLevel);

                int viewDepthOrLayers;

                if (view._info.Target == Target.Texture3D)
                {
                    viewDepthOrLayers = Math.Max(1, depthOrLayers >> view._firstLevel);
                }
                else
                {
                    viewDepthOrLayers = view._info.DepthOrLayers;
                }

                view.RecreateStorageOrView(viewWidth, viewHeight, viewDepthOrLayers);
            }
        }

        private void RecreateStorageOrView(int width, int height, int depthOrLayers)
        {
            SetInfo(new TextureInfo(
                _info.Address,
                width,
                height,
                depthOrLayers,
                _info.Levels,
                _info.SamplesInX,
                _info.SamplesInY,
                _info.Stride,
                _info.IsLinear,
                _info.GobBlocksInY,
                _info.GobBlocksInZ,
                _info.GobBlocksInTileX,
                _info.Target,
                _info.FormatInfo,
                _info.DepthStencilMode,
                _info.SwizzleR,
                _info.SwizzleG,
                _info.SwizzleB,
                _info.SwizzleA));

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(_info, _context.Capabilities);

            if (_viewStorage != this)
            {
                ReplaceStorage(_viewStorage.HostTexture.CreateView(createInfo, _firstLayer, _firstLevel));
            }
            else
            {
                ITexture newStorage = _context.Renderer.CreateTexture(createInfo);

                HostTexture.CopyTo(newStorage);

                ReplaceStorage(newStorage);
            }
        }

        public void SynchronizeMemory()
        {
            if (_sequenceNumber == _context.SequenceNumber && _hasData)
            {
                return;
            }

            _sequenceNumber = _context.SequenceNumber;

            bool modified = _context.PhysicalMemory.GetModifiedRanges(Address, Size).Length != 0;

            if (!modified && _hasData)
            {
                return;
            }

            ulong pageSize = (uint)_context.PhysicalMemory.GetPageSize();

            ulong pageMask = pageSize - 1;

            ulong rangeAddress = Address & ~pageMask;

            ulong rangeSize = (EndAddress - Address + pageMask) & ~pageMask;

            _context.Methods.InvalidateRange(rangeAddress, rangeSize);

            Span<byte> data = _context.PhysicalMemory.Read(Address, Size);

            if (_info.IsLinear)
            {
                data = LayoutConverter.ConvertLinearStridedToLinear(
                    _info.Width,
                    _info.Height,
                    _info.FormatInfo.BlockWidth,
                    _info.FormatInfo.BlockHeight,
                    _info.Stride,
                    _info.FormatInfo.BytesPerPixel,
                    data);
            }
            else
            {
                data = LayoutConverter.ConvertBlockLinearToLinear(
                    _info.Width,
                    _info.Height,
                    _depth,
                    _info.Levels,
                    _layers,
                    _info.FormatInfo.BlockWidth,
                    _info.FormatInfo.BlockHeight,
                    _info.FormatInfo.BytesPerPixel,
                    _info.GobBlocksInY,
                    _info.GobBlocksInZ,
                    _info.GobBlocksInTileX,
                    _sizeInfo,
                    data);
            }

            if (!_context.Capabilities.SupportsAstcCompression && _info.FormatInfo.Format.IsAstc())
            {
                int blockWidth  = _info.FormatInfo.BlockWidth;
                int blockHeight = _info.FormatInfo.BlockHeight;

                data = AstcDecoder.DecodeToRgba8(
                    data,
                    blockWidth,
                    blockHeight,
                    1,
                    _info.Width,
                    _info.Height,
                    _depth);
            }

            HostTexture.SetData(data);

            _hasData = true;
        }

        public void Flush()
        {
            byte[] data = HostTexture.GetData(0);

            _context.PhysicalMemory.Write(Address, data);
        }

        public bool IsPerfectMatch(TextureInfo info, TextureSearchFlags flags)
        {
            if (!FormatMatches(info, (flags & TextureSearchFlags.Strict) != 0))
            {
                return false;
            }

            if (!LayoutMatches(info))
            {
                return false;
            }

            if (!SizeMatches(info, (flags & TextureSearchFlags.Strict) == 0))
            {
                return false;
            }

            if ((flags & TextureSearchFlags.Sampler) != 0)
            {
                if (!SamplerParamsMatches(info))
                {
                    return false;
                }
            }

            if ((flags & TextureSearchFlags.IgnoreMs) != 0)
            {
                bool msTargetCompatible = _info.Target == Target.Texture2DMultisample &&
                                           info.Target == Target.Texture2D;

                if (!msTargetCompatible && !TargetAndSamplesCompatible(info))
                {
                    return false;
                }
            }
            else if (!TargetAndSamplesCompatible(info))
            {
                return false;
            }

            return _info.Address == info.Address && _info.Levels == info.Levels;
        }

        private bool FormatMatches(TextureInfo info, bool strict)
        {
            // D32F and R32F texture have the same representation internally,
            // however the R32F format is used to sample from depth textures.
            if (_info.FormatInfo.Format == Format.D32Float &&
                 info.FormatInfo.Format == Format.R32Float && !strict)
            {
                return true;
            }

            if (_info.FormatInfo.Format == Format.R8G8B8A8Srgb &&
                 info.FormatInfo.Format == Format.R8G8B8A8Unorm && !strict)
            {
                return true;
            }

            if (_info.FormatInfo.Format == Format.R8G8B8A8Unorm &&
                 info.FormatInfo.Format == Format.R8G8B8A8Srgb && !strict)
            {
                return true;
            }

            return _info.FormatInfo.Format == info.FormatInfo.Format;
        }

        private bool LayoutMatches(TextureInfo info)
        {
            if (_info.IsLinear != info.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (info.IsLinear)
            {
                return _info.Stride == info.Stride;
            }
            else
            {
                return _info.GobBlocksInY == info.GobBlocksInY &&
                       _info.GobBlocksInZ == info.GobBlocksInZ;
            }
        }

        public bool SizeMatches(TextureInfo info)
        {
            return SizeMatches(info, alignSizes: false);
        }

        public bool SizeMatches(TextureInfo info, int level)
        {
            return Math.Max(1, _info.Width      >> level) == info.Width  &&
                   Math.Max(1, _info.Height     >> level) == info.Height &&
                   Math.Max(1, _info.GetDepth() >> level) == info.GetDepth();
        }

        private bool SizeMatches(TextureInfo info, bool alignSizes)
        {
            if (_info.GetLayers() != info.GetLayers())
            {
                return false;
            }

            if (alignSizes)
            {
                Size size0 = GetAlignedSize(_info);
                Size size1 = GetAlignedSize(info);

                return size0.Width  == size1.Width  &&
                       size0.Height == size1.Height &&
                       size0.Depth  == size1.Depth;
            }
            else
            {
                return _info.Width      == info.Width  &&
                       _info.Height     == info.Height &&
                       _info.GetDepth() == info.GetDepth();
            }
        }

        private bool SamplerParamsMatches(TextureInfo info)
        {
            return _info.DepthStencilMode == info.DepthStencilMode &&
                   _info.SwizzleR         == info.SwizzleR         &&
                   _info.SwizzleG         == info.SwizzleG         &&
                   _info.SwizzleB         == info.SwizzleB         &&
                   _info.SwizzleA         == info.SwizzleA;
        }

        private bool TargetAndSamplesCompatible(TextureInfo info)
        {
            return _info.Target     == info.Target     &&
                   _info.SamplesInX == info.SamplesInX &&
                   _info.SamplesInY == info.SamplesInY;
        }

        public bool IsViewCompatible(TextureInfo info, ulong size, out int firstLayer, out int firstLevel)
        {
            // Out of range.
            if (info.Address < Address || info.Address + size > EndAddress)
            {
                firstLayer = 0;
                firstLevel = 0;

                return false;
            }

            int offset = (int)(info.Address - Address);

            if (!_sizeInfo.FindView(offset, (int)size, out firstLayer, out firstLevel))
            {
                return false;
            }

            if (!ViewLayoutCompatible(info, firstLevel))
            {
                return false;
            }

            if (!ViewFormatCompatible(info))
            {
                return false;
            }

            if (!ViewSizeMatches(info, firstLevel))
            {
                return false;
            }

            if (!ViewTargetCompatible(info))
            {
                return false;
            }

            return _info.SamplesInX == info.SamplesInX &&
                   _info.SamplesInY == info.SamplesInY;
        }

        private bool ViewLayoutCompatible(TextureInfo info, int level)
        {
            if (_info.IsLinear != info.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (info.IsLinear)
            {
                int width = Math.Max(1, _info.Width >> level);

                int stride = width * _info.FormatInfo.BytesPerPixel;

                stride = BitUtils.AlignUp(stride, 32);

                return stride == info.Stride;
            }
            else
            {
                int height = Math.Max(1, _info.Height     >> level);
                int depth  = Math.Max(1, _info.GetDepth() >> level);

                (int gobBlocksInY, int gobBlocksInZ) = SizeCalculator.GetMipGobBlockSizes(
                    height,
                    depth,
                    _info.FormatInfo.BlockHeight,
                    _info.GobBlocksInY,
                    _info.GobBlocksInZ);

                return gobBlocksInY == info.GobBlocksInY &&
                       gobBlocksInZ == info.GobBlocksInZ;
            }
        }

        private bool ViewFormatCompatible(TextureInfo info)
        {
            return TextureCompatibility.FormatCompatible(_info.FormatInfo, info.FormatInfo);
        }

        private bool ViewSizeMatches(TextureInfo info, int level)
        {
            Size size = GetAlignedSize(_info, level);

            Size otherSize = GetAlignedSize(info);

            return size.Width  == otherSize.Width  &&
                   size.Height == otherSize.Height &&
                   size.Depth  == otherSize.Depth;
        }

        private bool ViewTargetCompatible(TextureInfo info)
        {
            switch (_info.Target)
            {
                case Target.Texture1D:
                case Target.Texture1DArray:
                    return info.Target == Target.Texture1D ||
                           info.Target == Target.Texture1DArray;

                case Target.Texture2D:
                    return info.Target == Target.Texture2D ||
                           info.Target == Target.Texture2DArray;

                case Target.Texture2DArray:
                case Target.Cubemap:
                case Target.CubemapArray:
                    return info.Target == Target.Texture2D      ||
                           info.Target == Target.Texture2DArray ||
                           info.Target == Target.Cubemap        ||
                           info.Target == Target.CubemapArray;

                case Target.Texture2DMultisample:
                case Target.Texture2DMultisampleArray:
                    return info.Target == Target.Texture2DMultisample ||
                           info.Target == Target.Texture2DMultisampleArray;

                case Target.Texture3D:
                    return info.Target == Target.Texture3D;
            }

            return false;
        }

        private static Size GetAlignedSize(TextureInfo info, int level = 0)
        {
            int width  = Math.Max(1, info.Width  >> level);
            int height = Math.Max(1, info.Height >> level);

            if (info.IsLinear)
            {
                return SizeCalculator.GetLinearAlignedSize(
                    width,
                    height,
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel);
            }
            else
            {
                int depth = Math.Max(1, info.GetDepth() >> level);

                (int gobBlocksInY, int gobBlocksInZ) = SizeCalculator.GetMipGobBlockSizes(
                    height,
                    depth,
                    info.FormatInfo.BlockHeight,
                    info.GobBlocksInY,
                    info.GobBlocksInZ);

                return SizeCalculator.GetBlockLinearAlignedSize(
                    width,
                    height,
                    depth,
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel,
                    gobBlocksInY,
                    gobBlocksInZ,
                    info.GobBlocksInTileX);
            }
        }

        public ITexture GetTargetTexture(Target target)
        {
            if (target == _info.Target)
            {
                return HostTexture;
            }

            if (_arrayViewTexture == null && IsSameDimensionsTarget(target))
            {
                TextureCreateInfo createInfo = new TextureCreateInfo(
                    _info.Width,
                    _info.Height,
                    target == Target.CubemapArray ? 6 : 1,
                    _info.Levels,
                    _info.Samples,
                    _info.FormatInfo.BlockWidth,
                    _info.FormatInfo.BlockHeight,
                    _info.FormatInfo.BytesPerPixel,
                    _info.FormatInfo.Format,
                    _info.DepthStencilMode,
                    target,
                    _info.SwizzleR,
                    _info.SwizzleG,
                    _info.SwizzleB,
                    _info.SwizzleA);

                ITexture viewTexture = HostTexture.CreateView(createInfo, 0, 0);

                _arrayViewTexture = viewTexture;
                _arrayViewTarget  = target;

                return viewTexture;
            }
            else if (_arrayViewTarget == target)
            {
                return _arrayViewTexture;
            }

            return null;
        }

        private bool IsSameDimensionsTarget(Target target)
        {
            switch (_info.Target)
            {
                case Target.Texture1D:
                case Target.Texture1DArray:
                    return target == Target.Texture1D ||
                           target == Target.Texture1DArray;

                case Target.Texture2D:
                case Target.Texture2DArray:
                    return target == Target.Texture2D ||
                           target == Target.Texture2DArray;

                case Target.Cubemap:
                case Target.CubemapArray:
                    return target == Target.Cubemap ||
                           target == Target.CubemapArray;

                case Target.Texture2DMultisample:
                case Target.Texture2DMultisampleArray:
                    return target == Target.Texture2DMultisample ||
                           target == Target.Texture2DMultisampleArray;

                case Target.Texture3D:
                    return target == Target.Texture3D;
            }

            return false;
        }

        public void ReplaceView(Texture parent, TextureInfo info, ITexture hostTexture)
        {
            ReplaceStorage(hostTexture);

            parent._viewStorage.AddView(this);

            SetInfo(info);
        }

        private void SetInfo(TextureInfo info)
        {
            _info = info;

            _depth  = info.GetDepth();
            _layers = info.GetLayers();
        }

        private void ReplaceStorage(ITexture hostTexture)
        {
            DisposeTextures();

            HostTexture = hostTexture;
        }

        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        public void Invalidate()
        {
            // _hasData = false;
        }

        public void IncrementReferenceCount()
        {
            _referenceCount++;
        }

        public void DecrementReferenceCount()
        {
            if (--_referenceCount == 0)
            {
                if (_viewStorage != this)
                {
                    _viewStorage.RemoveView(this);
                }

                _context.Methods.TextureManager.RemoveTextureFromCache(this);

                DisposeTextures();
            }
        }

        private void DisposeTextures()
        {
            HostTexture.Dispose();

            _arrayViewTexture?.Dispose();
            _arrayViewTexture = null;
        }
    }
}