using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.UI.Common.Helper
{
    [SupportedOSPlatform("macos")]
    public static partial class ObjectiveC
    {
        private const string ObjCRuntime = "/usr/lib/libobjc.A.dylib";

        [LibraryImport(ObjCRuntime, StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr sel_getUid(string name);

        [LibraryImport(ObjCRuntime, StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr objc_getClass(string name);

        [LibraryImport(ObjCRuntime)]
        private static partial void objc_msgSend(IntPtr receiver, Selector selector);

        [LibraryImport(ObjCRuntime)]
        private static partial void objc_msgSend(IntPtr receiver, Selector selector, byte value);

        [LibraryImport(ObjCRuntime)]
        private static partial void objc_msgSend(IntPtr receiver, Selector selector, IntPtr value);

        [LibraryImport(ObjCRuntime)]
        private static partial void objc_msgSend(IntPtr receiver, Selector selector, NSRect point);

        [LibraryImport(ObjCRuntime)]
        private static partial void objc_msgSend(IntPtr receiver, Selector selector, double value);

        [LibraryImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
        private static partial IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector);

        [LibraryImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
        private static partial IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, IntPtr param);

        [LibraryImport(ObjCRuntime, EntryPoint = "objc_msgSend", StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr IntPtr_objc_msgSend(IntPtr receiver, Selector selector, string param);

        [LibraryImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool bool_objc_msgSend(IntPtr receiver, Selector selector, IntPtr param);

        public readonly struct Object
        {
            public readonly IntPtr ObjPtr;

            private Object(IntPtr pointer)
            {
                ObjPtr = pointer;
            }

            public Object(string name)
            {
                ObjPtr = objc_getClass(name);
            }

            public void SendMessage(Selector selector)
            {
                objc_msgSend(ObjPtr, selector);
            }

            public void SendMessage(Selector selector, byte value)
            {
                objc_msgSend(ObjPtr, selector, value);
            }

            public void SendMessage(Selector selector, Object obj)
            {
                objc_msgSend(ObjPtr, selector, obj.ObjPtr);
            }

            public void SendMessage(Selector selector, NSRect point)
            {
                objc_msgSend(ObjPtr, selector, point);
            }

            public void SendMessage(Selector selector, double value)
            {
                objc_msgSend(ObjPtr, selector, value);
            }

            public Object GetFromMessage(Selector selector)
            {
                return new Object(IntPtr_objc_msgSend(ObjPtr, selector));
            }

            public Object GetFromMessage(Selector selector, Object obj)
            {
                return new Object(IntPtr_objc_msgSend(ObjPtr, selector, obj.ObjPtr));
            }

            public Object GetFromMessage(Selector selector, NSString nsString)
            {
                return new Object(IntPtr_objc_msgSend(ObjPtr, selector, nsString.StrPtr));
            }

            public Object GetFromMessage(Selector selector, string param)
            {
                return new Object(IntPtr_objc_msgSend(ObjPtr, selector, param));
            }

            public bool GetBoolFromMessage(Selector selector, Object obj)
            {
                return bool_objc_msgSend(ObjPtr, selector, obj.ObjPtr);
            }
        }

        public readonly struct Selector
        {
            public readonly IntPtr SelPtr;

            private Selector(string name)
            {
                SelPtr = sel_getUid(name);
            }

            public static implicit operator Selector(string value) => new(value);
        }

        public readonly struct NSString
        {
            public readonly IntPtr StrPtr;

            public NSString(string aString)
            {
                IntPtr nsString = objc_getClass("NSString");
                StrPtr = IntPtr_objc_msgSend(nsString, "stringWithUTF8String:", aString);
            }

            public static implicit operator IntPtr(NSString nsString) => nsString.StrPtr;
        }

        public readonly struct NSPoint
        {
            public readonly double X;
            public readonly double Y;

            public NSPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public readonly struct NSRect
        {
            public readonly NSPoint Pos;
            public readonly NSPoint Size;

            public NSRect(double x, double y, double width, double height)
            {
                Pos = new NSPoint(x, y);
                Size = new NSPoint(width, height);
            }
        }
    }
}
