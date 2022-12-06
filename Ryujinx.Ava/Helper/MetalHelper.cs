using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Avalonia;

namespace Ryujinx.Ava.Ui.Helper
{
    public delegate void UpdateBoundsCallbackDelegate(Rect rect);

    [SupportedOSPlatform("macos")]
    static class MetalHelper
    {
        private const string LibObjCImport = "/usr/lib/libobjc.A.dylib";

        private struct Selector
        {
            public readonly IntPtr NativePtr;

            public unsafe Selector(string value)
            {
                int size = System.Text.Encoding.UTF8.GetMaxByteCount(value.Length);
                byte* data = stackalloc byte[size];

                fixed (char* pValue = value)
                {
                    System.Text.Encoding.UTF8.GetBytes(pValue, value.Length, data, size);
                }

                NativePtr = sel_registerName(data);
            }

            public static implicit operator Selector(string value) => new Selector(value);
        }

        private static unsafe IntPtr GetClass(string value)
        {
            int size = System.Text.Encoding.UTF8.GetMaxByteCount(value.Length);
            byte* data = stackalloc byte[size];

            fixed (char* pValue = value)
            {
                System.Text.Encoding.UTF8.GetBytes(pValue, value.Length, data, size);
            }

            return objc_getClass(data);
        }

        private struct NSPoint
        {
            public double X;
            public double Y;

            public NSPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private struct NSRect
        {
            public NSPoint Pos;
            public NSPoint Size;

            public NSRect(double x, double y, double width, double height)
            {
                Pos = new NSPoint(x, y);
                Size = new NSPoint(width, height);
            }
        }

        public static IntPtr GetMetalLayer(out IntPtr nsView, out UpdateBoundsCallbackDelegate updateBounds)
        {
            // Create a new CAMetalLayer.
            IntPtr layerClass = GetClass("CAMetalLayer");
            IntPtr metalLayer = IntPtr_objc_msgSend(layerClass, "alloc");
            objc_msgSend(metalLayer, "init");

            // Create a child NSView to render into.
            IntPtr nsViewClass = GetClass("NSView");
            IntPtr child = IntPtr_objc_msgSend(nsViewClass, "alloc");
            objc_msgSend(child, "init", new NSRect(0, 0, 0, 0));

            // Make its renderer our metal layer.
            objc_msgSend(child, "setWantsLayer:", (byte)1);
            objc_msgSend(child, "setLayer:", metalLayer);
            objc_msgSend(metalLayer, "setContentsScale:", Program.DesktopScaleFactor);

            // Ensure the scale factor is up to date.
            updateBounds = (Rect rect) => {
                objc_msgSend(metalLayer, "setContentsScale:", Program.DesktopScaleFactor);
            };

            nsView = child;
            return metalLayer;
        }

        public static void DestroyMetalLayer(IntPtr nsView, IntPtr metalLayer)
        {
            // TODO
        }

        [DllImport(LibObjCImport)]
        private static unsafe extern IntPtr sel_registerName(byte* data);

        [DllImport(LibObjCImport)]
        private static unsafe extern IntPtr objc_getClass(byte* data);

        [DllImport(LibObjCImport)]
        private static extern void objc_msgSend(IntPtr receiver, Selector selector);

        [DllImport(LibObjCImport)]
        private static extern void objc_msgSend(IntPtr receiver, Selector selector, byte value);

        [DllImport(LibObjCImport)]
        private static extern void objc_msgSend(IntPtr receiver, Selector selector, IntPtr value);

        [DllImport(LibObjCImport)]
        private static extern void objc_msgSend(IntPtr receiver, Selector selector, NSRect point);

        [DllImport(LibObjCImport)]
        private static extern void objc_msgSend(IntPtr receiver, Selector selector, double value);

        [DllImport(LibObjCImport, EntryPoint = "objc_msgSend")]
        private static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector);
    }
}