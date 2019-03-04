using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class VpxBitStreamWriter : BitStreamWriter
    {
        public VpxBitStreamWriter(Stream baseStream) : base(baseStream) { }

        public void WriteU(int value, int valueSize)
        {
            WriteBits(value, valueSize);
        }

        public void WriteS(int value, int valueSize)
        {
            bool sign = value < 0;

            if (sign)
            {
                value = -value;
            }

            WriteBits((value << 1) | (sign ? 1 : 0), valueSize + 1);
        }

        public void WriteDeltaQ(int value)
        {
            bool deltaCoded = value != 0;

            WriteBit(deltaCoded);

            if (deltaCoded)
            {
                WriteBits(value, 4);
            }
        }
    }
}