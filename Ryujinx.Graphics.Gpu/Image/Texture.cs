using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Texture.Astc;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Represents a cached GPU texture.
    /// </summary>
    class Texture : IRange, IDisposable
    {
        private GpuContext _context;

        private SizeInfo _sizeInfo;

        /// <summary>
        /// Texture format.
        /// </summary>
        public Format Format => Info.FormatInfo.Format;

        /// <summary>
        /// Texture information.
        /// </summary>
        public TextureInfo Info { get; private set; }

        /// <summary>
        /// Host scale factor.
        /// </summary>
        public float ScaleFactor { get; private set; }

        /// <summary>
        /// Upscaling mode. Informs if a texture is scaled, or is eligible for scaling.
        /// </summary>
        public TextureScaleMode ScaleMode { get; private set; }

        private int _depth;
        private int _layers;
        private int _firstLayer;
        private int _firstLevel;

        private bool _hasData;

        private ITexture _arrayViewTexture;
        private Target   _arrayViewTarget;

        private Texture _viewStorage;

        private List<Texture> _views;

        /// <summary>
        /// Host texture.
        /// </summary>
        public ITexture HostTexture { get; private set; }

        /// <summary>
        /// Intrusive linked list node used on the auto deletion texture cache.
        /// </summary>
        public LinkedListNode<Texture> CacheNode { get; set; }

        /// <summary>
        /// Event to fire when texture data is modified by the GPU.
        /// </summary>
        public event Action<Texture> Modified;

        /// <summary>
        /// Event to fire when texture data is disposed.
        /// </summary>
        public event Action<Texture> Disposed;

        /// <summary>
        /// Start address of the texture in guest memory.
        /// </summary>
        public ulong Address => Info.Address;

        /// <summary>
        /// End address of the texture in guest memory.
        /// </summary>
        public ulong EndAddress => Info.Address + Size;

        /// <summary>
        /// Texture size in bytes.
        /// </summary>
        public ulong Size => (ulong)_sizeInfo.TotalSize;

        private (ulong, ulong)[] _modifiedRanges;

        private int _referenceCount;

        private int _sequenceNumber;

        /// <summary>
        /// Constructs a new instance of the cached GPU texture.
        /// </summary>
        /// <param name="context">GPU context that the texture belongs to</param>
        /// <param name="info">Texture information</param>
        /// <param name="sizeInfo">Size information of the texture</param>
        /// <param name="firstLayer">The first layer of the texture, or 0 if the texture has no parent</param>
        /// <param name="firstLevel">The first mipmap level of the texture, or 0 if the texture has no parent</param>
        /// <param name="scaleFactor">The floating point scale factor to initialize with</param>
        /// <param name="scaleMode">The scale mode to initialize with</param>
        private Texture(
            GpuContext       context,
            TextureInfo      info,
            SizeInfo         sizeInfo,
            int              firstLayer,
            int              firstLevel,
            float            scaleFactor,
            TextureScaleMode scaleMode)
        {
            InitializeTexture(context, info, sizeInfo);

            _firstLayer = firstLayer;
            _firstLevel = firstLevel;

            ScaleFactor = scaleFactor;
            ScaleMode = scaleMode;

            _hasData = true;
        }

        /// <summary>
        /// Constructs a new instance of the cached GPU texture.
        /// </summary>
        /// <param name="context">GPU context that the texture belongs to</param>
        /// <param name="info">Texture information</param>
        /// <param name="sizeInfo">Size information of the texture</param>
        /// <param name="scaleMode">The scale mode to initialize with. If scaled, the texture's data is loaded immediately and scaled up</param>
        public Texture(GpuContext context, TextureInfo info, SizeInfo sizeInfo, TextureScaleMode scaleMode)
        {
            ScaleFactor = 1f; // Texture is first loaded at scale 1x.
            ScaleMode = scaleMode;

            InitializeTexture(context, info, sizeInfo);

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(info, context.Capabilities);

            HostTexture = _context.Renderer.CreateTexture(createInfo, ScaleFactor);

            if (scaleMode == TextureScaleMode.Scaled)
            {
                SynchronizeMemory(); // Load the data and then scale it up.
                SetScale(GraphicsConfig.ResScale);
            }
        }

        /// <summary>
        /// Common texture initialization method.
        /// This sets the context, info and sizeInfo fields.
        /// Other fields are initialized with their default values.
        /// </summary>
        /// <param name="context">GPU context that the texture belongs to</param>
        /// <param name="info">Texture information</param>
        /// <param name="sizeInfo">Size information of the texture</param>
        private void InitializeTexture(GpuContext context, TextureInfo info, SizeInfo sizeInfo)
        {
            _context  = context;
            _sizeInfo = sizeInfo;

            _modifiedRanges = new (ulong, ulong)[(sizeInfo.TotalSize / PhysicalMemory.PageSize) + 1];

            SetInfo(info);

            _viewStorage = this;

            _views = new List<Texture>();
        }

        /// <summary>
        /// Create a texture view from this texture.
        /// A texture view is defined as a child texture, from a sub-range of their parent texture.
        /// For example, the initial layer and mipmap level of the view can be defined, so the texture
        /// will start at the given layer/level of the parent texture.
        /// </summary>
        /// <param name="info">Child texture information</param>
        /// <param name="sizeInfo">Child texture size information</param>
        /// <param name="firstLayer">Start layer of the child texture on the parent texture</param>
        /// <param name="firstLevel">Start mipmap level of the child texture on the parent texture</param>
        /// <returns>The child texture</returns>
        public Texture CreateView(TextureInfo info, SizeInfo sizeInfo, int firstLayer, int firstLevel)
        {
            Texture texture = new Texture(
                _context,
                info,
                sizeInfo,
                _firstLayer + firstLayer,
                _firstLevel + firstLevel,
                ScaleFactor,
                ScaleMode);

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(info, _context.Capabilities);

            texture.HostTexture = HostTexture.CreateView(createInfo, firstLayer, firstLevel);

            _viewStorage.AddView(texture);

            return texture;
        }

        /// <summary>
        /// Adds a child texture to this texture.
        /// </summary>
        /// <param name="texture">The child texture</param>
        private void AddView(Texture texture)
        {
            _views.Add(texture);

            texture._viewStorage = this;
        }

        /// <summary>
        /// Removes a child texture from this texture.
        /// </summary>
        /// <param name="texture">The child texture</param>
        private void RemoveView(Texture texture)
        {
            _views.Remove(texture);

            texture._viewStorage = null;

            DeleteIfNotUsed();
        }

        /// <summary>
        /// Changes the texture size.
        /// </summary>
        /// <remarks>
        /// This operation may also change the size of all mipmap levels, including from the parent
        /// and other possible child textures, to ensure that all sizes are consistent.
        /// </remarks>
        /// <param name="width">The new texture width</param>
        /// <param name="height">The new texture height</param>
        /// <param name="depthOrLayers">The new texture depth (for 3D textures) or layers (for layered textures)</param>
        public void ChangeSize(int width, int height, int depthOrLayers)
        {
            width  <<= _firstLevel;
            height <<= _firstLevel;

            if (Info.Target == Target.Texture3D)
            {
                depthOrLayers <<= _firstLevel;
            }
            else
            {
                depthOrLayers = _viewStorage.Info.DepthOrLayers;
            }

            _viewStorage.RecreateStorageOrView(width, height, depthOrLayers);

            foreach (Texture view in _viewStorage._views)
            {
                int viewWidth  = Math.Max(1, width  >> view._firstLevel);
                int viewHeight = Math.Max(1, height >> view._firstLevel);

                int viewDepthOrLayers;

                if (view.Info.Target == Target.Texture3D)
                {
                    viewDepthOrLayers = Math.Max(1, depthOrLayers >> view._firstLevel);
                }
                else
                {
                    viewDepthOrLayers = view.Info.DepthOrLayers;
                }

                view.RecreateStorageOrView(viewWidth, viewHeight, viewDepthOrLayers);
            }
        }

        /// <summary>
        /// Recreates the texture storage (or view, in the case of child textures) of this texture.
        /// This allows recreating the texture with a new size.
        /// A copy is automatically performed from the old to the new texture.
        /// </summary>
        /// <param name="width">The new texture width</param>
        /// <param name="height">The new texture height</param>
        /// <param name="depthOrLayers">The new texture depth (for 3D textures) or layers (for layered textures)</param>
        private void RecreateStorageOrView(int width, int height, int depthOrLayers)
        {
            SetInfo(new TextureInfo(
                Info.Address,
                width,
                height,
                depthOrLayers,
                Info.Levels,
                Info.SamplesInX,
                Info.SamplesInY,
                Info.Stride,
                Info.IsLinear,
                Info.GobBlocksInY,
                Info.GobBlocksInZ,
                Info.GobBlocksInTileX,
                Info.Target,
                Info.FormatInfo,
                Info.DepthStencilMode,
                Info.SwizzleR,
                Info.SwizzleG,
                Info.SwizzleB,
                Info.SwizzleA));

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(Info, _context.Capabilities);

            if (_viewStorage != this)
            {
                ReplaceStorage(_viewStorage.HostTexture.CreateView(createInfo, _firstLayer, _firstLevel));
            }
            else
            {
                ITexture newStorage = _context.Renderer.CreateTexture(createInfo, ScaleFactor);

                HostTexture.CopyTo(newStorage, 0, 0);

                ReplaceStorage(newStorage);
            }
        }

        /// <summary>
        /// Blacklists this texture from being scaled. Resets its scale to 1 if needed.
        /// </summary>
        public void BlacklistScale()
        {
            ScaleMode = TextureScaleMode.Blacklisted;
            SetScale(1f);
        }

        /// <summary>
        /// Propagates the scale between this texture and another to ensure they have the same scale.
        /// If one texture is blacklisted from scaling, the other will become blacklisted too.
        /// </summary>
        /// <param name="other">The other texture</param>
        public void PropagateScale(Texture other)
        {
            if (other.ScaleMode == TextureScaleMode.Blacklisted || ScaleMode == TextureScaleMode.Blacklisted)
            {
                BlacklistScale();
                other.BlacklistScale();
            }
            else
            {
                // Prefer the configured scale if present. If not, prefer the max.
                float targetScale = GraphicsConfig.ResScale;
                float sharedScale = (ScaleFactor == targetScale || other.ScaleFactor == targetScale) ? targetScale : Math.Max(ScaleFactor, other.ScaleFactor);

                SetScale(sharedScale);
                other.SetScale(sharedScale);
            }
        }

        /// <summary>
        /// Helper method for copying our Texture2DArray texture to the given target, with scaling.
        /// This creates temporary views for each array layer on both textures, copying each one at a time.
        /// </summary>
        /// <param name="target">The texture array to copy to</param>
        private void CopyArrayScaled(ITexture target)
        {
            TextureInfo viewInfo = new TextureInfo(
                Info.Address,
                Info.Width,
                Info.Height,
                1,
                Info.Levels,
                Info.SamplesInX,
                Info.SamplesInY,
                Info.Stride,
                Info.IsLinear,
                Info.GobBlocksInY,
                Info.GobBlocksInZ,
                Info.GobBlocksInTileX,
                Target.Texture2D,
                Info.FormatInfo,
                Info.DepthStencilMode,
                Info.SwizzleR,
                Info.SwizzleG,
                Info.SwizzleB,
                Info.SwizzleA);

            TextureCreateInfo createInfo = TextureManager.GetCreateInfo(viewInfo, _context.Capabilities);

            for (int i = 0; i < Info.DepthOrLayers; i++)
            {
                ITexture from = HostTexture.CreateView(createInfo, i, 0);
                ITexture to = target.CreateView(createInfo, i, 0);

                from.CopyTo(to, new Extents2D(0, 0, from.Width, from.Height), new Extents2D(0, 0, to.Width, to.Height), true);

                from.Dispose();
                to.Dispose();
            }
        }

        /// <summary>
        /// Sets the Scale Factor on this texture, and immediately recreates it at the correct size.
        /// When a texture is resized, a scaled copy is performed from the old texture to the new one, to ensure no data is lost.
        /// If scale is equivalent, this only propagates the blacklisted/scaled mode.
        /// If called on a view, its storage is resized instead.
        /// When resizing storage, all texture views are recreated.
        /// </summary>
        /// <param name="scale">The new scale factor for this texture</param>
        public void SetScale(float scale)
        {
            TextureScaleMode newScaleMode = ScaleMode == TextureScaleMode.Blacklisted ? ScaleMode : TextureScaleMode.Scaled;

            if (_viewStorage != this)
            {
                _viewStorage.ScaleMode = newScaleMode;
                _viewStorage.SetScale(scale);
                return;
            }

            if (ScaleFactor != scale)
            {
                Logger.PrintDebug(LogClass.Gpu, $"Rescaling {Info.Width}x{Info.Height} {Info.FormatInfo.Format.ToString()} to ({ScaleFactor} to {scale}). ");
                TextureCreateInfo createInfo = TextureManager.GetCreateInfo(Info, _context.Capabilities);

                ScaleFactor = scale;

                ITexture newStorage = _context.Renderer.CreateTexture(createInfo, ScaleFactor);

                if (Info.Target == Target.Texture2DArray)
                {
                    CopyArrayScaled(newStorage);
                }
                else
                {
                    HostTexture.CopyTo(newStorage, new Extents2D(0, 0, HostTexture.Width, HostTexture.Height), new Extents2D(0, 0, newStorage.Width, newStorage.Height), true);
                }

                Logger.PrintDebug(LogClass.Gpu, $"  Copy performed: {HostTexture.Width}x{HostTexture.Height} to {newStorage.Width}x{newStorage.Height}");

                ReplaceStorage(newStorage);

                // All views must be recreated against the new storage.

                foreach (var view in _views)
                {
                    Logger.PrintDebug(LogClass.Gpu, $"  Recreating view {Info.Width}x{Info.Height} {Info.FormatInfo.Format.ToString()}.");
                    view.ScaleFactor = scale;

                    TextureCreateInfo viewCreateInfo = TextureManager.GetCreateInfo(view.Info, _context.Capabilities);

                    ITexture newView = HostTexture.CreateView(viewCreateInfo, view._firstLayer - _firstLayer, view._firstLevel - _firstLevel);

                    view.ReplaceStorage(newView);

                    view.ScaleMode = newScaleMode;
                }
            }

            if (ScaleMode != newScaleMode)
            {
                ScaleMode = newScaleMode;

                foreach (var view in _views)
                {
                    view.ScaleMode = newScaleMode;
                }
            }
        }

        /// <summary>
        /// Synchronizes guest and host memory.
        /// This will overwrite the texture data with the texture data on the guest memory, if a CPU
        /// modification is detected.
        /// Be aware that this can cause texture data written by the GPU to be lost, this is just a
        /// one way copy (from CPU owned to GPU owned memory).
        /// </summary>
        public void SynchronizeMemory()
        {
            // Texture buffers are not handled here, instead they are invalidated (if modified)
            // when the texture is bound. This is handled by the buffer manager.
            if ((_sequenceNumber == _context.SequenceNumber && _hasData) || Info.Target == Target.TextureBuffer)
            {
                return;
            }

            _sequenceNumber = _context.SequenceNumber;

            int modifiedCount = _context.PhysicalMemory.QueryModified(Address, Size, ResourceName.Texture, _modifiedRanges);

            if (_hasData)
            {
                if (modifiedCount == 0)
                {
                    return;
                }

                BlacklistScale();
            }

            ReadOnlySpan<byte> data = _context.PhysicalMemory.GetSpan(Address, (int)Size);

            // If the texture was modified by the host GPU, we do partial invalidation
            // of the texture by getting GPU data and merging in the pages of memory
            // that were modified.
            // Note that if ASTC is not supported by the GPU we can't read it back since
            // it will use a different format. Since applications shouldn't be writing
            // ASTC textures from the GPU anyway, ignoring it should be safe.
            if (_context.Methods.TextureManager.IsTextureModified(this) && !Info.FormatInfo.Format.IsAstc())
            {
                Span<byte> gpuData = GetTextureDataFromGpu();

                ulong endAddress = Address + Size;

                for (int i = 0; i < modifiedCount; i++)
                {
                    (ulong modifiedAddress, ulong modifiedSize) = _modifiedRanges[i];

                    ulong endModifiedAddress = modifiedAddress + modifiedSize;

                    if (modifiedAddress < Address)
                    {
                        modifiedAddress = Address;
                    }

                    if (endModifiedAddress > endAddress)
                    {
                        endModifiedAddress = endAddress;
                    }

                    modifiedSize = endModifiedAddress - modifiedAddress;

                    int offset = (int)(modifiedAddress - Address);
                    int length = (int)modifiedSize;

                    data.Slice(offset, length).CopyTo(gpuData.Slice(offset, length));
                }

                data = gpuData;
            }

            data = ConvertToHostCompatibleFormat(data);

            HostTexture.SetData(data);

            _hasData = true;
        }

        /// <summary>
        /// Converts texture data to a format and layout that is supported by the host GPU.
        /// </summary>
        /// <param name="data">Data to be converted</param>
        /// <returns>Converted data</returns>
        private ReadOnlySpan<byte> ConvertToHostCompatibleFormat(ReadOnlySpan<byte> data)
        {
            if (Info.IsLinear)
            {
                data = LayoutConverter.ConvertLinearStridedToLinear(
                    Info.Width,
                    Info.Height,
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.Stride,
                    Info.FormatInfo.BytesPerPixel,
                    data);
            }
            else
            {
                data = LayoutConverter.ConvertBlockLinearToLinear(
                    Info.Width,
                    Info.Height,
                    _depth,
                    Info.Levels,
                    _layers,
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.FormatInfo.BytesPerPixel,
                    Info.GobBlocksInY,
                    Info.GobBlocksInZ,
                    Info.GobBlocksInTileX,
                    _sizeInfo,
                    data);
            }

            if (!_context.Capabilities.SupportsAstcCompression && Info.FormatInfo.Format.IsAstc())
            {
                if (!AstcDecoder.TryDecodeToRgba8(
                    data.ToArray(),
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.Width,
                    Info.Height,
                    _depth,
                    Info.Levels,
                    out Span<byte> decoded))
                {
                    string texInfo = $"{Info.Target} {Info.FormatInfo.Format} {Info.Width}x{Info.Height}x{Info.DepthOrLayers} levels {Info.Levels}";

                    Logger.PrintDebug(LogClass.Gpu, $"Invalid ASTC texture at 0x{Info.Address:X} ({texInfo}).");
                }

                data = decoded;
            }

            return data;
        }

        /// <summary>
        /// Flushes the texture data.
        /// This causes the texture data to be written back to guest memory.
        /// If the texture was written by the GPU, this includes all modification made by the GPU
        /// up to this point.
        /// Be aware that this is an expensive operation, avoid calling it unless strictly needed.
        /// This may cause data corruption if the memory is already being used for something else on the CPU side.
        /// </summary>
        public void Flush()
        {
            BlacklistScale();
            _context.PhysicalMemory.Write(Address, GetTextureDataFromGpu());
        }

        /// <summary>
        /// Gets data from the host GPU.
        /// </summary>
        /// <remarks>
        /// This method should be used to retrieve data that was modified by the host GPU.
        /// This is not cheap, avoid doing that unless strictly needed.
        /// </remarks>
        /// <returns>Host texture data</returns>
        private Span<byte> GetTextureDataFromGpu()
        {
            BlacklistScale();
            Span<byte> data = HostTexture.GetData();

            if (Info.IsLinear)
            {
                data = LayoutConverter.ConvertLinearToLinearStrided(
                    Info.Width,
                    Info.Height,
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.Stride,
                    Info.FormatInfo.BytesPerPixel,
                    data);
            }
            else
            {
                data = LayoutConverter.ConvertLinearToBlockLinear(
                    Info.Width,
                    Info.Height,
                    _depth,
                    Info.Levels,
                    _layers,
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.FormatInfo.BytesPerPixel,
                    Info.GobBlocksInY,
                    Info.GobBlocksInZ,
                    Info.GobBlocksInTileX,
                    _sizeInfo,
                    data);
            }

            return data;
        }

        /// <summary>
        /// Performs a comparison of this texture information, with the specified texture information.
        /// This performs a strict comparison, used to check if two textures are equal.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <param name="flags">Comparison flags</param>
        /// <returns>True if the textures are strictly equal or similar, false otherwise</returns>
        public bool IsPerfectMatch(TextureInfo info, TextureSearchFlags flags)
        {
            if (!FormatMatches(info, (flags & TextureSearchFlags.ForSampler) != 0, (flags & TextureSearchFlags.ForCopy) != 0))
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

            if ((flags & TextureSearchFlags.ForSampler) != 0 || (flags & TextureSearchFlags.Strict) != 0)
            {
                if (!SamplerParamsMatches(info))
                {
                    return false;
                }
            }

            if ((flags & TextureSearchFlags.ForCopy) != 0)
            {
                bool msTargetCompatible = Info.Target == Target.Texture2DMultisample && info.Target == Target.Texture2D;

                if (!msTargetCompatible && !TargetAndSamplesCompatible(info))
                {
                    return false;
                }
            }
            else if (!TargetAndSamplesCompatible(info))
            {
                return false;
            }

            return Info.Address == info.Address && Info.Levels == info.Levels;
        }

        /// <summary>
        /// Checks if the texture format matches with the specified texture information.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <param name="forSampler">Indicates that the texture will be used for shader sampling</param>
        /// <param name="forCopy">Indicates that the texture will be used as copy source or target</param>
        /// <returns>True if the format matches, with the given comparison rules</returns>
        private bool FormatMatches(TextureInfo info, bool forSampler, bool forCopy)
        {
            // D32F and R32F texture have the same representation internally,
            // however the R32F format is used to sample from depth textures.
            if (Info.FormatInfo.Format == Format.D32Float && info.FormatInfo.Format == Format.R32Float && (forSampler || forCopy))
            {
                return true;
            }

            if (forCopy)
            {
                // The 2D engine does not support depth-stencil formats, so it will instead
                // use equivalent color formats. We must also consider them as compatible.
                if (Info.FormatInfo.Format == Format.S8Uint && info.FormatInfo.Format == Format.R8Unorm)
                {
                    return true;
                }

                if (Info.FormatInfo.Format == Format.D16Unorm && info.FormatInfo.Format == Format.R16Unorm)
                {
                    return true;
                }

                if ((Info.FormatInfo.Format == Format.D24UnormS8Uint ||
                     Info.FormatInfo.Format == Format.D24X8Unorm) && info.FormatInfo.Format == Format.B8G8R8A8Unorm)
                {
                    return true;
                }
            }

            return Info.FormatInfo.Format == info.FormatInfo.Format;
        }

        /// <summary>
        /// Checks if the texture layout specified matches with this texture layout.
        /// The layout information is composed of the Stride for linear textures, or GOB block size
        /// for block linear textures.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <returns>True if the layout matches, false otherwise</returns>
        private bool LayoutMatches(TextureInfo info)
        {
            if (Info.IsLinear != info.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (info.IsLinear)
            {
                return Info.Stride == info.Stride;
            }
            else
            {
                return Info.GobBlocksInY == info.GobBlocksInY &&
                       Info.GobBlocksInZ == info.GobBlocksInZ;
            }
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture information matches this texture.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <returns>True if the size matches, false otherwise</returns>
        public bool SizeMatches(TextureInfo info)
        {
            return SizeMatches(info, alignSizes: false);
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture information matches the given level of
        /// this texture.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <param name="level">Mipmap level of this texture to compare with</param>
        /// <returns>True if the size matches with the level, false otherwise</returns>
        public bool SizeMatches(TextureInfo info, int level)
        {
            return Math.Max(1, Info.Width      >> level) == info.Width  &&
                   Math.Max(1, Info.Height     >> level) == info.Height &&
                   Math.Max(1, Info.GetDepth() >> level) == info.GetDepth();
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture information matches this texture.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <param name="alignSizes">True to align the sizes according to the texture layout for comparison</param>
        /// <returns>True if the sizes matches, false otherwise</returns>
        private bool SizeMatches(TextureInfo info, bool alignSizes)
        {
            if (Info.GetLayers() != info.GetLayers())
            {
                return false;
            }

            if (alignSizes)
            {
                Size size0 = GetAlignedSize(Info);
                Size size1 = GetAlignedSize(info);

                return size0.Width  == size1.Width  &&
                       size0.Height == size1.Height &&
                       size0.Depth  == size1.Depth;
            }
            else
            {
                return Info.Width      == info.Width  &&
                       Info.Height     == info.Height &&
                       Info.GetDepth() == info.GetDepth();
            }
        }

        /// <summary>
        /// Checks if the texture shader sampling parameters matches.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <returns>True if the texture shader sampling parameters matches, false otherwise</returns>
        private bool SamplerParamsMatches(TextureInfo info)
        {
            return Info.DepthStencilMode == info.DepthStencilMode &&
                   Info.SwizzleR         == info.SwizzleR         &&
                   Info.SwizzleG         == info.SwizzleG         &&
                   Info.SwizzleB         == info.SwizzleB         &&
                   Info.SwizzleA         == info.SwizzleA;
        }

        /// <summary>
        /// Check if the texture target and samples count (for multisampled textures) matches.
        /// </summary>
        /// <param name="info">Texture information to compare with</param>
        /// <returns>True if the texture target and samples count matches, false otherwise</returns>
        private bool TargetAndSamplesCompatible(TextureInfo info)
        {
            return Info.Target     == info.Target     &&
                   Info.SamplesInX == info.SamplesInX &&
                   Info.SamplesInY == info.SamplesInY;
        }

        /// <summary>
        /// Check if it's possible to create a view, with the given parameters, from this texture.
        /// </summary>
        /// <param name="info">Texture view information</param>
        /// <param name="size">Texture view size</param>
        /// <param name="firstLayer">Texture view initial layer on this texture</param>
        /// <param name="firstLevel">Texture view first mipmap level on this texture</param>
        /// <returns>True if a view with the given parameters can be created from this texture, false otherwise</returns>
        public bool IsViewCompatible(
            TextureInfo info,
            ulong       size,
            out int     firstLayer,
            out int     firstLevel)
        {
            return IsViewCompatible(info, size, isCopy: false, out firstLayer, out firstLevel);
        }

        /// <summary>
        /// Check if it's possible to create a view, with the given parameters, from this texture.
        /// </summary>
        /// <param name="info">Texture view information</param>
        /// <param name="size">Texture view size</param>
        /// <param name="isCopy">True to check for copy compability, instead of view compatibility</param>
        /// <param name="firstLayer">Texture view initial layer on this texture</param>
        /// <param name="firstLevel">Texture view first mipmap level on this texture</param>
        /// <returns>True if a view with the given parameters can be created from this texture, false otherwise</returns>
        public bool IsViewCompatible(
            TextureInfo info,
            ulong       size,
            bool        isCopy,
            out int     firstLayer,
            out int     firstLevel)
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

            if (!ViewSizeMatches(info, firstLevel, isCopy))
            {
                return false;
            }

            if (!ViewTargetCompatible(info, isCopy))
            {
                return false;
            }

            return Info.SamplesInX == info.SamplesInX &&
                   Info.SamplesInY == info.SamplesInY;
        }

        /// <summary>
        /// Check if it's possible to create a view with the specified layout.
        /// The layout information is composed of the Stride for linear textures, or GOB block size
        /// for block linear textures.
        /// </summary>
        /// <param name="info">Texture information of the texture view</param>
        /// <param name="level">Start level of the texture view, in relation with this texture</param>
        /// <returns>True if the layout is compatible, false otherwise</returns>
        private bool ViewLayoutCompatible(TextureInfo info, int level)
        {
            if (Info.IsLinear != info.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (info.IsLinear)
            {
                int width = Math.Max(1, Info.Width >> level);

                int stride = width * Info.FormatInfo.BytesPerPixel;

                stride = BitUtils.AlignUp(stride, 32);

                return stride == info.Stride;
            }
            else
            {
                int height = Math.Max(1, Info.Height     >> level);
                int depth  = Math.Max(1, Info.GetDepth() >> level);

                (int gobBlocksInY, int gobBlocksInZ) = SizeCalculator.GetMipGobBlockSizes(
                    height,
                    depth,
                    Info.FormatInfo.BlockHeight,
                    Info.GobBlocksInY,
                    Info.GobBlocksInZ);

                return gobBlocksInY == info.GobBlocksInY &&
                       gobBlocksInZ == info.GobBlocksInZ;
            }
        }

        /// <summary>
        /// Checks if the view format is compatible with this texture format.
        /// In general, the formats are considered compatible if the bytes per pixel values are equal,
        /// but there are more complex rules for some formats, like compressed or depth-stencil formats.
        /// This follows the host API copy compatibility rules.
        /// </summary>
        /// <param name="info">Texture information of the texture view</param>
        /// <returns>True if the formats are compatible, false otherwise</returns>
        private bool ViewFormatCompatible(TextureInfo info)
        {
            return TextureCompatibility.FormatCompatible(Info.FormatInfo, info.FormatInfo);
        }

        /// <summary>
        /// Checks if the size of a given texture view is compatible with this texture.
        /// </summary>
        /// <param name="info">Texture information of the texture view</param>
        /// <param name="level">Mipmap level of the texture view in relation to this texture</param>
        /// <param name="isCopy">True to check for copy compatibility rather than view compatibility</param>
        /// <returns>True if the sizes are compatible, false otherwise</returns>
        private bool ViewSizeMatches(TextureInfo info, int level, bool isCopy)
        {
            Size size = GetAlignedSize(Info, level);

            Size otherSize = GetAlignedSize(info);

            // For copies, we can copy a subset of the 3D texture slices,
            // so the depth may be different in this case.
            if (!isCopy && info.Target == Target.Texture3D && size.Depth != otherSize.Depth)
            {
                return false;
            }

            return size.Width  == otherSize.Width &&
                   size.Height == otherSize.Height;
        }

        /// <summary>
        /// Check if the target of the specified texture view information is compatible with this
        /// texture.
        /// This follows the host API target compatibility rules.
        /// </summary>
        /// <param name="info">Texture information of the texture view</param>
        /// <param name="isCopy">True to check for copy rather than view compatibility</param>
        /// <returns>True if the targets are compatible, false otherwise</returns>
        private bool ViewTargetCompatible(TextureInfo info, bool isCopy)
        {
            switch (Info.Target)
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
                    return info.Target == Target.Texture3D ||
                          (info.Target == Target.Texture2D && isCopy);
            }

            return false;
        }

        /// <summary>
        /// Gets the aligned sizes of the specified texture information.
        /// The alignment depends on the texture layout and format bytes per pixel.
        /// </summary>
        /// <param name="info">Texture information to calculate the aligned size from</param>
        /// <param name="level">Mipmap level for texture views</param>
        /// <returns>The aligned texture size</returns>
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

                return SizeCalculator.GetBlockLinearAlignedSize(
                    width,
                    height,
                    depth,
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel,
                    info.GobBlocksInY,
                    info.GobBlocksInZ,
                    info.GobBlocksInTileX);
            }
        }

        /// <summary>
        /// Gets a texture of the specified target type from this texture.
        /// This can be used to get an array texture from a non-array texture and vice-versa.
        /// If this texture and the requested targets are equal, then this texture Host texture is returned directly.
        /// </summary>
        /// <param name="target">The desired target type</param>
        /// <returns>A view of this texture with the requested target, or null if the target is invalid for this texture</returns>
        public ITexture GetTargetTexture(Target target)
        {
            if (target == Info.Target)
            {
                return HostTexture;
            }

            if (_arrayViewTexture == null && IsSameDimensionsTarget(target))
            {
                TextureCreateInfo createInfo = new TextureCreateInfo(
                    Info.Width,
                    Info.Height,
                    target == Target.CubemapArray ? 6 : 1,
                    Info.Levels,
                    Info.Samples,
                    Info.FormatInfo.BlockWidth,
                    Info.FormatInfo.BlockHeight,
                    Info.FormatInfo.BytesPerPixel,
                    Info.FormatInfo.Format,
                    Info.DepthStencilMode,
                    target,
                    Info.SwizzleR,
                    Info.SwizzleG,
                    Info.SwizzleB,
                    Info.SwizzleA);

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

        /// <summary>
        /// Check if this texture and the specified target have the same number of dimensions.
        /// For the purposes of this comparison, 2D and 2D Multisample textures are not considered to have
        /// the same number of dimensions. Same for Cubemap and 3D textures.
        /// </summary>
        /// <param name="target">The target to compare with</param>
        /// <returns>True if both targets have the same number of dimensions, false otherwise</returns>
        private bool IsSameDimensionsTarget(Target target)
        {
            switch (Info.Target)
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

        /// <summary>
        /// Replaces view texture information.
        /// This should only be used for child textures with a parent.
        /// </summary>
        /// <param name="parent">The parent texture</param>
        /// <param name="info">The new view texture information</param>
        /// <param name="hostTexture">The new host texture</param>
        /// <param name="firstLayer">The first layer of the view</param>
        /// <param name="firstLevel">The first level of the view</param>
        public void ReplaceView(Texture parent, TextureInfo info, ITexture hostTexture, int firstLayer, int firstLevel)
        {
            ReplaceStorage(hostTexture);

            _firstLayer = parent._firstLayer + firstLayer;
            _firstLevel = parent._firstLevel + firstLevel;
            parent._viewStorage.AddView(this);

            SetInfo(info);
        }

        /// <summary>
        /// Sets the internal texture information structure.
        /// </summary>
        /// <param name="info">The new texture information</param>
        private void SetInfo(TextureInfo info)
        {
            Info = info;

            _depth  = info.GetDepth();
            _layers = info.GetLayers();
        }

        /// <summary>
        /// Signals that the texture has been modified.
        /// </summary>
        public void SignalModified()
        {
            Modified?.Invoke(this);
        }

        /// <summary>
        /// Replaces the host texture, while disposing of the old one if needed.
        /// </summary>
        /// <param name="hostTexture">The new host texture</param>
        private void ReplaceStorage(ITexture hostTexture)
        {
            DisposeTextures();

            HostTexture = hostTexture;
        }

        /// <summary>
        /// Checks if the texture overlaps with a memory range.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <returns>True if the texture overlaps with the range, false otherwise</returns>
        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        /// <summary>
        /// Increments the texture reference count.
        /// </summary>
        public void IncrementReferenceCount()
        {
            _referenceCount++;
        }

        /// <summary>
        /// Decrements the texture reference count.
        /// When the reference count hits zero, the texture may be deleted and can't be used anymore.
        /// </summary>
        public void DecrementReferenceCount()
        {
            int newRefCount = --_referenceCount;

            if (newRefCount == 0)
            {
                if (_viewStorage != this)
                {
                    _viewStorage.RemoveView(this);
                }

                _context.Methods.TextureManager.RemoveTextureFromCache(this);
            }

            Debug.Assert(newRefCount >= 0);

            DeleteIfNotUsed();
        }

        /// <summary>
        /// Delete the texture if it is not used anymore.
        /// The texture is considered unused when the reference count is zero,
        /// and it has no child views.
        /// </summary>
        private void DeleteIfNotUsed()
        {
            // We can delete the texture as long it is not being used
            // in any cache (the reference count is 0 in this case), and
            // also all views that may be created from this texture were
            // already deleted (views count is 0).
            if (_referenceCount == 0 && _views.Count == 0)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Performs texture disposal, deleting the texture.
        /// </summary>
        private void DisposeTextures()
        {
            HostTexture.Dispose();

            _arrayViewTexture?.Dispose();
            _arrayViewTexture = null;
        }

        /// <summary>
        /// Performs texture disposal, deleting the texture.
        /// </summary>
        public void Dispose()
        {
            DisposeTextures();

            Disposed?.Invoke(this);
        }
    }
}