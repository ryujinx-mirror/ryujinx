using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class H264BitStreamWriter : BitStreamWriter
    {
        public H264BitStreamWriter(Stream BaseStream) : base(BaseStream) { }

        public void WriteU(int Value, int ValueSize)
        {
            WriteBits(Value, ValueSize);
        }

        public void WriteSe(int Value)
        {
            WriteExpGolombCodedInt(Value);
        }

        public void WriteUe(int Value)
        {
            WriteExpGolombCodedUInt((uint)Value);
        }

        public void End()
        {
            WriteBit(true);

            Flush();
        }

        private void WriteExpGolombCodedInt(int Value)
        {
            int Sign = Value <= 0 ? 0 : 1;

            if (Value < 0)
            {
                Value = -Value;
            }

            Value = (Value << 1) - Sign;

            WriteExpGolombCodedUInt((uint)Value);
        }

        private void WriteExpGolombCodedUInt(uint Value)
        {
            int Size = 32 - CountLeadingZeros((int)Value + 1);

            WriteBits(1, Size);

            Value -= (1u << (Size - 1)) - 1;

            WriteBits((int)Value, Size - 1);
        }

        private static readonly byte[] ClzNibbleTbl = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static int CountLeadingZeros(int Value)
        {
            if (Value == 0)
            {
                return 32;
            }

            int NibbleIdx = 32;
            int PreCount, Count = 0;

            do
            {
                NibbleIdx -= 4;
                PreCount = ClzNibbleTbl[(Value >> NibbleIdx) & 0b1111];
                Count += PreCount;
            }
            while (PreCount == 4);

            return Count;
        }
    }
}