using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class ImageWindow : WindowBase, IWindow, IDisposable
    {
        private const int ImageCount = 5;
        private const int SurfaceWidth = 1280;
        private const int SurfaceHeight = 720;

        private readonly VulkanRenderer _gd;
        private readonly PhysicalDevice _physicalDevice;
        private readonly Device _device;

        private Auto<DisposableImage>[] _images;
        private Auto<DisposableImageView>[] _imageViews;
        private Auto<MemoryAllocation>[] _imageAllocationAuto;
        private ulong[] _imageSizes;
        private ulong[] _imageOffsets;

        private Semaphore _imageAvailableSemaphore;
        private Semaphore _renderFinishedSemaphore;

        private int _width = SurfaceWidth;
        private int _height = SurfaceHeight;
        private VkFormat _format;
        private bool _recreateImages;
        private int _nextImage;

        internal new bool ScreenCaptureRequested { get; set; }

        public unsafe ImageWindow(VulkanRenderer gd, PhysicalDevice physicalDevice, Device device)
        {
            _gd = gd;
            _physicalDevice = physicalDevice;
            _device = device;

            _format = VkFormat.R8G8B8A8Unorm;

            _images = new Auto<DisposableImage>[ImageCount];
            _imageAllocationAuto = new Auto<MemoryAllocation>[ImageCount];
            _imageSizes = new ulong[ImageCount];
            _imageOffsets = new ulong[ImageCount];

            CreateImages();

            var semaphoreCreateInfo = new SemaphoreCreateInfo()
            {
                SType = StructureType.SemaphoreCreateInfo
            };

            gd.Api.CreateSemaphore(device, semaphoreCreateInfo, null, out _imageAvailableSemaphore).ThrowOnError();
            gd.Api.CreateSemaphore(device, semaphoreCreateInfo, null, out _renderFinishedSemaphore).ThrowOnError();
        }

        private void RecreateImages()
        {
            for (int i = 0; i < ImageCount; i++)
            {
                _imageViews[i]?.Dispose();
                _imageAllocationAuto[i]?.Dispose();
                _images[i]?.Dispose();
            }

            CreateImages();
        }

        private unsafe void CreateImages()
        {
            _imageViews = new Auto<DisposableImageView>[ImageCount];

            var cbs = _gd.CommandBufferPool.Rent();
            for (int i = 0; i < _images.Length; i++)
            {
                var imageCreateInfo = new ImageCreateInfo
                {
                    SType = StructureType.ImageCreateInfo,
                    ImageType = ImageType.ImageType2D,
                    Format = _format,
                    Extent =
                        new Extent3D((uint?)_width,
                            (uint?)_height, 1),
                    MipLevels = 1,
                    ArrayLayers = 1,
                    Samples = SampleCountFlags.SampleCount1Bit,
                    Tiling = ImageTiling.Optimal,
                    Usage = ImageUsageFlags.ImageUsageColorAttachmentBit | ImageUsageFlags.ImageUsageTransferSrcBit | ImageUsageFlags.ImageUsageTransferDstBit,
                    SharingMode = SharingMode.Exclusive,
                    InitialLayout = ImageLayout.Undefined,
                    Flags = ImageCreateFlags.ImageCreateMutableFormatBit
                };

                _gd.Api.CreateImage(_device, imageCreateInfo, null, out var image).ThrowOnError();
                _images[i] = new Auto<DisposableImage>(new DisposableImage(_gd.Api, _device, image));

                _gd.Api.GetImageMemoryRequirements(_device, image,
                    out var memoryRequirements);

                var allocation = _gd.MemoryAllocator.AllocateDeviceMemory(_physicalDevice, memoryRequirements, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit);

                _imageSizes[i] = allocation.Size;
                _imageOffsets[i] = allocation.Offset;

                _imageAllocationAuto[i] = new Auto<MemoryAllocation>(allocation);

                _gd.Api.BindImageMemory(_device, image, allocation.Memory, allocation.Offset);

                _imageViews[i] = CreateImageView(image, _format);

                Transition(
                    cbs.CommandBuffer,
                    image,
                    0,
                    0,
                    ImageLayout.Undefined,
                    ImageLayout.ColorAttachmentOptimal);
            }

            _gd.CommandBufferPool.Return(cbs);
        }

        private unsafe Auto<DisposableImageView> CreateImageView(Image image, VkFormat format)
        {
            var componentMapping = new ComponentMapping(
                ComponentSwizzle.R,
                ComponentSwizzle.G,
                ComponentSwizzle.B,
                ComponentSwizzle.A);

            var subresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1);

            var imageCreateInfo = new ImageViewCreateInfo()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.ImageViewType2D,
                Format = format,
                Components = componentMapping,
                SubresourceRange = subresourceRange
            };

            _gd.Api.CreateImageView(_device, imageCreateInfo, null, out var imageView).ThrowOnError();
            return new Auto<DisposableImageView>(new DisposableImageView(_gd.Api, _device, imageView));
        }

        public override unsafe void Present(ITexture texture, ImageCrop crop, Action<object> swapBuffersCallback)
        {
            if (_recreateImages)
            {
                RecreateImages();
                _recreateImages = false;
            }

            var image = _images[_nextImage];

            _gd.FlushAllCommands();

            var cbs = _gd.CommandBufferPool.Rent();

            Transition(
                cbs.CommandBuffer,
                image.GetUnsafe().Value,
                0,
                AccessFlags.AccessTransferWriteBit,
                ImageLayout.ColorAttachmentOptimal,
                ImageLayout.General);

            var view = (TextureView)texture;

            int srcX0, srcX1, srcY0, srcY1;
            float scale = view.ScaleFactor;

            if (crop.Left == 0 && crop.Right == 0)
            {
                srcX0 = 0;
                srcX1 = (int)(view.Width / scale);
            }
            else
            {
                srcX0 = crop.Left;
                srcX1 = crop.Right;
            }

            if (crop.Top == 0 && crop.Bottom == 0)
            {
                srcY0 = 0;
                srcY1 = (int)(view.Height / scale);
            }
            else
            {
                srcY0 = crop.Top;
                srcY1 = crop.Bottom;
            }

            if (scale != 1f)
            {
                srcX0 = (int)(srcX0 * scale);
                srcY0 = (int)(srcY0 * scale);
                srcX1 = (int)Math.Ceiling(srcX1 * scale);
                srcY1 = (int)Math.Ceiling(srcY1 * scale);
            }

            if (ScreenCaptureRequested)
            {
                CaptureFrame(view, srcX0, srcY0, srcX1 - srcX0, srcY1 - srcY0, view.Info.Format.IsBgr(), crop.FlipX, crop.FlipY);

                ScreenCaptureRequested = false;
            }

            float ratioX = crop.IsStretched ? 1.0f : MathF.Min(1.0f, _height * crop.AspectRatioX / (_width * crop.AspectRatioY));
            float ratioY = crop.IsStretched ? 1.0f : MathF.Min(1.0f, _width * crop.AspectRatioY / (_height * crop.AspectRatioX));

            int dstWidth = (int)(_width * ratioX);
            int dstHeight = (int)(_height * ratioY);

            int dstPaddingX = (_width - dstWidth) / 2;
            int dstPaddingY = (_height - dstHeight) / 2;

            int dstX0 = crop.FlipX ? _width - dstPaddingX : dstPaddingX;
            int dstX1 = crop.FlipX ? dstPaddingX : _width - dstPaddingX;

            int dstY0 = crop.FlipY ? dstPaddingY : _height - dstPaddingY;
            int dstY1 = crop.FlipY ? _height - dstPaddingY : dstPaddingY;

            _gd.HelperShader.Blit(
                _gd,
                cbs,
                view,
                _imageViews[_nextImage],
                _width,
                _height,
                _format,
                new Extents2D(srcX0, srcY0, srcX1, srcY1),
                new Extents2D(dstX0, dstY1, dstX1, dstY0),
                true,
                true);

            Transition(
                cbs.CommandBuffer,
                image.GetUnsafe().Value,
                0,
                0,
                ImageLayout.General,
                ImageLayout.ColorAttachmentOptimal);

            _gd.CommandBufferPool.Return(
                cbs,
                null,
                stackalloc[] { PipelineStageFlags.PipelineStageColorAttachmentOutputBit },
                null);

            var memory = _imageAllocationAuto[_nextImage].GetUnsafe().Memory;
            var presentInfo = new PresentImageInfo(image.GetUnsafe().Value, memory, _imageSizes[_nextImage], _imageOffsets[_nextImage], _renderFinishedSemaphore, _imageAvailableSemaphore);

            swapBuffersCallback(presentInfo);

            _nextImage %= ImageCount;
        }

        private unsafe void Transition(
            CommandBuffer commandBuffer,
            Image image,
            AccessFlags srcAccess,
            AccessFlags dstAccess,
            ImageLayout srcLayout,
            ImageLayout dstLayout)
        {
            var subresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1);

            var barrier = new ImageMemoryBarrier()
            {
                SType = StructureType.ImageMemoryBarrier,
                SrcAccessMask = srcAccess,
                DstAccessMask = dstAccess,
                OldLayout = srcLayout,
                NewLayout = dstLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange = subresourceRange
            };

            _gd.Api.CmdPipelineBarrier(
                commandBuffer,
                PipelineStageFlags.PipelineStageTopOfPipeBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                0,
                0,
                null,
                0,
                null,
                1,
                barrier);
        }

        private void CaptureFrame(TextureView texture, int x, int y, int width, int height, bool isBgra, bool flipX, bool flipY)
        {
            byte[] bitmap = texture.GetData(x, y, width, height);

            _gd.OnScreenCaptured(new ScreenCaptureImageInfo(width, height, isBgra, bitmap, flipX, flipY));
        }

        public override void SetSize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                _recreateImages = true;
            }

            _width = width;
            _height = height;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                unsafe
                {
                    _gd.Api.DestroySemaphore(_device, _renderFinishedSemaphore, null);
                    _gd.Api.DestroySemaphore(_device, _imageAvailableSemaphore, null);

                    for (int i = 0; i < ImageCount; i++)
                    {
                        _imageViews[i]?.Dispose();
                        _imageAllocationAuto[i]?.Dispose();
                        _images[i]?.Dispose();
                    }
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }

    public class PresentImageInfo
    {
        public Image Image { get; }
        public DeviceMemory Memory { get; }
        public ulong MemorySize { get; set; }
        public ulong MemoryOffset { get; set; }
        public Semaphore ReadySemaphore { get; }
        public Semaphore AvailableSemaphore { get; }

        public PresentImageInfo(Image image, DeviceMemory memory, ulong memorySize, ulong memoryOffset, Semaphore readySemaphore, Semaphore availableSemaphore)
        {
            this.Image = image;
            this.Memory = memory;
            this.MemorySize = memorySize;
            this.MemoryOffset = memoryOffset;
            this.ReadySemaphore = readySemaphore;
            this.AvailableSemaphore = availableSemaphore;
        }
    }
}