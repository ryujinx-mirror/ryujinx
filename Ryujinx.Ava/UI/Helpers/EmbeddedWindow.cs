using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Ryujinx.Ava.UI.Helper;
using SPB.Graphics;
using SPB.Platform;
using SPB.Platform.GLX;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using static Ryujinx.Ava.UI.Helpers.Win32NativeInterop;

namespace Ryujinx.Ava.UI.Helpers
{
    public class EmbeddedWindow : NativeControlHost
    {
        private WindowProc _wndProcDelegate;
        private string _className;

        protected GLXWindow X11Window { get; set; }
        protected IntPtr WindowHandle { get; set; }
        protected IntPtr X11Display { get; set; }
        protected IntPtr NsView { get; set; }
        protected IntPtr MetalLayer { get; set; }

        private UpdateBoundsCallbackDelegate _updateBoundsCallback;

        public event EventHandler<IntPtr> WindowCreated;
        public event EventHandler<Size> SizeChanged;

        protected virtual void OnWindowDestroyed() { }
        protected virtual void OnWindowDestroying()
        {
            WindowHandle = IntPtr.Zero;
            X11Display = IntPtr.Zero;
        }

        public EmbeddedWindow()
        {
            var stateObserverable = this.GetObservable(BoundsProperty);

            stateObserverable.Subscribe(StateChanged);

            this.Initialized += NativeEmbeddedWindow_Initialized;
        }

        public virtual void OnWindowCreated() { }

        private void NativeEmbeddedWindow_Initialized(object sender, EventArgs e)
        {
            OnWindowCreated();

            Task.Run(() =>
            {
                WindowCreated?.Invoke(this, WindowHandle);
            });
        }

        private void StateChanged(Rect rect)
        {
            SizeChanged?.Invoke(this, rect.Size);
            _updateBoundsCallback?.Invoke(rect);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (OperatingSystem.IsLinux())
            {
                return CreateLinux(parent);
            }
            else if (OperatingSystem.IsWindows())
            {
                return CreateWin32(parent);
            }
            else if (OperatingSystem.IsMacOS())
            {
                return CreateMacOs(parent);
            }

            return base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            OnWindowDestroying();

            if (OperatingSystem.IsLinux())
            {
                DestroyLinux();
            }
            else if (OperatingSystem.IsWindows())
            {
                DestroyWin32(control);
            }
            else if (OperatingSystem.IsMacOS())
            {
                DestroyMacOS();
            }
            else
            {
                base.DestroyNativeControlCore(control);
            }

            OnWindowDestroyed();
        }

        [SupportedOSPlatform("linux")]
        protected virtual IPlatformHandle CreateLinux(IPlatformHandle parent)
        {
            X11Window    = PlatformHelper.CreateOpenGLWindow(FramebufferFormat.Default, 0, 0, 100, 100) as GLXWindow;
            WindowHandle = X11Window.WindowHandle.RawHandle;
            X11Display   = X11Window.DisplayHandle.RawHandle;

            return new PlatformHandle(WindowHandle, "X11");
        }

        [SupportedOSPlatform("windows")]
        IPlatformHandle CreateWin32(IPlatformHandle parent)
        {
            _className = "NativeWindow-" + Guid.NewGuid();
            _wndProcDelegate = WndProc;
            var wndClassEx = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                hInstance = GetModuleHandle(null),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                style = ClassStyles.CS_OWNDC,
                lpszClassName = Marshal.StringToHGlobalUni(_className),
                hCursor = LoadCursor(IntPtr.Zero, (IntPtr)Cursors.IDC_ARROW)
            };

            var atom = RegisterClassEx(ref wndClassEx);

            var handle = CreateWindowEx(
                0,
                _className,
                "NativeWindow",
                WindowStyles.WS_CHILD,
                0,
                0,
                640,
                480,
                parent.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            WindowHandle = handle;
            
            Marshal.FreeHGlobal(wndClassEx.lpszClassName);
            
            return new PlatformHandle(WindowHandle, "HWND");
        }

        [SupportedOSPlatform("windows")]
        IntPtr WndProc(IntPtr hWnd, WindowsMessages msg, IntPtr wParam, IntPtr lParam)
        {
            var point = new Point((long)lParam & 0xFFFF, ((long)lParam >> 16) & 0xFFFF);
            var root = VisualRoot as Window;
            bool isLeft = false;
            switch (msg)
            {
                case WindowsMessages.LBUTTONDOWN:
                case WindowsMessages.RBUTTONDOWN:
                    isLeft = msg == WindowsMessages.LBUTTONDOWN;
                    this.RaiseEvent(new PointerPressedEventArgs(
                        this,
                        new Pointer(0, PointerType.Mouse, true),
                        root,
                        this.TranslatePoint(point, root).Value,
                        (ulong)Environment.TickCount64,
                        new PointerPointProperties(isLeft ? RawInputModifiers.LeftMouseButton : RawInputModifiers.RightMouseButton, isLeft ? PointerUpdateKind.LeftButtonPressed : PointerUpdateKind.RightButtonPressed),
                        KeyModifiers.None));
                    break;
                case WindowsMessages.LBUTTONUP:
                case WindowsMessages.RBUTTONUP:
                    isLeft = msg == WindowsMessages.LBUTTONUP;
                    this.RaiseEvent(new PointerReleasedEventArgs(
                        this,
                        new Pointer(0, PointerType.Mouse, true),
                        root,
                        this.TranslatePoint(point, root).Value,
                        (ulong)Environment.TickCount64,
                        new PointerPointProperties(isLeft ? RawInputModifiers.LeftMouseButton : RawInputModifiers.RightMouseButton, isLeft ? PointerUpdateKind.LeftButtonReleased : PointerUpdateKind.RightButtonReleased),
                        KeyModifiers.None,
                        isLeft ? MouseButton.Left : MouseButton.Right));
                    break;
                case WindowsMessages.MOUSEMOVE:
                    this.RaiseEvent(new PointerEventArgs(
                        PointerMovedEvent,
                        this,
                        new Pointer(0, PointerType.Mouse, true),
                        root,
                        this.TranslatePoint(point, root).Value,
                        (ulong)Environment.TickCount64,
                        new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
                        KeyModifiers.None));
                    break;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        [SupportedOSPlatform("macos")]
        IPlatformHandle CreateMacOs(IPlatformHandle parent)
        {
            MetalLayer = MetalHelper.GetMetalLayer(out IntPtr nsView, out _updateBoundsCallback);

            NsView = nsView;

            return new PlatformHandle(nsView, "NSView");
        }

        void DestroyLinux()
        {
            X11Window?.Dispose();
        }

        [SupportedOSPlatform("windows")]
        void DestroyWin32(IPlatformHandle handle)
        {
            DestroyWindow(handle.Handle);
            UnregisterClass(_className, GetModuleHandle(null));
        }

        [SupportedOSPlatform("macos")]
        void DestroyMacOS()
        {
            MetalHelper.DestroyMetalLayer(NsView, MetalLayer);
        }
    }
}