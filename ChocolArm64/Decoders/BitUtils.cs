namespace ChocolArm64.Decoders
{
    static class BitUtils
    {
        public static int HighestBitSet32(int value)
        {
            for (int bit = 31; bit >= 0; bit--)
            {
                if (((value >> bit) & 1) != 0)
                {
                    return bit;
                }
            }

            return -1;
        }

        private static readonly sbyte[] HbsNibbleTbl = { -1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };

        public static int HighestBitSetNibble(int value) => HbsNibbleTbl[value & 0b1111];

        public static long Replicate(long bits, int size)
        {
            long output = 0;

            for (int bit = 0; bit < 64; bit += size)
            {
                output |= bits << bit;
            }

            return output;
        }

        public static long FillWithOnes(int bits)
        {
            return bits == 64 ? -1L : (1L << bits) - 1;
        }

        public static int RotateRight(int bits, int shift, int size)
        {
            return (int)RotateRight((uint)bits, shift, size);
        }

        public static uint RotateRight(uint bits, int shift, int size)
        {
            return (bits >> shift) | (bits << (size - shift));
        }

        public static long RotateRight(long bits, int shift, int size)
        {
            return (long)RotateRight((ulong)bits, shift, size);
        }

        public static ulong RotateRight(ulong bits, int shift, int size)
        {
            return (bits >> shift) | (bits << (size - shift));
        }
    }
}
