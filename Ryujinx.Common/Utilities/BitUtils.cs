using System.Numerics;

namespace Ryujinx.Common
{
    public static class BitUtils
    {
        public static uint AlignUp(uint value, int size)
        {
            return (uint)AlignUp((int)value, size);
        }

        public static int AlignUp(int value, int size)
        {
            return (value + (size - 1)) & -size;
        }

        public static ulong AlignUp(ulong value, int size)
        {
            return (ulong)AlignUp((long)value, size);
        }

        public static long AlignUp(long value, int size)
        {
            return AlignUp(value, (long)size);
        }

        public static ulong AlignUp(ulong value, ulong size)
        {
            return (ulong)AlignUp((long)value, (long)size);
        }

        public static long AlignUp(long value, long size)
        {
            return (value + (size - 1)) & -size;
        }

        public static uint AlignDown(uint value, int size)
        {
            return (uint)AlignDown((int)value, size);
        }

        public static int AlignDown(int value, int size)
        {
            return value & -size;
        }

        public static ulong AlignDown(ulong value, int size)
        {
            return (ulong)AlignDown((long)value, size);
        }

        public static long AlignDown(long value, int size)
        {
            return AlignDown(value, (long)size);
        }

        public static ulong AlignDown(ulong value, ulong size)
        {
            return (ulong)AlignDown((long)value, (long)size);
        }

        public static long AlignDown(long value, long size)
        {
            return value & -size;
        }

        public static int DivRoundUp(int value, int dividend)
        {
            return (value + dividend - 1) / dividend;
        }

        public static ulong DivRoundUp(ulong value, uint dividend)
        {
            return (value + dividend - 1) / dividend;
        }

        public static long DivRoundUp(long value, int dividend)
        {
            return (value + dividend - 1) / dividend;
        }

        public static int Pow2RoundUp(int value)
        {
            value--;

            value |= (value >>  1);
            value |= (value >>  2);
            value |= (value >>  4);
            value |= (value >>  8);
            value |= (value >> 16);

            return ++value;
        }

        public static int Pow2RoundDown(int value)
        {
            return BitOperations.IsPow2(value) ? value : Pow2RoundUp(value) >> 1;
        }

        public static long ReverseBits64(long value)
        {
            return (long)ReverseBits64((ulong)value);
        }

        private static ulong ReverseBits64(ulong value)
        {
            value = ((value & 0xaaaaaaaaaaaaaaaa) >> 1 ) | ((value & 0x5555555555555555) << 1 );
            value = ((value & 0xcccccccccccccccc) >> 2 ) | ((value & 0x3333333333333333) << 2 );
            value = ((value & 0xf0f0f0f0f0f0f0f0) >> 4 ) | ((value & 0x0f0f0f0f0f0f0f0f) << 4 );
            value = ((value & 0xff00ff00ff00ff00) >> 8 ) | ((value & 0x00ff00ff00ff00ff) << 8 );
            value = ((value & 0xffff0000ffff0000) >> 16) | ((value & 0x0000ffff0000ffff) << 16);

            return (value >> 32) | (value << 32);
        }
    }
}
