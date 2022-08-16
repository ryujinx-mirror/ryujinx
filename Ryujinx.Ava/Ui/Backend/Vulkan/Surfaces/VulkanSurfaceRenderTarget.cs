using System;
using Avalonia;
using Ryujinx.Graphics.Vulkan;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Ui.Vulkan.Surfaces
{
    internal class VulkanSurfaceRenderTarget : IDisposable
    {
        private readonly VulkanPlatformInterface _platformInterface;
        private readonly Format _format;

        private VulkanCommandBufferPool.VulkanCommandBuffer _commandBuffer;
        private VulkanImage Image { get; set; }
        private object _lock = new object();

        public uint MipLevels => Image.MipLevels;
        public VulkanDevice Device { get; }

        public VulkanSurfaceRenderTarget(VulkanPlatformInterface platformInterface, VulkanSurface surface)
        {
            _platformInterface = platformInterface;

            var device = VulkanInitialization.CreateDevice(platformInterface.Api,
                platformInterface.PhysicalDevice.InternalHandle,
                platformInterface.PhysicalDevice.QueueFamilyIndex,
                VulkanInitialization.GetSupportedExtensions(platformInterface.Api, platformInterface.PhysicalDevice.InternalHandle),
                platformInterface.PhysicalDevice.QueueCount);

            Device = new VulkanDevice(device, platformInterface.PhysicalDevice, platformInterface.Api);

            Display = VulkanDisplay.CreateDisplay(
                platformInterface.Instance,
                Device,
                platformInterface.PhysicalDevice,
                surface);
            Surface = surface;

            // Skia seems to only create surfaces from images with unorm format
            IsRgba = Display.SurfaceFormat.Format >= Format.R8G8B8A8Unorm &&
                     Display.SurfaceFormat.Format <= Format.R8G8B8A8Srgb;

            _format = IsRgba ? Format.R8G8B8A8Unorm : Format.B8G8R8A8Unorm;
        }

        public bool IsRgba { get; }

        public uint ImageFormat => (uint)_format;

        public ulong MemorySize => Image.MemorySize;

        public VulkanDisplay Display { get; private set; }

        public VulkanSurface Surface { get; private set; }

        public uint UsageFlags => Image.UsageFlags;

        public PixelSize Size { get; private set; }

        public void Dispose()
        {
            lock (_lock)
            {
                DestroyImage();
                Display?.Dispose();
                Surface?.Dispose();
                Device?.Dispose();

                Display = null;
                Surface = null;
            }
        }

        public VulkanSurfaceRenderingSession BeginDraw(float scaling)
        {
            if (Image == null)
            {
                RecreateImage();
            }

            _commandBuffer?.WaitForFence();
            _commandBuffer = null;

            var session = new VulkanSurfaceRenderingSession(Display, Device, this, scaling);

            Image.TransitionLayout(ImageLayout.ColorAttachmentOptimal, AccessFlags.AccessNoneKhr);

            return session;
        }

        public void RecreateImage()
        {
            DestroyImage();
            CreateImage();
        }

        private void CreateImage()
        {
            Size = Display.Size;

            Image = new VulkanImage(Device, _platformInterface.PhysicalDevice, Display.CommandBufferPool, ImageFormat, Size);
        }

        private void DestroyImage()
        {
            _commandBuffer?.WaitForFence();
            _commandBuffer = null;
            Image?.Dispose();
            Image = null;
        }

        public VulkanImage GetImage()
        {
            return Image;
        }

        public void EndDraw()
        {
            lock (_lock)
            {
                if (Display == null)
                {
                    return;
                }

                _commandBuffer = Display.StartPresentation();

                Display.BlitImageToCurrentImage(this, _commandBuffer.InternalHandle);

                Display.EndPresentation(_commandBuffer);
            }
        }
    }
}
