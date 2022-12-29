using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Ava.UI.Helpers
{
    [SupportedOSPlatform("windows")]
    internal partial class Win32NativeInterop
    {
        [Flags]
        public enum ClassStyles : uint
        {
            CS_CLASSDC = 0x40,
            CS_OWNDC = 0x20,
        }

        [Flags]
        public enum WindowStyles : uint
        {
            WS_CHILD = 0x40000000
        }

        public enum Cursors : uint
        {
            IDC_ARROW = 32512
        }

        public enum WindowsMessages : uint
        {
            MOUSEMOVE = 0x0200,
            LBUTTONDOWN = 0x0201,
            LBUTTONUP = 0x0202,
            LBUTTONDBLCLK = 0x0203,
            RBUTTONDOWN = 0x0204,
            RBUTTONUP = 0x0205,
            RBUTTONDBLCLK = 0x0206,
            MBUTTONDOWN = 0x0207,
            MBUTTONUP = 0x0208,
            MBUTTONDBLCLK = 0x0209,
            MOUSEWHEEL = 0x020A,
            XBUTTONDOWN = 0x020B,
            XBUTTONUP = 0x020C,
            XBUTTONDBLCLK = 0x020D,
            MOUSEHWHEEL = 0x020E,
            MOUSELAST = 0x020E
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr WindowProc(IntPtr hWnd, WindowsMessages msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public ClassStyles style;
            public IntPtr lpfnWndProc; // not WndProc
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public IntPtr lpszMenuName;
            public IntPtr lpszClassName;
            public IntPtr hIconSm;

            public WNDCLASSEX()
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>();
            }
        }

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassExW")]
        public static partial ushort RegisterClassEx(ref WNDCLASSEX param);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterClassW")]
        public static partial short UnregisterClass([MarshalAs(UnmanagedType.LPWStr)] string lpClassName, IntPtr instance);

        [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static partial IntPtr DefWindowProc(IntPtr hWnd, WindowsMessages msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleA")]
        public static partial IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPStr)] string lpModuleName);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DestroyWindow(IntPtr hwnd);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "LoadCursorA")]
        public static partial IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowExW")]
        public static partial IntPtr CreateWindowEx(
           uint dwExStyle,
           [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
           [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
           WindowStyles dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);
    }
}
