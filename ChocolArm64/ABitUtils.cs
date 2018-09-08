namespace ChocolArm64
{
    static class ABitUtils
    {
        public static int CountBitsSet(long Value)
        {
            int Count = 0;

            for (int Bit = 0; Bit < 64; Bit++)
            {
                Count += (int)(Value >> Bit) & 1;
            }

            return Count;
        }

        public static int HighestBitSet32(int Value)
        {
            for (int Bit = 31; Bit >= 0; Bit--)
            {
                if (((Value >> Bit) & 1) != 0)
                {
                    return Bit;
                }
            }

            return -1;
        }

        private static readonly sbyte[] HbsNibbleTbl = { -1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };

        public static int HighestBitSetNibble(int Value) => HbsNibbleTbl[Value & 0b1111];

        public static long Replicate(long Bits, int Size)
        {
            long Output = 0;

            for (int Bit = 0; Bit < 64; Bit += Size)
            {
                Output |= Bits << Bit;
            }

            return Output;
        }

        public static long FillWithOnes(int Bits)
        {
            return Bits == 64 ? -1L : (1L << Bits) - 1;
        }

        public static long RotateRight(long Bits, int Shift, int Size)
        {
            return (Bits >> Shift) | (Bits << (Size - Shift));
        }

        public static bool IsPow2(int Value)
        {
            return Value != 0 && (Value & (Value - 1)) == 0;
        }
    }
}
