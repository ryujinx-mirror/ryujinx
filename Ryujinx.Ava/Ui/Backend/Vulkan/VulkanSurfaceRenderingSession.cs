using System;
using Avalonia;
using Ryujinx.Ava.Ui.Vulkan.Surfaces;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Ui.Vulkan
{
    internal class VulkanSurfaceRenderingSession : IDisposable
    {
        private readonly VulkanDevice _device;
        private readonly VulkanSurfaceRenderTarget _renderTarget;
        private VulkanCommandBufferPool.VulkanCommandBuffer _commandBuffer;

        public VulkanSurfaceRenderingSession(VulkanDisplay display, VulkanDevice device,
            VulkanSurfaceRenderTarget renderTarget, float scaling)
        {
            Display = display;
            _device = device;
            _renderTarget = renderTarget;
            Scaling = scaling;
            Begin();
        }

        public VulkanDisplay Display { get; }

        public PixelSize Size => _renderTarget.Size;
        public Vk Api => _device.Api;

        public float Scaling { get; }

        private void Begin()
        {
            if (!Display.EnsureSwapchainAvailable())
            {
                _renderTarget.Invalidate();
            }
        }

        public void Dispose()
        {
            _commandBuffer = Display.StartPresentation(_renderTarget);

            Display.BlitImageToCurrentImage(_renderTarget, _commandBuffer.InternalHandle);

            Display.EndPresentation(_commandBuffer);
        }
    }
}
