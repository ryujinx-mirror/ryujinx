using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    /// <summary>
    /// Holds inline index buffer state.
    /// The inline index buffer data is sent to the GPU through the command buffer.
    /// </summary>
    struct IbStreamer
    {
        private BufferHandle _inlineIndexBuffer;
        private int _inlineIndexBufferSize;
        private int _inlineIndexCount;

        public bool HasInlineIndexData => _inlineIndexCount != 0;

        /// <summary>
        /// Gets the handle for the host buffer currently holding the inline index buffer data.
        /// </summary>
        /// <returns>Host buffer handle</returns>
        public BufferHandle GetInlineIndexBuffer()
        {
            return _inlineIndexBuffer;
        }

        /// <summary>
        /// Gets the number of elements on the current inline index buffer,
        /// while also reseting it to zero for the next draw.
        /// </summary>
        /// <returns>Inline index bufffer count</returns>
        public int GetAndResetInlineIndexCount()
        {
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

            Span<uint> data = stackalloc uint[4];

            data[0] = i0;
            data[1] = i1;
            data[2] = i2;
            data[3] = i3;

            int offset = _inlineIndexCount * 4;

            renderer.SetBufferData(GetInlineIndexBuffer(renderer, offset), offset, MemoryMarshal.Cast<uint, byte>(data));

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

            Span<uint> data = stackalloc uint[2];

            data[0] = i0;
            data[1] = i1;

            int offset = _inlineIndexCount * 4;

            renderer.SetBufferData(GetInlineIndexBuffer(renderer, offset), offset, MemoryMarshal.Cast<uint, byte>(data));

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

            Span<uint> data = stackalloc uint[1];

            data[0] = i0;

            int offset = _inlineIndexCount++ * 4;

            renderer.SetBufferData(GetInlineIndexBuffer(renderer, offset), offset, MemoryMarshal.Cast<uint, byte>(data));
        }

        /// <summary>
        /// Gets the handle of a buffer large enough to hold the data that will be written to <paramref name="offset"/>.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        /// <param name="offset">Offset where the data will be written</param>
        /// <returns>Buffer handle</returns>
        private BufferHandle GetInlineIndexBuffer(IRenderer renderer, int offset)
        {
            // Calculate a reasonable size for the buffer that can fit all the data,
            // and that also won't require frequent resizes if we need to push more data.
            int size = BitUtils.AlignUp(offset + 0x10, 0x200);

            if (_inlineIndexBuffer == BufferHandle.Null)
            {
                _inlineIndexBuffer = renderer.CreateBuffer(size);
                _inlineIndexBufferSize = size;
            }
            else if (_inlineIndexBufferSize < size)
            {
                BufferHandle oldBuffer = _inlineIndexBuffer;
                int oldSize = _inlineIndexBufferSize;

                _inlineIndexBuffer = renderer.CreateBuffer(size);
                _inlineIndexBufferSize = size;

                renderer.Pipeline.CopyBuffer(oldBuffer, _inlineIndexBuffer, 0, 0, oldSize);
                renderer.DeleteBuffer(oldBuffer);
            }

            return _inlineIndexBuffer;
        }
    }
}
