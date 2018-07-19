using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLStreamBuffer : IDisposable
    {
        public int Handle { get; protected set; }

        public int Size { get; protected set; }

        protected BufferTarget Target { get; private set; }

        public OGLStreamBuffer(BufferTarget Target, int Size)
        {
            this.Target = Target;
            this.Size   = Size;

            Handle = GL.GenBuffer();

            GL.BindBuffer(Target, Handle);

            GL.BufferData(Target, Size, IntPtr.Zero, BufferUsageHint.StreamDraw);
        }

        public void SetData(int Size, IntPtr HostAddress)
        {
            GL.BindBuffer(Target, Handle);

            GL.BufferSubData(Target, IntPtr.Zero, Size, HostAddress);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing && Handle != 0)
            {
                GL.DeleteBuffer(Handle);

                Handle = 0;
            }
        }
    }
}
