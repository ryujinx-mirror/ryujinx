using System;
using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    abstract class OGLStreamBuffer : IDisposable
    {
        public int Handle { get; protected set; }

        public int Size { get; protected set; }

        protected BufferTarget Target { get; private set; }

        private bool Mapped = false;

        public OGLStreamBuffer(BufferTarget Target, int MaxSize)
        {
            Handle = 0;
            Mapped = false;

            this.Target = Target;
            this.Size = MaxSize;
        }

        public static OGLStreamBuffer Create(BufferTarget Target, int MaxSize)
        {
            //TODO: Query here for ARB_buffer_storage and use when available
            return new SubDataBuffer(Target, MaxSize);
        }

        public byte[] Map(int Size)
        {
            if (Handle == 0 || Mapped || Size > this.Size)
            {
                throw new InvalidOperationException();
            }

            byte[] Memory = InternMap(Size);

            Mapped = true;

            return Memory;
        }

        public void Unmap(int UsedSize)
        {
            if (Handle == 0 || !Mapped)
            {
                throw new InvalidOperationException();
            }

            InternUnmap(UsedSize);

            Mapped = false;
        }

        protected abstract byte[] InternMap(int Size);

        protected abstract void InternUnmap(int UsedSize);

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

    class SubDataBuffer : OGLStreamBuffer
    {
        private byte[] Memory;

        public SubDataBuffer(BufferTarget Target, int MaxSize)
            : base(Target, MaxSize)
        {
            Memory = new byte[MaxSize];

            GL.GenBuffers(1, out int Handle);

            GL.BindBuffer(Target, Handle);

            GL.BufferData(Target, Size, IntPtr.Zero, BufferUsageHint.StreamDraw);

            this.Handle = Handle;
        }

        protected override byte[] InternMap(int Size)
        {
            return Memory;
        }

        protected override void InternUnmap(int UsedSize)
        {
            GL.BindBuffer(Target, Handle);
            
            unsafe
            {
                fixed (byte* MemoryPtr = Memory)
                {
                    GL.BufferSubData(Target, IntPtr.Zero, UsedSize, (IntPtr)MemoryPtr);
                }
            }
        }
    }
}
