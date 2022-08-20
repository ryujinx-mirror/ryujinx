using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class TextureView : ITexture, IDisposable
    {
        private readonly VulkanRenderer _gd;

        private readonly Device _device;

        private readonly Auto<DisposableImageView> _imageView;
        private readonly Auto<DisposableImageView> _imageViewIdentity;
        private readonly Auto<DisposableImageView> _imageView2dArray;
        private Dictionary<GAL.Format, TextureView> _selfManagedViews;

        private TextureCreateInfo _info;

        public TextureCreateInfo Info => _info;

        public TextureStorage Storage { get; }

        public int Width => Info.Width;
        public int Height => Info.Height;
        public int Layers => Info.GetDepthOrLayers();
        public int FirstLayer { get; }
        public int FirstLevel { get; }
        public float ScaleFactor => Storage.ScaleFactor;
        public VkFormat VkFormat { get; }
        public bool Valid { get; private set; }

        public TextureView(
            VulkanRenderer gd,
            Device device,
            TextureCreateInfo info,
            TextureStorage storage,
            int firstLayer,
            int firstLevel)
        {
            _gd = gd;
            _device = device;
            _info = info;
            Storage = storage;
            FirstLayer = firstLayer;
            FirstLevel = firstLevel;

            storage.IncrementViewsCount();

            gd.Textures.Add(this);

            var format = _gd.FormatCapabilities.ConvertToVkFormat(info.Format);
            var levels = (uint)info.Levels;
            var layers = (uint)info.GetLayers();

            VkFormat = format;

            var type = info.Target.ConvertView();

            var swizzleR = info.SwizzleR.Convert();
            var swizzleG = info.SwizzleG.Convert();
            var swizzleB = info.SwizzleB.Convert();
            var swizzleA = info.SwizzleA.Convert();

            if (info.Format == GAL.Format.R5G5B5A1Unorm ||
                info.Format == GAL.Format.R5G5B5X1Unorm ||
                info.Format == GAL.Format.R5G6B5Unorm)
            {
                var temp = swizzleR;

                swizzleR = swizzleB;
                swizzleB = temp;
            }
            else if (info.Format == GAL.Format.R4G4B4A4Unorm)
            {
                var tempG = swizzleG;
                var tempB = swizzleB;

                swizzleB = swizzleA;
                swizzleG = swizzleR;
                swizzleR = tempG;
                swizzleA = tempB;
            }
            else if (info.Format == GAL.Format.A1B5G5R5Unorm)
            {
                var tempB = swizzleB;
                var tempA = swizzleA;

                swizzleB = swizzleG;
                swizzleA = swizzleR;
                swizzleR = tempA;
                swizzleG = tempB;
            }

            var componentMapping = new ComponentMapping(swizzleR, swizzleG, swizzleB, swizzleA);

            var aspectFlags = info.Format.ConvertAspectFlags(info.DepthStencilMode);
            var aspectFlagsDepth = info.Format.ConvertAspectFlags(DepthStencilMode.Depth);

            var subresourceRange = new ImageSubresourceRange(aspectFlags, (uint)firstLevel, levels, (uint)firstLayer, layers);
            var subresourceRangeDepth = new ImageSubresourceRange(aspectFlagsDepth, (uint)firstLevel, levels, (uint)firstLayer, layers);

            unsafe Auto<DisposableImageView> CreateImageView(ComponentMapping cm, ImageSubresourceRange sr, ImageViewType viewType)
            {
                var imageCreateInfo = new ImageViewCreateInfo()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = storage.GetImageForViewCreation(),
                    ViewType = viewType,
                    Format = format,
                    Components = cm,
                    SubresourceRange = sr
                };

                gd.Api.CreateImageView(device, imageCreateInfo, null, out var imageView).ThrowOnError();
                return new Auto<DisposableImageView>(new DisposableImageView(gd.Api, device, imageView), null, storage.GetImage());
            }

            _imageView = CreateImageView(componentMapping, subresourceRange, type);

            // Framebuffer attachments and storage images requires a identity component mapping.
            var identityComponentMapping = new ComponentMapping(
                ComponentSwizzle.R,
                ComponentSwizzle.G,
                ComponentSwizzle.B,
                ComponentSwizzle.A);

            _imageViewIdentity = CreateImageView(identityComponentMapping, subresourceRangeDepth, type);

            // Framebuffer attachments also require 3D textures to be bound as 2D array.
            if (info.Target == Target.Texture3D)
            {
                subresourceRange = new ImageSubresourceRange(aspectFlags, (uint)firstLevel, levels, (uint)firstLayer, (uint)info.Depth);

                _imageView2dArray = CreateImageView(identityComponentMapping, subresourceRange, ImageViewType.ImageViewType2DArray);
            }

            Valid = true;
        }

        public Auto<DisposableImage> GetImage()
        {
            return Storage.GetImage();
        }

        public Auto<DisposableImageView> GetImageView()
        {
            return _imageView;
        }

        public Auto<DisposableImageView> GetIdentityImageView()
        {
            return _imageViewIdentity;
        }

        public Auto<DisposableImageView> GetImageViewForAttachment()
        {
            return _imageView2dArray ?? _imageViewIdentity;
        }

        public void CopyTo(ITexture destination, int firstLayer, int firstLevel)
        {
            var src = this;
            var dst = (TextureView)destination;

            if (!Valid || !dst.Valid)
            {
                return;
            }

            _gd.PipelineInternal.EndRenderPass();

            var cbs = _gd.PipelineInternal.CurrentCommandBuffer;

            var srcImage = src.GetImage().Get(cbs).Value;
            var dstImage = dst.GetImage().Get(cbs).Value;

            if (src.Info.Target.IsMultisample())
            {
                int depth = Math.Min(src.Info.Depth, dst.Info.Depth - firstLayer);
                int levels = Math.Min(src.Info.Levels, dst.Info.Levels - firstLevel);

                CopyMSToNonMS(_gd, cbs, src, dst, srcImage, dstImage, 0, firstLayer, 0, firstLevel, depth, levels);
            }
            else
            {
                TextureCopy.Copy(
                    _gd.Api,
                    cbs.CommandBuffer,
                    srcImage,
                    dstImage,
                    src.Info,
                    dst.Info,
                    src.FirstLayer,
                    dst.FirstLayer,
                    src.FirstLevel,
                    dst.FirstLevel,
                    0,
                    firstLayer,
                    0,
                    firstLevel);
            }
        }

        public void CopyTo(ITexture destination, int srcLayer, int dstLayer, int srcLevel, int dstLevel)
        {
            var src = this;
            var dst = (TextureView)destination;

            if (!Valid || !dst.Valid)
            {
                return;
            }

            _gd.PipelineInternal.EndRenderPass();

            var cbs = _gd.PipelineInternal.CurrentCommandBuffer;

            var srcImage = src.GetImage().Get(cbs).Value;
            var dstImage = dst.GetImage().Get(cbs).Value;

            if (src.Info.Target.IsMultisample())
            {
                CopyMSToNonMS(_gd, cbs, src, dst, srcImage, dstImage, srcLayer, dstLayer, srcLevel, dstLevel, 1, 1);
            }
            else
            {
                TextureCopy.Copy(
                    _gd.Api,
                    cbs.CommandBuffer,
                    srcImage,
                    dstImage,
                    src.Info,
                    dst.Info,
                    src.FirstLayer,
                    dst.FirstLayer,
                    src.FirstLevel,
                    dst.FirstLevel,
                    srcLayer,
                    dstLayer,
                    srcLevel,
                    dstLevel,
                    1,
                    1);
            }
        }

        private static void CopyMSToNonMS(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            TextureView dst,
            Image srcImage,
            Image dstImage,
            int srcLayer,
            int dstLayer,
            int srcLevel,
            int dstLevel,
            int layers,
            int levels)
        {
            bool differentFormats = src.Info.Format != dst.Info.Format;

            var target = src.Info.Target switch
            {
                Target.Texture2D => Target.Texture2DMultisample,
                Target.Texture2DArray => Target.Texture2DMultisampleArray,
                Target.Texture2DMultisampleArray => Target.Texture2DArray,
                _ => Target.Texture2D
            };

            var intermmediateTarget = differentFormats ? dst.Info.Target : target;
            using var intermmediate = CreateIntermmediateTexture(gd, src, ref dst._info, intermmediateTarget, layers, levels);
            var intermmediateImage = intermmediate.GetImage().Get(cbs).Value;

            if (differentFormats)
            {
                // If the formats are different, the resolve would perform format conversion.
                // So we need yet another intermmediate texture and do a copy to reinterpret the
                // data into the correct (destination) format, without doing any sort of conversion.
                using var intermmediate2 = CreateIntermmediateTexture(gd, src, ref src._info, target, layers, levels);
                var intermmediate2Image = intermmediate2.GetImage().Get(cbs).Value;

                TextureCopy.Copy(
                    gd.Api,
                    cbs.CommandBuffer,
                    srcImage,
                    intermmediate2Image,
                    src.Info,
                    intermmediate2.Info,
                    src.FirstLayer,
                    0,
                    src.FirstLevel,
                    0,
                    srcLayer,
                    0,
                    srcLevel,
                    0,
                    layers,
                    levels);

                TextureCopy.Copy(
                    gd.Api,
                    cbs.CommandBuffer,
                    intermmediate2Image,
                    intermmediateImage,
                    intermmediate2.Info,
                    intermmediate.Info,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    layers,
                    levels);
            }
            else
            {
                TextureCopy.Copy(
                    gd.Api,
                    cbs.CommandBuffer,
                    srcImage,
                    intermmediateImage,
                    src.Info,
                    intermmediate.Info,
                    src.FirstLayer,
                    0,
                    src.FirstLevel,
                    0,
                    srcLayer,
                    0,
                    srcLevel,
                    0,
                    layers,
                    levels);
            }

            var srcRegion = new Extents2D(0, 0, src.Width, src.Height);
            var dstRegion = new Extents2D(0, 0, dst.Width, dst.Height);

            TextureCopy.Blit(
                gd.Api,
                cbs.CommandBuffer,
                intermmediateImage,
                dstImage,
                intermmediate.Info,
                dst.Info,
                srcRegion,
                dstRegion,
                0,
                dst.FirstLevel + dstLevel,
                0,
                dst.FirstLayer + dstLayer,
                layers,
                levels,
                true,
                ImageAspectFlags.ImageAspectColorBit,
                ImageAspectFlags.ImageAspectColorBit);
        }

        private static TextureView CreateIntermmediateTexture(VulkanRenderer gd, TextureView src, ref TextureCreateInfo formatInfo, Target target, int depth, int levels)
        {
            return gd.CreateTextureView(new GAL.TextureCreateInfo(
                src.Width,
                src.Height,
                depth,
                levels,
                1,
                formatInfo.BlockWidth,
                formatInfo.BlockHeight,
                formatInfo.BytesPerPixel,
                formatInfo.Format,
                DepthStencilMode.Depth,
                target,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha), 1f);
        }

        public void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            var dst = (TextureView)destination;

            if (_gd.CommandBufferPool.OwnedByCurrentThread)
            {
                _gd.PipelineInternal.EndRenderPass();

                var cbs = _gd.PipelineInternal.CurrentCommandBuffer;

                CopyToImpl(cbs, dst, srcRegion, dstRegion, linearFilter);
            }
            else
            {
                var cbp = _gd.BackgroundResources.Get().GetPool();

                using var cbs = cbp.Rent();

                CopyToImpl(cbs, dst, srcRegion, dstRegion, linearFilter);
            }
        }

        private void CopyToImpl(CommandBufferScoped cbs, TextureView dst, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            var src = this;

            var srcFormat = GetCompatibleGalFormat(src.Info.Format);
            var dstFormat = GetCompatibleGalFormat(dst.Info.Format);

            bool srcUsesStorageFormat = src.VkFormat == src.Storage.VkFormat;
            bool dstUsesStorageFormat = dst.VkFormat == dst.Storage.VkFormat;

            int layers = Math.Min(dst.Info.GetDepthOrLayers(), src.Info.GetDepthOrLayers());
            int levels = Math.Min(dst.Info.Levels, src.Info.Levels);

            if (srcUsesStorageFormat && dstUsesStorageFormat)
            {
                if ((srcRegion.X1 | dstRegion.X1) == 0 &&
                    (srcRegion.Y1 | dstRegion.Y1) == 0 &&
                    srcRegion.X2 == src.Width &&
                    srcRegion.Y2 == src.Height &&
                    dstRegion.X2 == dst.Width &&
                    dstRegion.Y2 == dst.Height &&
                    src.Width == dst.Width &&
                    src.Height == dst.Height &&
                    src.VkFormat == dst.VkFormat)
                {
                    TextureCopy.Copy(
                        _gd.Api,
                        cbs.CommandBuffer,
                        src.GetImage().Get(cbs).Value,
                        dst.GetImage().Get(cbs).Value,
                        src.Info,
                        dst.Info,
                        src.FirstLayer,
                        dst.FirstLayer,
                        src.FirstLevel,
                        dst.FirstLevel,
                        0,
                        0,
                        0,
                        0,
                        layers,
                        levels);

                    return;
                }
                else if (_gd.FormatCapabilities.OptimalFormatSupports(FormatFeatureFlags.FormatFeatureBlitSrcBit, srcFormat) &&
                         _gd.FormatCapabilities.OptimalFormatSupports(FormatFeatureFlags.FormatFeatureBlitDstBit, dstFormat))
                {
                    TextureCopy.Blit(
                        _gd.Api,
                        cbs.CommandBuffer,
                        src.GetImage().Get(cbs).Value,
                        dst.GetImage().Get(cbs).Value,
                        src.Info,
                        dst.Info,
                        srcRegion,
                        dstRegion,
                        src.FirstLayer,
                        dst.FirstLayer,
                        src.FirstLevel,
                        dst.FirstLevel,
                        layers,
                        levels,
                        linearFilter);

                    return;
                }
                else if (srcFormat == GAL.Format.D32FloatS8Uint && srcFormat == dstFormat && SupportsBlitFromD32FS8ToD32FAndS8())
                {
                    BlitDepthStencilWithBuffer(_gd, cbs, src, dst, srcRegion, dstRegion);

                    return;
                }
            }

            if (VulkanConfiguration.UseSlowSafeBlitOnAmd &&
                _gd.Vendor == Vendor.Amd &&
                src.Info.Target == Target.Texture2D &&
                dst.Info.Target == Target.Texture2D &&
                !dst.Info.Format.IsDepthOrStencil())
            {
                _gd.HelperShader.Blit(
                    _gd,
                    src,
                    dst.GetIdentityImageView(),
                    dst.Width,
                    dst.Height,
                    dst.VkFormat,
                    srcRegion,
                    dstRegion,
                    linearFilter);

                return;
            }

            Auto<DisposableImage> srcImage;
            Auto<DisposableImage> dstImage;

            if (dst.Info.Format.IsDepthOrStencil())
            {
                srcImage = src.Storage.CreateAliasedColorForDepthStorageUnsafe(srcFormat).GetImage();
                dstImage = dst.Storage.CreateAliasedColorForDepthStorageUnsafe(dstFormat).GetImage();
            }
            else
            {
                srcImage = src.Storage.CreateAliasedStorageUnsafe(srcFormat).GetImage();
                dstImage = dst.Storage.CreateAliasedStorageUnsafe(dstFormat).GetImage();
            }

            TextureCopy.Blit(
                _gd.Api,
                cbs.CommandBuffer,
                srcImage.Get(cbs).Value,
                dstImage.Get(cbs).Value,
                src.Info,
                dst.Info,
                srcRegion,
                dstRegion,
                src.FirstLevel,
                dst.FirstLevel,
                src.FirstLayer,
                dst.FirstLayer,
                layers,
                levels,
                linearFilter,
                ImageAspectFlags.ImageAspectColorBit,
                ImageAspectFlags.ImageAspectColorBit);
        }

        private static void BlitDepthStencilWithBuffer(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            TextureView dst,
            Extents2D srcRegion,
            Extents2D dstRegion)
        {
            int drBaseX = Math.Min(dstRegion.X1, dstRegion.X2);
            int drBaseY = Math.Min(dstRegion.Y1, dstRegion.Y2);
            int drWidth = Math.Abs(dstRegion.X2 - dstRegion.X1);
            int drHeight = Math.Abs(dstRegion.Y2 - dstRegion.Y1);

            var drOriginZero = new Extents2D(
                dstRegion.X1 - drBaseX,
                dstRegion.Y1 - drBaseY,
                dstRegion.X2 - drBaseX,
                dstRegion.Y2 - drBaseY);

            var d32SrcStorageInfo = TextureStorage.NewCreateInfoWith(ref src._info, GAL.Format.D32Float, 4);
            var d32DstStorageInfo = TextureStorage.NewCreateInfoWith(ref dst._info, GAL.Format.D32Float, 4, drWidth, drHeight);
            var s8SrcStorageInfo = TextureStorage.NewCreateInfoWith(ref src._info, GAL.Format.S8Uint, 1);
            var s8DstStorageInfo = TextureStorage.NewCreateInfoWith(ref dst._info, GAL.Format.S8Uint, 1, drWidth, drHeight);

            using var d32SrcStorage = gd.CreateTextureStorage(d32SrcStorageInfo, src.Storage.ScaleFactor);
            using var d32DstStorage = gd.CreateTextureStorage(d32DstStorageInfo, dst.Storage.ScaleFactor);
            using var s8SrcStorage = gd.CreateTextureStorage(s8SrcStorageInfo, src.Storage.ScaleFactor);
            using var s8DstStorage = gd.CreateTextureStorage(s8DstStorageInfo, dst.Storage.ScaleFactor);

            void SlowBlit(TextureStorage srcTemp, TextureStorage dstTemp, ImageAspectFlags aspectFlags)
            {
                int levels = Math.Min(src.Info.Levels, dst.Info.Levels);

                int srcSize = 0;
                int dstSize = 0;

                for (int l = 0; l < levels; l++)
                {
                    srcSize += srcTemp.Info.GetMipSize2D(l);
                    dstSize += dstTemp.Info.GetMipSize2D(l);
                }

                using var srcTempBuffer = gd.BufferManager.Create(gd, srcSize, deviceLocal: true);
                using var dstTempBuffer = gd.BufferManager.Create(gd, dstSize, deviceLocal: true);

                src.Storage.CopyFromOrToBuffer(
                    cbs.CommandBuffer,
                    srcTempBuffer.GetBuffer().Get(cbs, 0, srcSize).Value,
                    src.GetImage().Get(cbs).Value,
                    srcSize,
                    to: true,
                    0,
                    0,
                    src.FirstLayer,
                    src.FirstLevel,
                    1,
                    levels,
                    true,
                    aspectFlags,
                    false);

                BufferHolder.InsertBufferBarrier(
                    gd,
                    cbs.CommandBuffer,
                    srcTempBuffer.GetBuffer().Get(cbs, 0, srcSize).Value,
                    AccessFlags.AccessTransferWriteBit,
                    AccessFlags.AccessTransferReadBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    0,
                    srcSize);

                srcTemp.CopyFromOrToBuffer(
                    cbs.CommandBuffer,
                    srcTempBuffer.GetBuffer().Get(cbs, 0, srcSize).Value,
                    srcTemp.GetImage().Get(cbs).Value,
                    srcSize,
                    to: false,
                    0,
                    0,
                    0,
                    0,
                    1,
                    levels,
                    true,
                    aspectFlags,
                    false);

                InsertImageBarrier(
                    gd.Api,
                    cbs.CommandBuffer,
                    srcTemp.GetImage().Get(cbs).Value,
                    AccessFlags.AccessTransferWriteBit,
                    AccessFlags.AccessTransferReadBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    aspectFlags,
                    0,
                    0,
                    1,
                    levels);

                TextureCopy.Blit(
                    gd.Api,
                    cbs.CommandBuffer,
                    srcTemp.GetImage().Get(cbs).Value,
                    dstTemp.GetImage().Get(cbs).Value,
                    srcTemp.Info,
                    dstTemp.Info,
                    srcRegion,
                    drOriginZero,
                    0,
                    0,
                    0,
                    0,
                    1,
                    levels,
                    false,
                    aspectFlags,
                    aspectFlags);

                InsertImageBarrier(
                    gd.Api,
                    cbs.CommandBuffer,
                    dstTemp.GetImage().Get(cbs).Value,
                    AccessFlags.AccessTransferWriteBit,
                    AccessFlags.AccessTransferReadBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    aspectFlags,
                    0,
                    0,
                    1,
                    levels);

                dstTemp.CopyFromOrToBuffer(
                    cbs.CommandBuffer,
                    dstTempBuffer.GetBuffer().Get(cbs, 0, dstSize).Value,
                    dstTemp.GetImage().Get(cbs).Value,
                    dstSize,
                    to: true,
                    0,
                    0,
                    0,
                    0,
                    1,
                    levels,
                    true,
                    aspectFlags,
                    false);

                BufferHolder.InsertBufferBarrier(
                    gd,
                    cbs.CommandBuffer,
                    dstTempBuffer.GetBuffer().Get(cbs, 0, dstSize).Value,
                    AccessFlags.AccessTransferWriteBit,
                    AccessFlags.AccessTransferReadBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    PipelineStageFlags.PipelineStageTransferBit,
                    0,
                    dstSize);

                dst.Storage.CopyFromOrToBuffer(
                    cbs.CommandBuffer,
                    dstTempBuffer.GetBuffer().Get(cbs, 0, dstSize).Value,
                    dst.GetImage().Get(cbs).Value,
                    dstSize,
                    to: false,
                    drBaseX,
                    drBaseY,
                    dst.FirstLayer,
                    dst.FirstLevel,
                    1,
                    levels,
                    true,
                    aspectFlags,
                    false);
            }

            SlowBlit(d32SrcStorage, d32DstStorage, ImageAspectFlags.ImageAspectDepthBit);
            SlowBlit(s8SrcStorage, s8DstStorage, ImageAspectFlags.ImageAspectStencilBit);
        }

        public static unsafe void InsertImageBarrier(
            Vk api,
            CommandBuffer commandBuffer,
            Image image,
            AccessFlags srcAccessMask,
            AccessFlags dstAccessMask,
            PipelineStageFlags srcStageMask,
            PipelineStageFlags dstStageMask,
            ImageAspectFlags aspectFlags,
            int firstLayer,
            int firstLevel,
            int layers,
            int levels)
        {
            ImageMemoryBarrier memoryBarrier = new ImageMemoryBarrier()
            {
                SType = StructureType.ImageMemoryBarrier,
                SrcAccessMask = srcAccessMask,
                DstAccessMask = dstAccessMask,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                OldLayout = ImageLayout.General,
                NewLayout = ImageLayout.General,
                SubresourceRange = new ImageSubresourceRange(aspectFlags, (uint)firstLevel, (uint)levels, (uint)firstLayer, (uint)layers)
            };

            api.CmdPipelineBarrier(
                commandBuffer,
                srcStageMask,
                dstStageMask,
                0,
                0,
                null,
                0,
                null,
                1,
                memoryBarrier);
        }

        private bool SupportsBlitFromD32FS8ToD32FAndS8()
        {
            var formatFeatureFlags = FormatFeatureFlags.FormatFeatureBlitSrcBit | FormatFeatureFlags.FormatFeatureBlitDstBit;
            return _gd.FormatCapabilities.OptimalFormatSupports(formatFeatureFlags, GAL.Format.D32Float)  &&
                   _gd.FormatCapabilities.OptimalFormatSupports(formatFeatureFlags, GAL.Format.S8Uint);
        }

        public TextureView GetView(GAL.Format format)
        {
            if (format == Info.Format)
            {
                return this;
            }

            if (_selfManagedViews != null && _selfManagedViews.TryGetValue(format, out var view))
            {
                return view;
            }

            view = CreateViewImpl(new TextureCreateInfo(
                Info.Width,
                Info.Height,
                Info.Depth,
                Info.Levels,
                Info.Samples,
                Info.BlockWidth,
                Info.BlockHeight,
                Info.BytesPerPixel,
                format,
                Info.DepthStencilMode,
                Info.Target,
                Info.SwizzleR,
                Info.SwizzleG,
                Info.SwizzleB,
                Info.SwizzleA), 0, 0);

            (_selfManagedViews ??= new Dictionary<GAL.Format, TextureView>()).Add(format, view);

            return view;
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            return CreateViewImpl(info, firstLayer, firstLevel);
        }

        private TextureView CreateViewImpl(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            return new TextureView(_gd, _device, info, Storage, FirstLayer + firstLayer, FirstLevel + firstLevel);
        }

        public byte[] GetData(int x, int y, int width, int height)
        {
            int size = width * height * Info.BytesPerPixel;
            using var bufferHolder = _gd.BufferManager.Create(_gd, size);

            using (var cbs = _gd.CommandBufferPool.Rent())
            {
                var buffer = bufferHolder.GetBuffer(cbs.CommandBuffer).Get(cbs).Value;
                var image = GetImage().Get(cbs).Value;

                CopyFromOrToBuffer(cbs.CommandBuffer, buffer, image, size, true, x, y, width, height);
            }

            bufferHolder.WaitForFences();
            byte[] bitmap = new byte[size];
            GetDataFromBuffer(bufferHolder.GetDataStorage(0, size), size, Span<byte>.Empty).CopyTo(bitmap);
            return bitmap;
        }

        public ReadOnlySpan<byte> GetData()
        {
            BackgroundResource resources = _gd.BackgroundResources.Get();

            if (_gd.CommandBufferPool.OwnedByCurrentThread)
            {
                _gd.FlushAllCommands();

                return GetData(_gd.CommandBufferPool, resources.GetFlushBuffer());
            }
            else
            {
                return GetData(resources.GetPool(), resources.GetFlushBuffer());
            }
        }

        public ReadOnlySpan<byte> GetData(int layer, int level)
        {
            BackgroundResource resources = _gd.BackgroundResources.Get();

            if (_gd.CommandBufferPool.OwnedByCurrentThread)
            {
                _gd.FlushAllCommands();

                return GetData(_gd.CommandBufferPool, resources.GetFlushBuffer(), layer, level);
            }
            else
            {
                return GetData(resources.GetPool(), resources.GetFlushBuffer(), layer, level);
            }
        }

        private ReadOnlySpan<byte> GetData(CommandBufferPool cbp, PersistentFlushBuffer flushBuffer)
        {
            int size = 0;

            for (int level = 0; level < Info.Levels; level++)
            {
                size += Info.GetMipSize(level);
            }

            size = GetBufferDataLength(size);

            Span<byte> result = flushBuffer.GetTextureData(cbp, this, size);
            return GetDataFromBuffer(result, size, result);
        }

        private ReadOnlySpan<byte> GetData(CommandBufferPool cbp, PersistentFlushBuffer flushBuffer, int layer, int level)
        {
            int size = GetBufferDataLength(Info.GetMipSize(level));

            Span<byte> result = flushBuffer.GetTextureData(cbp, this, size, layer, level);
            return GetDataFromBuffer(result, size, result);
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            SetData(data, 0, 0, Info.GetLayers(), Info.Levels, singleSlice: false);
        }

        public void SetData(ReadOnlySpan<byte> data, int layer, int level)
        {
            SetData(data, layer, level, 1, 1, singleSlice: true);
        }

        private void SetData(ReadOnlySpan<byte> data, int layer, int level, int layers, int levels, bool singleSlice)
        {
            int bufferDataLength = GetBufferDataLength(data.Length);

            using var bufferHolder = _gd.BufferManager.Create(_gd, bufferDataLength);

            Auto<DisposableImage> imageAuto = GetImage();

            // Load texture data inline if the texture has been used on the current command buffer.

            bool loadInline = Storage.HasCommandBufferDependency(_gd.PipelineInternal.CurrentCommandBuffer);

            var cbs = loadInline ? _gd.PipelineInternal.CurrentCommandBuffer : _gd.PipelineInternal.GetPreloadCommandBuffer();

            if (loadInline)
            {
                _gd.PipelineInternal.EndRenderPass();
            }

            CopyDataToBuffer(bufferHolder.GetDataStorage(0, bufferDataLength), data);

            var buffer = bufferHolder.GetBuffer(cbs.CommandBuffer).Get(cbs).Value;
            var image = imageAuto.Get(cbs).Value;

            CopyFromOrToBuffer(cbs.CommandBuffer, buffer, image, bufferDataLength, false, layer, level, layers, levels, singleSlice);
        }

        private int GetBufferDataLength(int length)
        {
            if (NeedsD24S8Conversion())
            {
                return length * 2;
            }

            return length;
        }

        private GAL.Format GetCompatibleGalFormat(GAL.Format format)
        {
            if (NeedsD24S8Conversion())
            {
                return GAL.Format.D32FloatS8Uint;
            }

            return format;
        }

        private void CopyDataToBuffer(Span<byte> storage, ReadOnlySpan<byte> input)
        {
            if (NeedsD24S8Conversion())
            {
                FormatConverter.ConvertD24S8ToD32FS8(storage, input);
                return;
            }

            input.CopyTo(storage);
        }

        private ReadOnlySpan<byte> GetDataFromBuffer(ReadOnlySpan<byte> storage, int size, Span<byte> output)
        {
            if (NeedsD24S8Conversion())
            {
                if (output.IsEmpty)
                {
                    output = new byte[GetBufferDataLength(size)];
                }

                FormatConverter.ConvertD32FS8ToD24S8(output, storage);
                return output;
            }

            return storage;
        }

        private bool NeedsD24S8Conversion()
        {
            return FormatCapabilities.IsD24S8(Info.Format) && VkFormat == VkFormat.D32SfloatS8Uint;
        }

        public void CopyFromOrToBuffer(
            CommandBuffer commandBuffer,
            VkBuffer buffer,
            Image image,
            int size,
            bool to,
            int dstLayer,
            int dstLevel,
            int dstLayers,
            int dstLevels,
            bool singleSlice)
        {
            bool is3D = Info.Target == Target.Texture3D;
            int width = Math.Max(1, Info.Width >> dstLevel);
            int height = Math.Max(1, Info.Height >> dstLevel);
            int depth = is3D && !singleSlice ? Math.Max(1, Info.Depth >> dstLevel) : 1;
            int layer = is3D ? 0 : dstLayer;
            int layers = dstLayers;
            int levels = dstLevels;

            int offset = 0;

            for (int level = 0; level < levels; level++)
            {
                int mipSize = GetBufferDataLength(Info.GetMipSize(dstLevel + level));

                int endOffset = offset + mipSize;

                if ((uint)endOffset > (uint)size)
                {
                    break;
                }

                int rowLength = (Info.GetMipStride(dstLevel + level) / Info.BytesPerPixel) * Info.BlockWidth;

                var aspectFlags = Info.Format.ConvertAspectFlags();

                if (aspectFlags == (ImageAspectFlags.ImageAspectDepthBit | ImageAspectFlags.ImageAspectStencilBit))
                {
                    aspectFlags = ImageAspectFlags.ImageAspectDepthBit;
                }

                var sl = new ImageSubresourceLayers(
                    aspectFlags,
                    (uint)(FirstLevel + dstLevel + level),
                    (uint)(FirstLayer + layer),
                    (uint)layers);

                var extent = new Extent3D((uint)width, (uint)height, (uint)depth);

                int z = is3D ? dstLayer : 0;

                var region = new BufferImageCopy(
                    (ulong)offset,
                    (uint)AlignUpNpot(rowLength, Info.BlockWidth),
                    (uint)AlignUpNpot(height, Info.BlockHeight),
                    sl,
                    new Offset3D(0, 0, z),
                    extent);

                if (to)
                {
                    _gd.Api.CmdCopyImageToBuffer(commandBuffer, image, ImageLayout.General, buffer, 1, region);
                }
                else
                {
                    _gd.Api.CmdCopyBufferToImage(commandBuffer, buffer, image, ImageLayout.General, 1, region);
                }

                offset += mipSize;

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);

                if (Info.Target == Target.Texture3D)
                {
                    depth = Math.Max(1, depth >> 1);
                }
            }
        }

        private void CopyFromOrToBuffer(
            CommandBuffer commandBuffer,
            VkBuffer buffer,
            Image image,
            int size,
            bool to,
            int x,
            int y,
            int width,
            int height)
        {
            var aspectFlags = Info.Format.ConvertAspectFlags();

            if (aspectFlags == (ImageAspectFlags.ImageAspectDepthBit | ImageAspectFlags.ImageAspectStencilBit))
            {
                aspectFlags = ImageAspectFlags.ImageAspectDepthBit;
            }

            var sl = new ImageSubresourceLayers(aspectFlags, (uint)FirstLevel, (uint)FirstLayer, 1);

            var extent = new Extent3D((uint)width, (uint)height, 1);

            var region = new BufferImageCopy(
                0,
                (uint)AlignUpNpot(width, Info.BlockWidth),
                (uint)AlignUpNpot(height, Info.BlockHeight),
                sl,
                new Offset3D(x, y, 0),
                extent);

            if (to)
            {
                _gd.Api.CmdCopyImageToBuffer(commandBuffer, image, ImageLayout.General, buffer, 1, region);
            }
            else
            {
                _gd.Api.CmdCopyBufferToImage(commandBuffer, buffer, image, ImageLayout.General, 1, region);
            }
        }

        private static int AlignUpNpot(int size, int alignment)
        {
            int remainder = size % alignment;
            if (remainder == 0)
            {
                return size;
            }

            return size + (alignment - remainder);
        }

        public void SetStorage(BufferRange buffer)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Valid = false;

                if (_gd.Textures.Remove(this))
                {
                    _imageView.Dispose();
                    _imageViewIdentity.Dispose();
                    _imageView2dArray?.Dispose();

                    Storage.DecrementViewsCount();
                }
            }
        }

        public void Dispose()
        {
            if (_selfManagedViews != null)
            {
                foreach (var view in _selfManagedViews.Values)
                {
                    view.Dispose();
                }

                _selfManagedViews = null;
            }

            Dispose(true);
        }

        public void Release()
        {
            Dispose();
        }
    }
}
