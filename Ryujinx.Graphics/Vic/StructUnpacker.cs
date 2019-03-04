using Ryujinx.Graphics.Memory;
using System;

namespace Ryujinx.Graphics.Vic
{
    class StructUnpacker
    {
        private NvGpuVmm _vmm;

        private long _position;

        private ulong _buffer;
        private int   _buffPos;

        public StructUnpacker(NvGpuVmm vmm, long position)
        {
            _vmm      = vmm;
            _position = position;

            _buffPos = 64;
        }

        public int Read(int bits)
        {
            if ((uint)bits > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bits));
            }

            int value = 0;

            while (bits > 0)
            {
                RefillBufferIfNeeded();

                int readBits = bits;

                int maxReadBits = 64 - _buffPos;

                if (readBits > maxReadBits)
                {
                    readBits = maxReadBits;
                }

                value <<= readBits;

                value |= (int)(_buffer >> _buffPos) & (int)(0xffffffff >> (32 - readBits));

                _buffPos += readBits;

                bits -= readBits;
            }

            return value;
        }

        private void RefillBufferIfNeeded()
        {
            if (_buffPos >= 64)
            {
                _buffer = _vmm.ReadUInt64(_position);

                _position += 8;

                _buffPos = 0;
            }
        }
    }
}