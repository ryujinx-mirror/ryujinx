using Ryujinx.Ava.Ui.Controls;
using Silk.NET.Vulkan;
using SPB.Graphics.Vulkan;
using SPB.Platform.Win32;
using SPB.Platform.X11;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.Ui
{
    public class VulkanEmbeddedWindow : EmbeddedWindow
    {
        private NativeWindowBase _window;

        public SurfaceKHR CreateSurface(Instance instance)
        {
            if (OperatingSystem.IsWindows())
            {
                _window = new SimpleWin32Window(new NativeHandle(WindowHandle));
            }
            else if (OperatingSystem.IsLinux())
            {
                _window = new SimpleX11Window(new NativeHandle(X11Display), new NativeHandle(WindowHandle));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return new SurfaceKHR((ulong?)VulkanHelper.CreateWindowSurface(instance.Handle, _window));
        }
    }
}