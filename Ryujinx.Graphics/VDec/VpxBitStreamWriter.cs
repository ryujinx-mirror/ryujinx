using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class VpxBitStreamWriter : BitStreamWriter
    {
        public VpxBitStreamWriter(Stream BaseStream) : base(BaseStream) { }

        public void WriteU(int Value, int ValueSize)
        {
            WriteBits(Value, ValueSize);
        }

        public void WriteS(int Value, int ValueSize)
        {
            bool Sign = Value < 0;

            if (Sign)
            {
                Value = -Value;
            }

            WriteBits((Value << 1) | (Sign ? 1 : 0), ValueSize + 1);
        }

        public void WriteDeltaQ(int Value)
        {
            bool DeltaCoded = Value != 0;

            WriteBit(DeltaCoded);

            if (DeltaCoded)
            {
                WriteBits(Value, 4);
            }
        }
    }
}