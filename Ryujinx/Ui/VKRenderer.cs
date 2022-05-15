using Gdk;
using Gtk;
using Ryujinx.Common.Configuration;
using Ryujinx.Input.HLE;
using SPB.Graphics.Vulkan;
using SPB.Platform.Win32;
using SPB.Platform.X11;
using SPB.Windowing;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Ui
{
    public class VKRenderer : RendererWidgetBase
    {
        public NativeWindowBase NativeWindow { get; private set; }

        public VKRenderer(InputManager inputManager, GraphicsDebugLevel glLogLevel) : base(inputManager, glLogLevel) { }

        private NativeWindowBase RetrieveNativeWindow()
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr windowHandle = gdk_win32_window_get_handle(Window.Handle);

                return new SimpleWin32Window(new NativeHandle(windowHandle));
            }
            else if (OperatingSystem.IsLinux())
            {
                IntPtr displayHandle = gdk_x11_display_get_xdisplay(Display.Handle);
                IntPtr windowHandle = gdk_x11_window_get_xid(Window.Handle);

                return new SimpleX11Window(new NativeHandle(displayHandle), new NativeHandle(windowHandle));
            }

            throw new NotImplementedException();
        }

        [DllImport("libgdk-3-0.dll")]
        private static extern IntPtr gdk_win32_window_get_handle(IntPtr d);

        [DllImport("libgdk-3.so.0")]
        private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr gdkDisplay);

        [DllImport("libgdk-3.so.0")]
        private static extern IntPtr gdk_x11_window_get_xid(IntPtr gdkWindow);

        protected override bool OnConfigureEvent(EventConfigure evnt)
        {
            if (NativeWindow == null)
            {
                NativeWindow = RetrieveNativeWindow();

                WaitEvent.Set();
            }

            return base.OnConfigureEvent(evnt);
        }

        public unsafe IntPtr CreateWindowSurface(IntPtr instance)
        {
            return VulkanHelper.CreateWindowSurface(instance, NativeWindow);
        }

        public override void InitializeRenderer() { }

        public override void SwapBuffers(object image) { }

        public override string GetGpuVendorName()
        {
            return "Vulkan (Unknown)";
        }

        protected override void Dispose(bool disposing)
        {
            Device.DisposeGpu();
            NpadManager.Dispose();
        }
    }
}
