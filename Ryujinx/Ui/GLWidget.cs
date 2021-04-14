using Gtk;
using SPB.Graphics;
using SPB.Graphics.OpenGL;
using SPB.Platform;
using SPB.Platform.GLX;
using SPB.Platform.WGL;
using SPB.Windowing;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Ryujinx.Ui
{
    [ToolboxItem(true)]
    public class GLWidget : DrawingArea
    {
        private bool _initialized;

        public event EventHandler Initialized;
        public event EventHandler ShuttingDown;

        public OpenGLContextBase OpenGLContext { get; private set; }
        public NativeWindowBase NativeWindow { get; private set; }

        public FramebufferFormat FramebufferFormat { get; }
        public int GLVersionMajor { get; }
        public int GLVersionMinor { get; }
        public OpenGLContextFlags ContextFlags { get; }

        public bool DirectRendering { get; }
        public OpenGLContextBase SharedContext { get; }

        public GLWidget(FramebufferFormat framebufferFormat, int major, int minor, OpenGLContextFlags flags = OpenGLContextFlags.Default, bool directRendering = true, OpenGLContextBase sharedContext = null)
        {
            FramebufferFormat = framebufferFormat;
            GLVersionMajor = major;
            GLVersionMinor = minor;
            ContextFlags = flags;
            DirectRendering = directRendering;
            SharedContext = sharedContext;
        }

        protected override bool OnDrawn(Cairo.Context cr)
        {
            if (!_initialized)
            {
                Intialize();
            }

            return true;
        }

        private NativeWindowBase RetrieveNativeWindow()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr windowHandle = gdk_win32_window_get_handle(Window.Handle);

                return new WGLWindow(new NativeHandle(windowHandle));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                IntPtr displayHandle = gdk_x11_display_get_xdisplay(Display.Handle);
                IntPtr windowHandle = gdk_x11_window_get_xid(Window.Handle);

                return new GLXWindow(new NativeHandle(displayHandle), new NativeHandle(windowHandle));
            }

            throw new NotImplementedException();
        }

        [DllImport("libgdk-3-0.dll")]
        private static extern IntPtr gdk_win32_window_get_handle(IntPtr d);

        [DllImport("libgdk-3.so.0")]
        private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr gdkDisplay);

        [DllImport("libgdk-3.so.0")]
        private static extern IntPtr gdk_x11_window_get_xid(IntPtr gdkWindow);

        private void Intialize()
        {
            NativeWindow = RetrieveNativeWindow();

            Window.EnsureNative();

            OpenGLContext = PlatformHelper.CreateOpenGLContext(FramebufferFormat, GLVersionMajor, GLVersionMinor, ContextFlags, DirectRendering, SharedContext);

            OpenGLContext.Initialize(NativeWindow);
            OpenGLContext.MakeCurrent(NativeWindow);

            _initialized = true;

            Initialized?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            // Try to bind the OpenGL context before calling the shutdown event
            try
            {
                OpenGLContext?.MakeCurrent(NativeWindow);
            }
            catch (Exception) { }

            ShuttingDown?.Invoke(this, EventArgs.Empty);

            // Unbind context and destroy everything
            try
            {
                OpenGLContext?.MakeCurrent(null);
            }
            catch (Exception) { }

            OpenGLContext.Dispose();
        }
    }
}
