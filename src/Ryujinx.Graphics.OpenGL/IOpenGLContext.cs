using Ryujinx.Graphics.OpenGL.Helper;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    public interface IOpenGLContext : IDisposable
    {
        void MakeCurrent();

        // TODO: Support more APIs per platform.
        static bool HasContext()
        {
            if (OperatingSystem.IsWindows())
            {
                return WGLHelper.GetCurrentContext() != IntPtr.Zero;
            }
            else if (OperatingSystem.IsLinux())
            {
                return GLXHelper.GetCurrentContext() != IntPtr.Zero;
            }
            else
            {
                return false;
            }
        }
    }
}
