using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class MersenneTwister
    {
        private int Index;
        private uint[] Mt;

        public MersenneTwister(uint Seed)
        {
            Mt = new uint[624];

            Mt[0] = Seed;

            for (int MtIdx = 1; MtIdx < Mt.Length; MtIdx++)
            {
                uint Prev = Mt[MtIdx - 1];

                Mt[MtIdx] = (uint)(0x6c078965 * (Prev ^ (Prev >> 30)) + MtIdx);
            }

            Index = Mt.Length;
        }

        public long GenRandomNumber(long Min, long Max)
        {
            long Range = Max - Min;

            if (Min == Max)
            {
                return Min;
            }

            if (Range == -1)
            {
                //Increment would cause a overflow, special case.
                return GenRandomNumber(2, 2, 32, 0xffffffffu, 0xffffffffu);
            }

            Range++;

            //This is log2(Range) plus one.
            int NextRangeLog2 = 64 - BitUtils.CountLeadingZeros64(Range);

            //If Range is already power of 2, subtract one to use log2(Range) directly.
            int RangeLog2 = NextRangeLog2 - (BitUtils.IsPowerOfTwo64(Range) ? 1 : 0);

            int Parts       = RangeLog2 > 32 ? 2 : 1;
            int BitsPerPart = RangeLog2 / Parts;

            int FullParts = Parts - (RangeLog2 - Parts * BitsPerPart);

            uint Mask      = 0xffffffffu >> (32 - BitsPerPart);
            uint MaskPlus1 = 0xffffffffu >> (31 - BitsPerPart);

            long RandomNumber;

            do
            {
                RandomNumber = GenRandomNumber(Parts, FullParts, BitsPerPart, Mask, MaskPlus1);
            }
            while ((ulong)RandomNumber >= (ulong)Range);

            return Min + RandomNumber;
        }

        private long GenRandomNumber(
            int  Parts,
            int  FullParts,
            int  BitsPerPart,
            uint Mask,
            uint MaskPlus1)
        {
            long RandomNumber = 0;

            int Part = 0;

            for (; Part < FullParts; Part++)
            {
                RandomNumber <<= BitsPerPart;
                RandomNumber  |= GenRandomNumber() & Mask;
            }

            for (; Part < Parts; Part++)
            {
                RandomNumber <<= BitsPerPart + 1;
                RandomNumber  |= GenRandomNumber() & MaskPlus1;
            }

            return RandomNumber;
        }

        private uint GenRandomNumber()
        {
            if (Index >= Mt.Length)
            {
                Twist();
            }

            uint Value = Mt[Index++];

            Value ^= Value >> 11;
            Value ^= (Value << 7) & 0x9d2c5680;
            Value ^= (Value << 15) & 0xefc60000;
            Value ^= Value >> 18;

            return Value;
        }

        private void Twist()
        {
            for (int MtIdx = 0; MtIdx < Mt.Length; MtIdx++)
            {
                uint Value = (Mt[MtIdx] & 0x80000000) + (Mt[(MtIdx + 1) % Mt.Length] & 0x7fffffff);

                Mt[MtIdx] = Mt[(MtIdx + 397) % Mt.Length] ^ (Value >> 1);

                if ((Value & 1) != 0)
                {
                    Mt[MtIdx] ^= 0x9908b0df;
                }
            }

            Index = 0;
        }
    }
}
