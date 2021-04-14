using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Graphics.OpenGL.Helper
{
    [SupportedOSPlatform("windows")]
    internal static class WGLHelper
    {
        private const string LibraryName = "OPENGL32.DLL";

        [DllImport(LibraryName, EntryPoint = "wglGetCurrentContext")]
        public extern static IntPtr GetCurrentContext();
    }
}
