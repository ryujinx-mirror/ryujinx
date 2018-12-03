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

        private Stream BaseStream;

        private uint LowValue;
        private uint Range;
        private int  Count;

        public VpxRangeEncoder(Stream BaseStream)
        {
            this.BaseStream = BaseStream;

            Range = 0xff;
            Count = -24;

            Write(false);
        }

        public void WriteByte(byte Value)
        {
            Write(Value, 8);
        }

        public void Write(int Value, int ValueSize)
        {
            for (int Bit = ValueSize - 1; Bit >= 0; Bit--)
            {
                Write(((Value >> Bit) & 1) != 0);
            }
        }

        public void Write(bool Bit)
        {
            Write(Bit, HalfProbability);
        }

        public void Write(bool Bit, int Probability)
        {
            uint Range = this.Range;

            uint Split = 1 + (((Range - 1) * (uint)Probability) >> 8);

            Range = Split;

            if (Bit)
            {
                LowValue += Split;
                Range     = this.Range - Split;
            }

            int Shift = NormLut[Range];

            Range <<= Shift;
            Count +=  Shift;

            if (Count >= 0)
            {
                int Offset = Shift - Count;

                if (((LowValue << (Offset - 1)) >> 31) != 0)
                {
                    long CurrentPos = BaseStream.Position;

                    BaseStream.Seek(-1, SeekOrigin.Current);

                    while (BaseStream.Position >= 0 && PeekByte() == 0xff)
                    {
                        BaseStream.WriteByte(0);

                        BaseStream.Seek(-2, SeekOrigin.Current);
                    }

                    BaseStream.WriteByte((byte)(PeekByte() + 1));

                    BaseStream.Seek(CurrentPos, SeekOrigin.Begin);
                }

                BaseStream.WriteByte((byte)(LowValue >> (24 - Offset)));

                LowValue <<= Offset;
                Shift      = Count;
                LowValue  &= 0xffffff;
                Count     -= 8;
            }

            LowValue <<= Shift;

            this.Range = Range;
        }

        private byte PeekByte()
        {
            byte Value = (byte)BaseStream.ReadByte();

            BaseStream.Seek(-1, SeekOrigin.Current);

            return Value;
        }

        public void End()
        {
            for (int Index = 0; Index < 32; Index++)
            {
                Write(false);
            }
        }
    }
}