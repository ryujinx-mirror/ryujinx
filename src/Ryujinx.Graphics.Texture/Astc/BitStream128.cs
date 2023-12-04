using Ryujinx.Common.Utilities;
using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Texture.Astc
{
    public struct BitStream128
    {
#pragma warning disable IDE0044 // Make field readonly
        private Buffer16 _data;
#pragma warning restore IDE0044
        public int BitsLeft { get; set; }

        public BitStream128(Buffer16 data)
        {
            _data = data;
            BitsLeft = 128;
        }

        public int ReadBits(int bitCount)
        {
            Debug.Assert(bitCount < 32);

            if (bitCount == 0)
            {
                return 0;
            }

            int mask = (1 << bitCount) - 1;
            int value = _data.As<int>() & mask;

            Span<ulong> span = _data.AsSpan<ulong>();

            ulong carry = span[1] << (64 - bitCount);
            span[0] = (span[0] >> bitCount) | carry;
            span[1] >>= bitCount;

            BitsLeft -= bitCount;

            return value;
        }

        public void WriteBits(int value, int bitCount)
        {
            Debug.Assert(bitCount < 32);

            if (bitCount == 0)
            {
                return;
            }

            ulong maskedValue = (uint)(value & ((1 << bitCount) - 1));

            Span<ulong> span = _data.AsSpan<ulong>();

            if (BitsLeft < 64)
            {
                ulong lowMask = maskedValue << BitsLeft;
                span[0] |= lowMask;
            }

            if (BitsLeft + bitCount > 64)
            {
                if (BitsLeft > 64)
                {
                    span[1] |= maskedValue << (BitsLeft - 64);
                }
                else
                {
                    span[1] |= maskedValue >> (64 - BitsLeft);
                }
            }

            BitsLeft += bitCount;
        }
    }
}
