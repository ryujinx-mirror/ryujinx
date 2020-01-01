using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class VpxRangeEncoder
    {
        private const int HalfProbability = 128;

        private static readonly int[] NormLut = new int[]
        {
            0, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private Stream _baseStream;

        private uint _lowValue;
        private uint _range;
        private int  _count;

        public VpxRangeEncoder(Stream baseStream)
        {
            _baseStream = baseStream;

            _range = 0xff;
            _count = -24;

            Write(false);
        }

        public void WriteByte(byte value)
        {
            Write(value, 8);
        }

        public void Write(int value, int valueSize)
        {
            for (int bit = valueSize - 1; bit >= 0; bit--)
            {
                Write(((value >> bit) & 1) != 0);
            }
        }

        public void Write(bool bit)
        {
            Write(bit, HalfProbability);
        }

        public void Write(bool bit, int probability)
        {
            uint range = _range;

            uint split = 1 + (((range - 1) * (uint)probability) >> 8);

            range = split;

            if (bit)
            {
                _lowValue += split;
                range      = _range - split;
            }

            int shift = NormLut[range];

            range  <<= shift;
            _count  += shift;

            if (_count >= 0)
            {
                int offset = shift - _count;

                if (((_lowValue << (offset - 1)) >> 31) != 0)
                {
                    long currentPos = _baseStream.Position;

                    _baseStream.Seek(-1, SeekOrigin.Current);

                    while (_baseStream.Position >= 0 && PeekByte() == 0xff)
                    {
                        _baseStream.WriteByte(0);

                        _baseStream.Seek(-2, SeekOrigin.Current);
                    }

                    _baseStream.WriteByte((byte)(PeekByte() + 1));

                    _baseStream.Seek(currentPos, SeekOrigin.Begin);
                }

                _baseStream.WriteByte((byte)(_lowValue >> (24 - offset)));

                _lowValue <<= offset;
                shift       = _count;
                _lowValue  &= 0xffffff;
                _count     -= 8;
            }

            _lowValue <<= shift;

            _range = range;
        }

        private byte PeekByte()
        {
            byte value = (byte)_baseStream.ReadByte();

            _baseStream.Seek(-1, SeekOrigin.Current);

            return value;
        }

        public void End()
        {
            for (int index = 0; index < 32; index++)
            {
                Write(false);
            }
        }
    }
}