using Avalonia.Platform;
using Ryujinx.Ava.Ui.Controls;
using Silk.NET.Vulkan;
using SPB.Graphics.Vulkan;
using SPB.Platform.GLX;
using SPB.Platform.Metal;
using SPB.Platform.Win32;
using SPB.Platform.X11;
using SPB.Windowing;
using System;
using System.Runtime.Versioning;

namespace Ryujinx.Ava.Ui
{
    public class VulkanEmbeddedWindow : EmbeddedWindow
    {
        private NativeWindowBase _window;

        [SupportedOSPlatform("linux")]
        protected override IPlatformHandle CreateLinux(IPlatformHandle parent)
        {
            X11Window    = new GLXWindow(new NativeHandle(X11.DefaultDisplay), new NativeHandle(parent.Handle));
            WindowHandle = X11Window.WindowHandle.RawHandle;
            X11Display   = X11Window.DisplayHandle.RawHandle;

            X11Window.Hide();

            return new PlatformHandle(WindowHandle, "X11");
        }

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
            else if (OperatingSystem.IsMacOS())
            {
                _window = new SimpleMetalWindow(new NativeHandle(NsView), new NativeHandle(MetalLayer));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return new SurfaceKHR((ulong?)VulkanHelper.CreateWindowSurface(instance.Handle, _window));
        }
    }
}