using System.Runtime.CompilerServices;

namespace ARMeilleure.Common
{
    static class BitUtils
    {
        private const int DeBrujinSequence = 0x77cb531;

        private static int[] DeBrujinLbsLut;

        static BitUtils()
        {
            DeBrujinLbsLut = new int[32];

            for (int index = 0; index < DeBrujinLbsLut.Length; index++)
            {
                uint lutIndex = (uint)(DeBrujinSequence * (1 << index)) >> 27;

                DeBrujinLbsLut[lutIndex] = index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LowestBitSet(int value)
        {
            if (value == 0)
            {
                return -1;
            }

            int lsb = value & -value;

            return DeBrujinLbsLut[(uint)(DeBrujinSequence * lsb) >> 27];
        }

        public static int HighestBitSet(int value)
        {
            if (value == 0)
            {
                return -1;
            }

            for (int bit = 31; bit >= 0; bit--)
            {
                if (((value >> bit) & 1) != 0)
                {
                    return bit;
                }
            }

            return -1;
        }

        private static readonly sbyte[] HbsNibbleLut = { -1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };

        public static int HighestBitSetNibble(int value) => HbsNibbleLut[value & 0b1111];

        public static long Replicate(long bits, int size)
        {
            long output = 0;

            for (int bit = 0; bit < 64; bit += size)
            {
                output |= bits << bit;
            }

            return output;
        }

        public static int CountBits(int value)
        {
            int count = 0;

            while (value != 0)
            {
                value &= ~(value & -value);

                count++;
            }

            return count;
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
