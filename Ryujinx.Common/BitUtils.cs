namespace Ryujinx.Common
{
    public static class BitUtils
    {
        public static int AlignUp(int Value, int Size)
        {
            return (Value + (Size - 1)) & -Size;
        }

        public static ulong AlignUp(ulong Value, int Size)
        {
            return (ulong)AlignUp((long)Value, Size);
        }

        public static long AlignUp(long Value, int Size)
        {
            return (Value + (Size - 1)) & -(long)Size;
        }

        public static int AlignDown(int Value, int Size)
        {
            return Value & -Size;
        }

        public static ulong AlignDown(ulong Value, int Size)
        {
            return (ulong)AlignDown((long)Value, Size);
        }

        public static long AlignDown(long Value, int Size)
        {
            return Value & -(long)Size;
        }

        public static ulong DivRoundUp(ulong Value, uint Dividend)
        {
            return (Value + Dividend - 1) / Dividend;
        }

        public static long DivRoundUp(long Value, int Dividend)
        {
            return (Value + Dividend - 1) / Dividend;
        }

        public static bool IsPowerOfTwo32(int Value)
        {
            return Value != 0 && (Value & (Value - 1)) == 0;
        }

        public static bool IsPowerOfTwo64(long Value)
        {
            return Value != 0 && (Value & (Value - 1)) == 0;
        }

        public static int CountLeadingZeros32(int Value)
        {
            return (int)CountLeadingZeros((ulong)Value, 32);
        }

        public static int CountLeadingZeros64(long Value)
        {
            return (int)CountLeadingZeros((ulong)Value, 64);
        }

        private static readonly byte[] ClzNibbleTbl = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static ulong CountLeadingZeros(ulong Value, int Size) // Size is 8, 16, 32 or 64 (SIMD&FP or Base Inst.).
        {
            if (Value == 0ul)
            {
                return (ulong)Size;
            }

            int NibbleIdx = Size;
            int PreCount, Count = 0;

            do
            {
                NibbleIdx -= 4;
                PreCount = ClzNibbleTbl[(Value >> NibbleIdx) & 0b1111];
                Count += PreCount;
            }
            while (PreCount == 4);

            return (ulong)Count;
        }

        public static long ReverseBits64(long Value)
        {
            return (long)ReverseBits64((ulong)Value);
        }

        private static ulong ReverseBits64(ulong Value)
        {
            Value = ((Value & 0xaaaaaaaaaaaaaaaa) >> 1 ) | ((Value & 0x5555555555555555) << 1 );
            Value = ((Value & 0xcccccccccccccccc) >> 2 ) | ((Value & 0x3333333333333333) << 2 );
            Value = ((Value & 0xf0f0f0f0f0f0f0f0) >> 4 ) | ((Value & 0x0f0f0f0f0f0f0f0f) << 4 );
            Value = ((Value & 0xff00ff00ff00ff00) >> 8 ) | ((Value & 0x00ff00ff00ff00ff) << 8 );
            Value = ((Value & 0xffff0000ffff0000) >> 16) | ((Value & 0x0000ffff0000ffff) << 16);

            return (Value >> 32) | (Value << 32);
        }
    }
}