using Avalonia;
using Avalonia.OpenGL;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Configuration;
using SPB.Graphics;
using SPB.Graphics.OpenGL;
using SPB.Platform;
using SPB.Platform.GLX;
using SPB.Platform.WGL;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.Ui.Controls
{
    public class OpenGLEmbeddedWindow : EmbeddedWindow
    {
        private readonly int _major;
        private readonly int _minor;
        private readonly GraphicsDebugLevel _graphicsDebugLevel;
        private SwappableNativeWindowBase _window;
        public OpenGLContextBase Context { get; set; }

        public OpenGLEmbeddedWindow(int major, int minor, GraphicsDebugLevel graphicsDebugLevel)
        {
            _major = major;
            _minor = minor;
            _graphicsDebugLevel = graphicsDebugLevel;
        }

        protected override void OnWindowDestroying()
        {
            Context.Dispose();
            base.OnWindowDestroying();
        }

        public override void OnWindowCreated()
        {
            base.OnWindowCreated();

            if (OperatingSystem.IsWindows())
            {
                _window = new WGLWindow(new NativeHandle(WindowHandle));
            }
            else if (OperatingSystem.IsLinux())
            {
                _window = X11Window;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            var flags = OpenGLContextFlags.Compat;
            if (_graphicsDebugLevel != GraphicsDebugLevel.None)
            {
                flags |= OpenGLContextFlags.Debug;
            }

            Context = PlatformHelper.CreateOpenGLContext(FramebufferFormat.Default, _major, _minor, flags);

            Context.Initialize(_window);
            Context.MakeCurrent(_window);

            var bindingsContext = new OpenToolkitBindingsContext(Context.GetProcAddress);

            GL.LoadBindings(bindingsContext);
            Context.MakeCurrent(null);
        }

        public void MakeCurrent()
        {
            Context?.MakeCurrent(_window);
        }

        public void MakeCurrent(NativeWindowBase window)
        {
            Context?.MakeCurrent(window);
        }

        public void SwapBuffers()
        {
            _window.SwapBuffers();
        }
    }
}