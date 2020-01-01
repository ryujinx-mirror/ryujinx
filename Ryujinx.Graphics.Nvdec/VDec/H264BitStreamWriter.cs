using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class H264BitStreamWriter : BitStreamWriter
    {
        public H264BitStreamWriter(Stream baseStream) : base(baseStream) { }

        public void WriteU(int value, int valueSize)
        {
            WriteBits(value, valueSize);
        }

        public void WriteSe(int value)
        {
            WriteExpGolombCodedInt(value);
        }

        public void WriteUe(int value)
        {
            WriteExpGolombCodedUInt((uint)value);
        }

        public void End()
        {
            WriteBit(true);

            Flush();
        }

        private void WriteExpGolombCodedInt(int value)
        {
            int sign = value <= 0 ? 0 : 1;

            if (value < 0)
            {
                value = -value;
            }

            value = (value << 1) - sign;

            WriteExpGolombCodedUInt((uint)value);
        }

        private void WriteExpGolombCodedUInt(uint value)
        {
            int size = 32 - CountLeadingZeros((int)value + 1);

            WriteBits(1, size);

            value -= (1u << (size - 1)) - 1;

            WriteBits((int)value, size - 1);
        }

        private static readonly byte[] ClzNibbleTbl = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static int CountLeadingZeros(int value)
        {
            if (value == 0)
            {
                return 32;
            }

            int nibbleIdx = 32;
            int preCount, count = 0;

            do
            {
                nibbleIdx -= 4;
                preCount = ClzNibbleTbl[(value >> nibbleIdx) & 0b1111];
                count += preCount;
            }
            while (preCount == 4);

            return count;
        }
    }
}