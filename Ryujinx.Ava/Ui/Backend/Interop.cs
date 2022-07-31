using FluentAvalonia.Interop;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Ava.Ui.Backend
{
    public static class Interop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XWindowAttributes
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public int border_width;
            public int depth;
            public IntPtr visual;
            public IntPtr root;
            public int c_class;
            public int bit_gravity;
            public int win_gravity;
            public int backing_store;
            public IntPtr backing_planes;
            public IntPtr backing_pixel;
            public int save_under;
            public IntPtr colormap;
            public int map_installed;
            public int map_state;
            public IntPtr all_event_masks;
            public IntPtr your_event_mask;
            public IntPtr do_not_propagate_mask;
            public int override_direct;
            public IntPtr screen;
        }

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("libX11.so.6")]
        public static extern int XCloseDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(IntPtr display);
    }
}
