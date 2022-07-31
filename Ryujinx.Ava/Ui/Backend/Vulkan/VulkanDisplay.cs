using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Ryujinx.Ava.Ui.Vulkan.Surfaces;
using Ryujinx.Ui.Common.Configuration;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Ryujinx.Ava.Ui.Vulkan
{
    internal class VulkanDisplay : IDisposable
    {
        private static KhrSwapchain _swapchainExtension;
        private readonly VulkanInstance _instance;
        private readonly VulkanPhysicalDevice _physicalDevice;
        private readonly VulkanSemaphorePair _semaphorePair;
        private uint _nextImage;
        private readonly VulkanSurface _surface;
        private SurfaceFormatKHR _surfaceFormat;
        private SwapchainKHR _swapchain;
        private Extent2D _swapchainExtent;
        private Image[] _swapchainImages;
        private VulkanDevice _device { get; }
        private ImageView[] _swapchainImageViews = new ImageView[0];
        private bool _vsyncStateChanged;
        private bool _vsyncEnabled;

        public VulkanCommandBufferPool CommandBufferPool { get; set; }

        public object Lock => _device.Lock;

        private VulkanDisplay(VulkanInstance instance, VulkanDevice device,
            VulkanPhysicalDevice physicalDevice, VulkanSurface surface, SwapchainKHR swapchain,
            Extent2D swapchainExtent)
        {
            _instance = instance;
            _device = device;
            _physicalDevice = physicalDevice;
            _swapchain = swapchain;
            _swapchainExtent = swapchainExtent;
            _surface = surface;

            CreateSwapchainImages();

            _semaphorePair = new VulkanSemaphorePair(_device);

            CommandBufferPool = new VulkanCommandBufferPool(device, physicalDevice);
        }

        public PixelSize Size { get; private set; }
        public uint QueueFamilyIndex => _physicalDevice.QueueFamilyIndex;

        internal SurfaceFormatKHR SurfaceFormat
        {
            get
            {
                if (_surfaceFormat.Format == Format.Undefined)
                {
                    _surfaceFormat = _surface.GetSurfaceFormat(_physicalDevice);
                }

                return _surfaceFormat;
            }
        }

        public void Dispose()
        {
            _device.WaitIdle();
            _semaphorePair?.Dispose();
            DestroyCurrentImageViews();
            _swapchainExtension.DestroySwapchain(_device.InternalHandle, _swapchain, Span<AllocationCallbacks>.Empty);
            CommandBufferPool.Dispose();
        }

        private static unsafe SwapchainKHR CreateSwapchain(VulkanInstance instance, VulkanDevice device,
            VulkanPhysicalDevice physicalDevice, VulkanSurface surface, out Extent2D swapchainExtent,
            SwapchainKHR? oldswapchain = null, bool vsyncEnabled = true)
        {
            if (_swapchainExtension == null)
            {
                instance.Api.TryGetDeviceExtension(instance.InternalHandle, device.InternalHandle, out _swapchainExtension);
            }

            while (!surface.CanSurfacePresent(physicalDevice))
            {
                Thread.Sleep(16);
            }

            VulkanSurface.SurfaceExtension.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.InternalHandle,
                surface.ApiHandle, out var capabilities);

            var imageCount = capabilities.MinImageCount + 1;
            if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
            {
                imageCount = capabilities.MaxImageCount;
            }

            var surfaceFormat = surface.GetSurfaceFormat(physicalDevice);

            bool supportsIdentityTransform = capabilities.SupportedTransforms.HasFlag(SurfaceTransformFlagsKHR.SurfaceTransformIdentityBitKhr);
            bool isRotated = capabilities.CurrentTransform.HasFlag(SurfaceTransformFlagsKHR.SurfaceTransformRotate90BitKhr) ||
                capabilities.CurrentTransform.HasFlag(SurfaceTransformFlagsKHR.SurfaceTransformRotate270BitKhr);

            swapchainExtent = GetSwapchainExtent(surface, capabilities);

            CompositeAlphaFlagsKHR compositeAlphaFlags = GetSuitableCompositeAlphaFlags(capabilities);

            PresentModeKHR presentMode = GetSuitablePresentMode(physicalDevice, surface, vsyncEnabled);

            var swapchainCreateInfo = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = surface.ApiHandle,
                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = swapchainExtent,
                ImageUsage =
                    ImageUsageFlags.ImageUsageColorAttachmentBit | ImageUsageFlags.ImageUsageTransferDstBit,
                ImageSharingMode = SharingMode.Exclusive,
                ImageArrayLayers = 1,
                PreTransform = supportsIdentityTransform && isRotated ?
                    SurfaceTransformFlagsKHR.SurfaceTransformIdentityBitKhr :
                    capabilities.CurrentTransform,
                CompositeAlpha = compositeAlphaFlags,
                PresentMode = presentMode,
                Clipped = true,
                OldSwapchain = oldswapchain ?? new SwapchainKHR()
            };

            _swapchainExtension.CreateSwapchain(device.InternalHandle, swapchainCreateInfo, null, out var swapchain)
                .ThrowOnError();

            if (oldswapchain != null)
            {
                _swapchainExtension.DestroySwapchain(device.InternalHandle, oldswapchain.Value, null);
            }

            return swapchain;
        }

        private static unsafe Extent2D GetSwapchainExtent(VulkanSurface surface, SurfaceCapabilitiesKHR capabilities)
        {
            Extent2D swapchainExtent;
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                swapchainExtent = capabilities.CurrentExtent;
            }
            else
            {
                var surfaceSize = surface.SurfaceSize;

                var width = Math.Clamp((uint)surfaceSize.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                var height = Math.Clamp((uint)surfaceSize.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

                swapchainExtent = new Extent2D(width, height);
            }

            return swapchainExtent;
        }

        private static unsafe CompositeAlphaFlagsKHR GetSuitableCompositeAlphaFlags(SurfaceCapabilitiesKHR capabilities)
        {
            var compositeAlphaFlags = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr;

            if (capabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKHR.CompositeAlphaPostMultipliedBitKhr))
            {
                compositeAlphaFlags = CompositeAlphaFlagsKHR.CompositeAlphaPostMultipliedBitKhr;
            }
            else if (capabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKHR.CompositeAlphaPreMultipliedBitKhr))
            {
                compositeAlphaFlags = CompositeAlphaFlagsKHR.CompositeAlphaPreMultipliedBitKhr;
            }

            return compositeAlphaFlags;
        }

        private static unsafe PresentModeKHR GetSuitablePresentMode(VulkanPhysicalDevice physicalDevice, VulkanSurface surface, bool vsyncEnabled)
        {
            uint presentModesCount;

            VulkanSurface.SurfaceExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.InternalHandle,
                surface.ApiHandle,
                &presentModesCount, null);

            var presentModes = new PresentModeKHR[presentModesCount];

            fixed (PresentModeKHR* pPresentModes = presentModes)
            {
                VulkanSurface.SurfaceExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.InternalHandle,
                    surface.ApiHandle, &presentModesCount, pPresentModes);
            }

            var modes = presentModes.ToList();
            var presentMode = PresentModeKHR.PresentModeFifoKhr;

            if (!vsyncEnabled && modes.Contains(PresentModeKHR.PresentModeImmediateKhr))
            {
                presentMode = PresentModeKHR.PresentModeImmediateKhr;
            }
            else if (modes.Contains(PresentModeKHR.PresentModeMailboxKhr))
            {
                presentMode = PresentModeKHR.PresentModeMailboxKhr;
            }
            else if (modes.Contains(PresentModeKHR.PresentModeImmediateKhr))
            {
                presentMode = PresentModeKHR.PresentModeImmediateKhr;
            }

            return presentMode;
        }

        internal static VulkanDisplay CreateDisplay(VulkanInstance instance, VulkanDevice device,
            VulkanPhysicalDevice physicalDevice, VulkanSurface surface)
        {
            var swapchain = CreateSwapchain(instance, device, physicalDevice, surface, out var extent, null, true);

            return new VulkanDisplay(instance, device, physicalDevice, surface, swapchain, extent);
        }

        private unsafe void CreateSwapchainImages()
        {
            DestroyCurrentImageViews();

            Size = new PixelSize((int)_swapchainExtent.Width, (int)_swapchainExtent.Height);

            uint imageCount = 0;

            _swapchainExtension.GetSwapchainImages(_device.InternalHandle, _swapchain, &imageCount, null);

            _swapchainImages = new Image[imageCount];

            fixed (Image* pSwapchainImages = _swapchainImages)
            {
                _swapchainExtension.GetSwapchainImages(_device.InternalHandle, _swapchain, &imageCount, pSwapchainImages);
            }

            _swapchainImageViews = new ImageView[imageCount];

            var surfaceFormat = SurfaceFormat;

            for (var i = 0; i < imageCount; i++)
            {
                _swapchainImageViews[i] = CreateSwapchainImageView(_swapchainImages[i], surfaceFormat.Format);
            }
        }

        private void DestroyCurrentImageViews()
        {
            for (var i = 0; i < _swapchainImageViews.Length; i++)
            {
                _instance.Api.DestroyImageView(_device.InternalHandle, _swapchainImageViews[i], Span<AllocationCallbacks>.Empty);
            }
        }

        internal void ChangeVSyncMode(bool vsyncEnabled)
        {
            _vsyncStateChanged = true;
            _vsyncEnabled = vsyncEnabled;
        }

        private void Recreate()
        {
            _device.WaitIdle();
            _swapchain = CreateSwapchain(_instance, _device, _physicalDevice, _surface, out _swapchainExtent, _swapchain, _vsyncEnabled);

            CreateSwapchainImages();
        }

        private unsafe ImageView CreateSwapchainImageView(Image swapchainImage, Format format)
        {
            var componentMapping = new ComponentMapping(
                ComponentSwizzle.Identity,
                ComponentSwizzle.Identity,
                ComponentSwizzle.Identity,
                ComponentSwizzle.Identity);

            var subresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1);

            var imageCreateInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = swapchainImage,
                ViewType = ImageViewType.ImageViewType2D,
                Format = format,
                Components = componentMapping,
                SubresourceRange = subresourceRange
            };

            _instance.Api.CreateImageView(_device.InternalHandle, imageCreateInfo, null, out var imageView).ThrowOnError();
            return imageView;
        }

        public bool EnsureSwapchainAvailable()
        {
            if (Size != _surface.SurfaceSize || _vsyncStateChanged)
            {
                _vsyncStateChanged = false;

                Recreate();

                return false;
            }

            return true;
        }

        internal VulkanCommandBufferPool.VulkanCommandBuffer StartPresentation(VulkanSurfaceRenderTarget renderTarget)
        {
            _nextImage = 0;
            while (true)
            {
                var acquireResult = _swapchainExtension.AcquireNextImage(
                    _device.InternalHandle,
                    _swapchain,
                    ulong.MaxValue,
                    _semaphorePair.ImageAvailableSemaphore,
                    new Fence(),
                    ref _nextImage);

                if (acquireResult == Result.ErrorOutOfDateKhr ||
                    acquireResult == Result.SuboptimalKhr)
                {
                    Recreate();
                }
                else
                {
                    acquireResult.ThrowOnError();
                    break;
                }
            }

            var commandBuffer = CommandBufferPool.CreateCommandBuffer();
            commandBuffer.BeginRecording();

            VulkanMemoryHelper.TransitionLayout(_device, commandBuffer.InternalHandle,
                _swapchainImages[_nextImage], ImageLayout.Undefined,
                AccessFlags.AccessNoneKhr,
                ImageLayout.TransferDstOptimal,
                AccessFlags.AccessTransferWriteBit,
                1);

            return commandBuffer;
        }

        internal void BlitImageToCurrentImage(VulkanSurfaceRenderTarget renderTarget, CommandBuffer commandBuffer)
        {
            VulkanMemoryHelper.TransitionLayout(_device, commandBuffer,
                renderTarget.Image.InternalHandle.Value, (ImageLayout)renderTarget.Image.CurrentLayout,
                AccessFlags.AccessNoneKhr,
                ImageLayout.TransferSrcOptimal,
                AccessFlags.AccessTransferReadBit,
                renderTarget.MipLevels);

            var srcBlitRegion = new ImageBlit
            {
                SrcOffsets = new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(renderTarget.Size.Width, renderTarget.Size.Height, 1),
                },
                DstOffsets = new ImageBlit.DstOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(Size.Width, Size.Height, 1),
                },
                SrcSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    MipLevel = 0
                },
                DstSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    MipLevel = 0
                }
            };

            _device.Api.CmdBlitImage(commandBuffer, renderTarget.Image.InternalHandle.Value,
                ImageLayout.TransferSrcOptimal,
                _swapchainImages[_nextImage],
                ImageLayout.TransferDstOptimal,
                1,
                srcBlitRegion,
                Filter.Linear);

            VulkanMemoryHelper.TransitionLayout(_device, commandBuffer,
                renderTarget.Image.InternalHandle.Value, ImageLayout.TransferSrcOptimal,
                AccessFlags.AccessTransferReadBit,
                (ImageLayout)renderTarget.Image.CurrentLayout,
                AccessFlags.AccessNoneKhr,
                renderTarget.MipLevels);
        }

        internal unsafe void EndPresentation(VulkanCommandBufferPool.VulkanCommandBuffer commandBuffer)
        {
            VulkanMemoryHelper.TransitionLayout(_device, commandBuffer.InternalHandle,
                _swapchainImages[_nextImage], ImageLayout.TransferDstOptimal,
                AccessFlags.AccessNoneKhr,
                ImageLayout.PresentSrcKhr,
                AccessFlags.AccessNoneKhr,
                1);

            commandBuffer.Submit(
                stackalloc[] { _semaphorePair.ImageAvailableSemaphore },
                stackalloc[] { PipelineStageFlags.PipelineStageColorAttachmentOutputBit },
                stackalloc[] { _semaphorePair.RenderFinishedSemaphore });

            var semaphore = _semaphorePair.RenderFinishedSemaphore;
            var swapchain = _swapchain;
            var nextImage = _nextImage;

            Result result;

            var presentInfo = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &semaphore,
                SwapchainCount = 1,
                PSwapchains = &swapchain,
                PImageIndices = &nextImage,
                PResults = &result
            };

            lock (_device.Lock)
            {
                _swapchainExtension.QueuePresent(_device.PresentQueue.InternalHandle, presentInfo);
            }

            CommandBufferPool.FreeUsedCommandBuffers();
        }
    }
}
