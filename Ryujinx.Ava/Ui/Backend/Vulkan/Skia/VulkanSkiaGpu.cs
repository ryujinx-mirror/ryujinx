using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.X11;
using Ryujinx.Ava.Ui.Vulkan;
using Silk.NET.Vulkan;
using SkiaSharp;

namespace Ryujinx.Ava.Ui.Backend.Vulkan
{
    public class VulkanSkiaGpu : ISkiaGpu
    {
        private readonly VulkanPlatformInterface _vulkan;
        public long? MaxResourceBytes { get; }

        public VulkanSkiaGpu(long? maxResourceBytes)
        {
            _vulkan = AvaloniaLocator.Current.GetService<VulkanPlatformInterface>();
            MaxResourceBytes = maxResourceBytes;
        }

        public ISkiaGpuRenderTarget TryCreateRenderTarget(IEnumerable<object> surfaces)
        {
            foreach (var surface in surfaces)
            {
                VulkanWindowSurface window;

                if (surface is IPlatformHandle handle)
                {
                    window = new VulkanWindowSurface(handle.Handle);
                }
                else if (surface is X11FramebufferSurface x11FramebufferSurface)
                {
                    // As of Avalonia 0.10.13, an IPlatformHandle isn't passed for linux, so use reflection to otherwise get the window id
                    var xId = (IntPtr)x11FramebufferSurface.GetType().GetField(
                        "_xid",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(x11FramebufferSurface);

                    window = new VulkanWindowSurface(xId);
                }
                else
                {
                    continue;
                }

                VulkanRenderTarget vulkanRenderTarget = new VulkanRenderTarget(_vulkan, window);

                return vulkanRenderTarget;
            }

            return null;
        }

        public ISkiaSurface TryCreateSurface(PixelSize size, ISkiaGpuRenderSession session)
        {
            return null;
        }
    }
}
