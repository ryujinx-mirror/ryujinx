using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Buffer : IBuffer
    {
        public int Handle { get; }

        public Buffer(int size)
        {
            Handle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, Handle);
            GL.BufferData(BufferTarget.CopyWriteBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        public void CopyTo(IBuffer destination, int srcOffset, int dstOffset, int size)
        {
            GL.BindBuffer(BufferTarget.CopyReadBuffer, Handle);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, ((Buffer)destination).Handle);

            GL.CopyBufferSubData(
                BufferTarget.CopyReadBuffer,
                BufferTarget.CopyWriteBuffer,
                (IntPtr)srcOffset,
                (IntPtr)dstOffset,
                (IntPtr)size);
        }

        public byte[] GetData(int offset, int size)
        {
            GL.BindBuffer(BufferTarget.CopyReadBuffer, Handle);

            byte[] data = new byte[size];

            GL.GetBufferSubData(BufferTarget.CopyReadBuffer, (IntPtr)offset, size, data);

            return data;
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, Handle);

                fixed (byte* ptr = data)
                {
                    GL.BufferData(BufferTarget.CopyWriteBuffer, data.Length, (IntPtr)ptr, BufferUsageHint.DynamicDraw);
                }
            }
        }

        public void SetData(int offset, ReadOnlySpan<byte> data)
        {
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, Handle);

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    GL.BufferSubData(BufferTarget.CopyWriteBuffer, (IntPtr)offset, data.Length, (IntPtr)ptr);
                }
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
