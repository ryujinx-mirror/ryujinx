using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class BitStreamWriter
    {
        private const int BufferSize = 8;

        private Stream _baseStream;

        private int _buffer;
        private int _bufferPos;

        public BitStreamWriter(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public void WriteBit(bool value)
        {
            WriteBits(value ? 1 : 0, 1);
        }

        public void WriteBits(int value, int valueSize)
        {
            int valuePos = 0;

            int remaining = valueSize;

            while (remaining > 0)
            {
                int copySize = remaining;

                int free = GetFreeBufferBits();

                if (copySize > free)
                {
                    copySize = free;
                }

                int mask = (1 << copySize) - 1;

                int srcShift = (valueSize  - valuePos)  - copySize;
                int dstShift = (BufferSize - _bufferPos) - copySize;

                _buffer |= ((value >> srcShift) & mask) << dstShift;

                valuePos   += copySize;
                _bufferPos += copySize;
                remaining  -= copySize;
            }
        }

        private int GetFreeBufferBits()
        {
            if (_bufferPos == BufferSize)
            {
                Flush();
            }

            return BufferSize - _bufferPos;
        }

        public void Flush()
        {
            if (_bufferPos != 0)
            {
                _baseStream.WriteByte((byte)_buffer);

                _buffer    = 0;
                _bufferPos = 0;
            }
        }
    }
}