using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class Buffer
    {
        public static void Clear(BufferHandle destination, int offset, int size, uint value)
        {
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, destination.ToInt32());

            unsafe
            {
                uint* valueArr = stackalloc uint[1];

                valueArr[0] = value;

                GL.ClearBufferSubData(
                    BufferTarget.CopyWriteBuffer,
                    PixelInternalFormat.Rgba8ui,
                    (IntPtr)offset,
                    (IntPtr)size,
                    PixelFormat.RgbaInteger,
                    PixelType.UnsignedByte,
                    (IntPtr)valueArr);
            }
        }

        public static BufferHandle Create()
        {
            return Handle.FromInt32<BufferHandle>(GL.GenBuffer());
        }

        public static BufferHandle Create(int size)
        {
            int handle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, handle);
            GL.BufferData(BufferTarget.CopyWriteBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            return Handle.FromInt32<BufferHandle>(handle);
        }

        public static void Copy(BufferHandle source, BufferHandle destination, int srcOffset, int dstOffset, int size)
        {
            GL.BindBuffer(BufferTarget.CopyReadBuffer, source.ToInt32());
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, destination.ToInt32());

            GL.CopyBufferSubData(
                BufferTarget.CopyReadBuffer,
                BufferTarget.CopyWriteBuffer,
                (IntPtr)srcOffset,
                (IntPtr)dstOffset,
                (IntPtr)size);
        }

        public static byte[] GetData(BufferHandle buffer, int offset, int size)
        {
            GL.BindBuffer(BufferTarget.CopyReadBuffer, buffer.ToInt32());

            byte[] data = new byte[size];

            GL.GetBufferSubData(BufferTarget.CopyReadBuffer, (IntPtr)offset, size, data);

            return data;
        }

        public static void Resize(BufferHandle handle, int size)
        {
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, handle.ToInt32());
            GL.BufferData(BufferTarget.CopyWriteBuffer, size, IntPtr.Zero, BufferUsageHint.StreamCopy);
        }

        public static void SetData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data)
        {
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, buffer.ToInt32());

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    GL.BufferSubData(BufferTarget.CopyWriteBuffer, (IntPtr)offset, data.Length, (IntPtr)ptr);
                }
            }
        }

        public static void Delete(BufferHandle buffer)
        {
            GL.DeleteBuffer(buffer.ToInt32());
        }
    }
}
