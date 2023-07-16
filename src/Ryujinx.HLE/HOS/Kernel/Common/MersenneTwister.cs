using System.Numerics;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    class MersenneTwister
    {
        private int _index;
        private readonly uint[] _mt;

        public MersenneTwister(uint seed)
        {
            _mt = new uint[624];

            _mt[0] = seed;

            for (int mtIdx = 1; mtIdx < _mt.Length; mtIdx++)
            {
                uint prev = _mt[mtIdx - 1];

                _mt[mtIdx] = (uint)(0x6c078965 * (prev ^ (prev >> 30)) + mtIdx);
            }

            _index = _mt.Length;
        }

        public long GenRandomNumber(long min, long max)
        {
            long range = max - min;

            if (min == max)
            {
                return min;
            }

            if (range == -1)
            {
                // Increment would cause a overflow, special case.
                return GenRandomNumber(2, 2, 32, 0xffffffffu, 0xffffffffu);
            }

            range++;

            // This is log2(Range) plus one.
            int nextRangeLog2 = 64 - BitOperations.LeadingZeroCount((ulong)range);

            // If Range is already power of 2, subtract one to use log2(Range) directly.
            int rangeLog2 = nextRangeLog2 - (BitOperations.IsPow2(range) ? 1 : 0);

            int parts = rangeLog2 > 32 ? 2 : 1;
            int bitsPerPart = rangeLog2 / parts;

            int fullParts = parts - (rangeLog2 - parts * bitsPerPart);

            uint mask = 0xffffffffu >> (32 - bitsPerPart);
            uint maskPlus1 = 0xffffffffu >> (31 - bitsPerPart);

            long randomNumber;

            do
            {
                randomNumber = GenRandomNumber(parts, fullParts, bitsPerPart, mask, maskPlus1);
            }
            while ((ulong)randomNumber >= (ulong)range);

            return min + randomNumber;
        }

        private long GenRandomNumber(
            int parts,
            int fullParts,
            int bitsPerPart,
            uint mask,
            uint maskPlus1)
        {
            long randomNumber = 0;

            int part = 0;

            for (; part < fullParts; part++)
            {
                randomNumber <<= bitsPerPart;
                randomNumber |= GenRandomNumber() & mask;
            }

            for (; part < parts; part++)
            {
                randomNumber <<= bitsPerPart + 1;
                randomNumber |= GenRandomNumber() & maskPlus1;
            }

            return randomNumber;
        }

        private uint GenRandomNumber()
        {
            if (_index >= _mt.Length)
            {
                Twist();
            }

            uint value = _mt[_index++];

            value ^= value >> 11;
            value ^= (value << 7) & 0x9d2c5680;
            value ^= (value << 15) & 0xefc60000;
            value ^= value >> 18;

            return value;
        }

        private void Twist()
        {
            for (int mtIdx = 0; mtIdx < _mt.Length; mtIdx++)
            {
                uint value = (_mt[mtIdx] & 0x80000000) + (_mt[(mtIdx + 1) % _mt.Length] & 0x7fffffff);

                _mt[mtIdx] = _mt[(mtIdx + 397) % _mt.Length] ^ (value >> 1);

                if ((value & 1) != 0)
                {
                    _mt[mtIdx] ^= 0x9908b0df;
                }
            }

            _index = 0;
        }
    }
}
