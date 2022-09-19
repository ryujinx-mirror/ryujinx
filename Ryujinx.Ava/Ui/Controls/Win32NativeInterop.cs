using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Ava.Ui.Controls
{
    [SupportedOSPlatform("windows")]
    internal class Win32NativeInterop
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public ClassStyles style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WindowProc lpfnWndProc; // not WndProc
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public IntPtr hIconSm;

            public static WNDCLASSEX Create()
            {
                return new WNDCLASSEX
                {
                    cbSize = Marshal.SizeOf<WNDCLASSEX>()
                };
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX param);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern short UnregisterClass([MarshalAs(UnmanagedType.LPWStr)] string lpClassName, IntPtr instance);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowsMessages msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
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
