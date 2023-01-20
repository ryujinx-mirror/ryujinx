using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Linq;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class Window : WindowBase, IDisposable
    {
        private const int SurfaceWidth = 1280;
        private const int SurfaceHeight = 720;

        private readonly VulkanRenderer _gd;
        private readonly SurfaceKHR _surface;
        private readonly PhysicalDevice _physicalDevice;
        private readonly Device _device;
        private SwapchainKHR _swapchain;

        private Image[] _swapchainImages;
        private Auto<DisposableImageView>[] _swapchainImageViews;

        private Semaphore _imageAvailableSemaphore;
        private Semaphore _renderFinishedSemaphore;

        private int _width;
        private int _height;
        private bool _vsyncEnabled;
        private bool _vsyncModeChanged;
        private VkFormat _format;

        public unsafe Window(VulkanRenderer gd, SurfaceKHR surface, PhysicalDevice physicalDevice, Device device)
        {
            _gd = gd;
            _physicalDevice = physicalDevice;
            _device = device;
            _surface = surface;

            CreateSwapchain();

            var semaphoreCreateInfo = new SemaphoreCreateInfo()
            {
                SType = StructureType.SemaphoreCreateInfo
            };

            gd.Api.CreateSemaphore(device, semaphoreCreateInfo, null, out _imageAvailableSemaphore).ThrowOnError();
            gd.Api.CreateSemaphore(device, semaphoreCreateInfo, null, out _renderFinishedSemaphore).ThrowOnError();
        }

        private void RecreateSwapchain()
        {
            var oldSwapchain = _swapchain;
            int imageCount = _swapchainImageViews.Length;
            _vsyncModeChanged = false;

            for (int i = 0; i < imageCount; i++)
            {
                _swapchainImageViews[i].Dispose();
            }

            // Destroy old Swapchain.
            _gd.Api.DeviceWaitIdle(_device);
            _gd.SwapchainApi.DestroySwapchain(_device, oldSwapchain, Span<AllocationCallbacks>.Empty);

            CreateSwapchain();
        }

        private unsafe void CreateSwapchain()
        {
            _gd.SurfaceApi.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, out var capabilities);

            uint surfaceFormatsCount;

            _gd.SurfaceApi.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &surfaceFormatsCount, null);

            var surfaceFormats = new SurfaceFormatKHR[surfaceFormatsCount];

            fixed (SurfaceFormatKHR* pSurfaceFormats = surfaceFormats)
            {
                _gd.SurfaceApi.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &surfaceFormatsCount, pSurfaceFormats);
            }

            uint presentModesCount;

            _gd.SurfaceApi.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModesCount, null);

            var presentModes = new PresentModeKHR[presentModesCount];

            fixed (PresentModeKHR* pPresentModes = presentModes)
            {
                _gd.SurfaceApi.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModesCount, pPresentModes);
            }

            uint imageCount = capabilities.MinImageCount + 1;
            if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
            {
                imageCount = capabilities.MaxImageCount;
            }

            var surfaceFormat = ChooseSwapSurfaceFormat(surfaceFormats);

            var extent = ChooseSwapExtent(capabilities);

            _width = (int)extent.Width;
            _height = (int)extent.Height;
            _format = surfaceFormat.Format;

            var oldSwapchain = _swapchain;

            var swapchainCreateInfo = new SwapchainCreateInfoKHR()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _surface,
                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferDstBit,
                ImageSharingMode = SharingMode.Exclusive,
                ImageArrayLayers = 1,
                PreTransform = capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = ChooseSwapPresentMode(presentModes, _vsyncEnabled),
                Clipped = true
            };

            _gd.SwapchainApi.CreateSwapchain(_device, swapchainCreateInfo, null, out _swapchain).ThrowOnError();

            _gd.SwapchainApi.GetSwapchainImages(_device, _swapchain, &imageCount, null);

            _swapchainImages = new Image[imageCount];

            fixed (Image* pSwapchainImages = _swapchainImages)
            {
                _gd.SwapchainApi.GetSwapchainImages(_device, _swapchain, &imageCount, pSwapchainImages);
            }

            _swapchainImageViews = new Auto<DisposableImageView>[imageCount];

            for (int i = 0; i < imageCount; i++)
            {
                _swapchainImageViews[i] = CreateSwapchainImageView(_swapchainImages[i], surfaceFormat.Format);
            }
        }

        private unsafe Auto<DisposableImageView> CreateSwapchainImageView(Image swapchainImage, VkFormat format)
        {
            var componentMapping = new ComponentMapping(
                ComponentSwizzle.R,
                ComponentSwizzle.G,
                ComponentSwizzle.B,
                ComponentSwizzle.A);

            var aspectFlags = ImageAspectFlags.ColorBit;

            var subresourceRange = new ImageSubresourceRange(aspectFlags, 0, 1, 0, 1);

            var imageCreateInfo = new ImageViewCreateInfo()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = swapchainImage,
                ViewType = ImageViewType.Type2D,
                Format = format,
                Components = componentMapping,
                SubresourceRange = subresourceRange
            };

            _gd.Api.CreateImageView(_device, imageCreateInfo, null, out var imageView).ThrowOnError();
            return new Auto<DisposableImageView>(new DisposableImageView(_gd.Api, _device, imageView));
        }

        private static SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] availableFormats)
        {
            if (availableFormats.Length == 1 && availableFormats[0].Format == VkFormat.Undefined)
            {
                return new SurfaceFormatKHR(VkFormat.B8G8R8A8Unorm, ColorSpaceKHR.PaceSrgbNonlinearKhr);
            }

            foreach (var format in availableFormats)
            {
                if (format.Format == VkFormat.B8G8R8A8Unorm && format.ColorSpace == ColorSpaceKHR.PaceSrgbNonlinearKhr)
                {
                    return format;
                }
            }

            return availableFormats[0];
        }

        private static PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] availablePresentModes, bool vsyncEnabled)
        {
            if (!vsyncEnabled && availablePresentModes.Contains(PresentModeKHR.ImmediateKhr))
            {
                return PresentModeKHR.ImmediateKhr;
            }
            else if (availablePresentModes.Contains(PresentModeKHR.MailboxKhr))
            {
                return PresentModeKHR.MailboxKhr;
            }
            else if (availablePresentModes.Contains(PresentModeKHR.FifoKhr))
            {
               return PresentModeKHR.FifoKhr;
            }
            else
            {
                return PresentModeKHR.FifoKhr;
            }
        }

        public static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                uint width = Math.Max(capabilities.MinImageExtent.Width, Math.Min(capabilities.MaxImageExtent.Width, SurfaceWidth));
                uint height = Math.Max(capabilities.MinImageExtent.Height, Math.Min(capabilities.MaxImageExtent.Height, SurfaceHeight));

                return new Extent2D(width, height);
            }
        }

        public unsafe override void Present(ITexture texture, ImageCrop crop, Action swapBuffersCallback)
        {
            uint nextImage = 0;

            while (true)
            {
                var acquireResult = _gd.SwapchainApi.AcquireNextImage(
                    _device,
                    _swapchain,
                    ulong.MaxValue,
                    _imageAvailableSemaphore,
                    new Fence(),
                    ref nextImage);

                if (acquireResult == Result.ErrorOutOfDateKhr ||
                    acquireResult == Result.SuboptimalKhr ||
                    _vsyncModeChanged)
                {
                    RecreateSwapchain();
                }
                else
                {
                    acquireResult.ThrowOnError();
                    break;
                }
            }

            var swapchainImage = _swapchainImages[nextImage];

            _gd.FlushAllCommands();

            var cbs = _gd.CommandBufferPool.Rent();

            Transition(
                cbs.CommandBuffer,
                swapchainImage,
                0,
                AccessFlags.TransferWriteBit,
                ImageLayout.Undefined,
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

            int dstWidth  = (int)(_width  * ratioX);
            int dstHeight = (int)(_height * ratioY);

            int dstPaddingX = (_width  - dstWidth)  / 2;
            int dstPaddingY = (_height - dstHeight) / 2;

            int dstX0 = crop.FlipX ? _width - dstPaddingX : dstPaddingX;
            int dstX1 = crop.FlipX ? dstPaddingX : _width - dstPaddingX;

            int dstY0 = crop.FlipY ? dstPaddingY : _height - dstPaddingY;
            int dstY1 = crop.FlipY ? _height - dstPaddingY : dstPaddingY;

            _gd.HelperShader.Blit(
                _gd,
                cbs,
                view,
                _swapchainImageViews[nextImage],
                _width,
                _height,
                _format,
                new Extents2D(srcX0, srcY0, srcX1, srcY1),
                new Extents2D(dstX0, dstY1, dstX1, dstY0),
                true,
                true);

            Transition(
                cbs.CommandBuffer,
                swapchainImage,
                0,
                0,
                ImageLayout.General,
                ImageLayout.PresentSrcKhr);

            _gd.CommandBufferPool.Return(
                cbs,
                stackalloc[] { _imageAvailableSemaphore },
                stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit },
                stackalloc[] { _renderFinishedSemaphore });

            // TODO: Present queue.
            var semaphore = _renderFinishedSemaphore;
            var swapchain = _swapchain;

            Result result;

            var presentInfo = new PresentInfoKHR()
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &semaphore,
                SwapchainCount = 1,
                PSwapchains = &swapchain,
                PImageIndices = &nextImage,
                PResults = &result
            };

            lock (_gd.QueueLock)
            {
                _gd.SwapchainApi.QueuePresent(_gd.Queue, presentInfo);
            }
        }

        private unsafe void Transition(
            CommandBuffer commandBuffer,
            Image image,
            AccessFlags srcAccess,
            AccessFlags dstAccess,
            ImageLayout srcLayout,
            ImageLayout dstLayout)
        {
            var subresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1);

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
                PipelineStageFlags.TopOfPipeBit,
                PipelineStageFlags.AllCommandsBit,
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
            // Not needed as we can get the size from the surface.
        }

        public override void ChangeVSyncMode(bool vsyncEnabled)
        {
            _vsyncEnabled = vsyncEnabled;
            _vsyncModeChanged = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                unsafe
                {
                    _gd.Api.DestroySemaphore(_device, _renderFinishedSemaphore, null);
                    _gd.Api.DestroySemaphore(_device, _imageAvailableSemaphore, null);

                    for (int i = 0; i < _swapchainImageViews.Length; i++)
                    {
                        _swapchainImageViews[i].Dispose();
                    }

                    _gd.SwapchainApi.DestroySwapchain(_device, _swapchain, null);

                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
