using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Holds inline index buffer state.
    /// The inline index buffer data is sent to the GPU through the command buffer.
    /// </summary>
    struct IbStreamer
    {
        private const int BufferCapacity = 256; // Must be a power of 2.

        private BufferHandle _inlineIndexBuffer;
        private int _inlineIndexBufferSize;
        private int _inlineIndexCount;
        private uint[] _buffer;
#pragma warning disable IDE0051 // Remove unused private member
        private readonly int _bufferOffset;
#pragma warning restore IDE0051

        /// <summary>
        /// Indicates if any index buffer data has been pushed.
        /// </summary>
        public readonly bool HasInlineIndexData => _inlineIndexCount != 0;

        /// <summary>
        /// Total numbers of indices that have been pushed.
        /// </summary>
        public readonly int InlineIndexCount => _inlineIndexCount;

        /// <summary>
        /// Gets the handle for the host buffer currently holding the inline index buffer data.
        /// </summary>
        /// <returns>Host buffer handle</returns>
        public readonly BufferHandle GetInlineIndexBuffer()
        {
            return _inlineIndexBuffer;
        }

        /// <summary>
        /// Gets the number of elements on the current inline index buffer,
        /// while also resetting it to zero for the next draw.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <returns>Inline index buffer count</returns>
        public int GetAndResetInlineIndexCount(IRenderer renderer)
        {
            UpdateRemaining(renderer);
            int temp = _inlineIndexCount;
            _inlineIndexCount = 0;
            return temp;
        }

        /// <summary>
        /// Pushes four 8-bit index buffer elements.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="argument">Method call argument</param>
        public void VbElementU8(IRenderer renderer, int argument)
        {
            byte i0 = (byte)argument;
            byte i1 = (byte)(argument >> 8);
            byte i2 = (byte)(argument >> 16);
            byte i3 = (byte)(argument >> 24);

            int offset = _inlineIndexCount;

            PushData(renderer, offset, i0);
            PushData(renderer, offset + 1, i1);
            PushData(renderer, offset + 2, i2);
            PushData(renderer, offset + 3, i3);

            _inlineIndexCount += 4;
        }

        /// <summary>
        /// Pushes two 16-bit index buffer elements.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="argument">Method call argument</param>
        public void VbElementU16(IRenderer renderer, int argument)
        {
            ushort i0 = (ushort)argument;
            ushort i1 = (ushort)(argument >> 16);

            int offset = _inlineIndexCount;

            PushData(renderer, offset, i0);
            PushData(renderer, offset + 1, i1);

            _inlineIndexCount += 2;
        }

        /// <summary>
        /// Pushes one 32-bit index buffer element.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="argument">Method call argument</param>
        public void VbElementU32(IRenderer renderer, int argument)
        {
            uint i0 = (uint)argument;

            int offset = _inlineIndexCount++;

            PushData(renderer, offset, i0);
        }

        /// <summary>
        /// Pushes a 32-bit value to the index buffer.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="offset">Offset where the data should be written, in 32-bit words</param>
        /// <param name="value">Index value to be written</param>
        private void PushData(IRenderer renderer, int offset, uint value)
        {
            _buffer ??= new uint[BufferCapacity];

            // We upload data in chunks.
            // If we are at the start of a chunk, then the buffer might be full,
            // in that case we need to submit any existing data before overwriting the buffer.
            int subOffset = offset & (BufferCapacity - 1);

            if (subOffset == 0 && offset != 0)
            {
                int baseOffset = (offset - BufferCapacity) * sizeof(uint);
                BufferHandle buffer = GetInlineIndexBuffer(renderer, baseOffset, BufferCapacity * sizeof(uint));
                renderer.SetBufferData(buffer, baseOffset, MemoryMarshal.Cast<uint, byte>(_buffer));
            }

            _buffer[subOffset] = value;
        }

        /// <summary>
        /// Makes sure that any pending data is submitted to the GPU before the index buffer is used.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        private void UpdateRemaining(IRenderer renderer)
        {
            int offset = _inlineIndexCount;
            if (offset == 0)
            {
                return;
            }

            int count = offset & (BufferCapacity - 1);
            if (count == 0)
            {
                count = BufferCapacity;
            }

            int baseOffset = (offset - count) * sizeof(uint);
            int length = count * sizeof(uint);
            BufferHandle buffer = GetInlineIndexBuffer(renderer, baseOffset, length);
            renderer.SetBufferData(buffer, baseOffset, MemoryMarshal.Cast<uint, byte>(_buffer)[..length]);
        }

        /// <summary>
        /// Gets the handle of a buffer large enough to hold the data that will be written to <paramref name="offset"/>.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="offset">Offset where the data will be written</param>
        /// <param name="length">Number of bytes that will be written</param>
        /// <returns>Buffer handle</returns>
        private BufferHandle GetInlineIndexBuffer(IRenderer renderer, int offset, int length)
        {
            // Calculate a reasonable size for the buffer that can fit all the data,
            // and that also won't require frequent resizes if we need to push more data.
            int size = BitUtils.AlignUp(offset + length + 0x10, 0x200);

            if (_inlineIndexBuffer == BufferHandle.Null)
            {
                _inlineIndexBuffer = renderer.CreateBuffer(size, BufferAccess.Stream);
                _inlineIndexBufferSize = size;
            }
            else if (_inlineIndexBufferSize < size)
            {
                BufferHandle oldBuffer = _inlineIndexBuffer;
                int oldSize = _inlineIndexBufferSize;

                _inlineIndexBuffer = renderer.CreateBuffer(size, BufferAccess.Stream);
                _inlineIndexBufferSize = size;

                renderer.Pipeline.CopyBuffer(oldBuffer, _inlineIndexBuffer, 0, 0, oldSize);
                renderer.DeleteBuffer(oldBuffer);
            }

            return _inlineIndexBuffer;
        }
    }
}
