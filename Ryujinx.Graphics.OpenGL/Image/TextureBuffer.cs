using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL.Image
{
    class TextureBuffer : TextureBase, ITexture
    {
        private int _bufferOffset;
        private int _bufferSize;

        private BufferHandle _buffer;

        public TextureBuffer(TextureCreateInfo info) : base(info) {}

        public void CopyTo(ITexture destination, int firstLayer, int firstLevel)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            throw new NotSupportedException();
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            throw new NotSupportedException();
        }

        public byte[] GetData()
        {
            return Buffer.GetData(_buffer, _bufferOffset, _bufferSize);
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            Buffer.SetData(_buffer, _bufferOffset, data.Slice(0, Math.Min(data.Length, _bufferSize)));
        }

        public void SetStorage(BufferRange buffer)
        {
            if (buffer.Handle == _buffer &&
                buffer.Offset == _bufferOffset &&
                buffer.Size == _bufferSize)
            {
                return;
            }

            _buffer = buffer.Handle;
            _bufferOffset = buffer.Offset;
            _bufferSize = buffer.Size;

            Bind(0);

            SizedInternalFormat format = (SizedInternalFormat)FormatTable.GetFormatInfo(Info.Format).PixelInternalFormat;

            GL.TexBufferRange(TextureBufferTarget.TextureBuffer, format, _buffer.ToInt32(), (IntPtr)buffer.Offset, buffer.Size);
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteTexture(Handle);

                Handle = 0;
            }
        }

        public void Release()
        {
            Dispose();
        }
    }
}
