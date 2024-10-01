using Ryujinx.Graphics.OpenGL.Helper;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    public interface IOpenGLContext : IDisposable
    {
        void MakeCurrent();

        bool HasContext();
    }
}
