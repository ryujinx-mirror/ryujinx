using Avalonia;
using System;
using System.Runtime.InteropServices;
using static Ryujinx.Ava.Ui.Backend.Interop;

namespace Ryujinx.Ava.Ui.Backend
{
    public abstract class BackendSurface : IDisposable
    {
        protected IntPtr Display => _display;

        private IntPtr _display = IntPtr.Zero;

        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern int XCloseDisplay(IntPtr display);

        private PixelSize _currentSize;
        public IntPtr Handle { get; protected set; }

        public bool IsDisposed { get; private set; }

        public BackendSurface(IntPtr handle)
        {
            Handle = handle;

            if (OperatingSystem.IsLinux())
            {
                _display = XOpenDisplay(IntPtr.Zero);
            }
        }

        public PixelSize Size
        {
            get
            {
                PixelSize size = new PixelSize();
                if (OperatingSystem.IsWindows())
                {
                    GetClientRect(Handle, out var rect);
                    size = new PixelSize(rect.right, rect.bottom);
                }
                else if (OperatingSystem.IsLinux())
                {
                    XWindowAttributes attributes = new XWindowAttributes();
                    XGetWindowAttributes(Display, Handle, ref attributes);

                    size = new PixelSize(attributes.width, attributes.height);
                }

                _currentSize = size;

                return size;
            }
        }

        public PixelSize CurrentSize => _currentSize;

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BackendSurface));
            }

            IsDisposed = true;

            if (_display != IntPtr.Zero)
            {
                XCloseDisplay(_display);
            }
        }
    }
}