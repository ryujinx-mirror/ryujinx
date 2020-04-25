using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class TextureBuffer : TextureBase, ITexture
    {
        private int _bufferOffset;
        private int _bufferSize;

        private Buffer _buffer;

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
            return _buffer?.GetData(_bufferOffset, _bufferSize);
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            _buffer?.SetData(_bufferOffset, data.Slice(0, Math.Min(data.Length, _bufferSize)));
        }

        public void SetStorage(BufferRange buffer)
        {
            if (buffer.Buffer == _buffer &&
                buffer.Offset == _bufferOffset &&
                buffer.Size == _bufferSize)
            {
                return;
            }

            _buffer = (Buffer)buffer.Buffer;
            _bufferOffset = buffer.Offset;
            _bufferSize = buffer.Size;

            Bind(0);

            SizedInternalFormat format = (SizedInternalFormat)FormatTable.GetFormatInfo(Info.Format).PixelInternalFormat;

            GL.TexBufferRange(TextureBufferTarget.TextureBuffer, format, _buffer.Handle, (IntPtr)buffer.Offset, buffer.Size);
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteTexture(Handle);

                Handle = 0;
            }
        }
    }
}
