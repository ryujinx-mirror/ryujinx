using System;
using Avalonia;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Ui.Vulkan.Surfaces
{
    public interface IVulkanPlatformSurface : IDisposable
    {
        float Scaling { get; }
        PixelSize SurfaceSize { get; }
        SurfaceKHR CreateSurface(VulkanInstance instance);
    }
}
