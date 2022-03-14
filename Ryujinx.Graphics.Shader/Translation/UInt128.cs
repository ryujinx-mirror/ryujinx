using System;
using System.Numerics;

namespace Ryujinx.Graphics.Shader.Translation
{
    struct UInt128 : IEquatable<UInt128>
    {
        public static UInt128 Zero => new UInt128() { _v0 = 0, _v1 = 0 };

        private ulong _v0;
        private ulong _v1;

        public UInt128(ulong low, ulong high)
        {
            _v0 = low;
            _v1 = high;
        }

        public int TrailingZeroCount()
        {
            int count = BitOperations.TrailingZeroCount(_v0);
            if (count == 64)
            {
                count += BitOperations.TrailingZeroCount(_v1);
            }

            return count;
        }

        public static UInt128 Pow2(int x)
        {
            if (x >= 64)
            {
                return new UInt128(0, 1UL << (x - 64));
            }

            return new UInt128(1UL << x, 0);
        }

        public static UInt128 operator ~(UInt128 x)
        {
            return new UInt128(~x._v0, ~x._v1);
        }

        public static UInt128 operator &(UInt128 x, UInt128 y)
        {
            return new UInt128(x._v0 & y._v0, x._v1 & y._v1);
        }

        public static UInt128 operator |(UInt128 x, UInt128 y)
        {
            return new UInt128(x._v0 | y._v0, x._v1 | y._v1);
        }

        public static UInt128 operator <<(UInt128 x, int shift)
        {
            if (shift == 0)
            {
                return new UInt128(x._v0, x._v1);
            }
            else if (shift >= 64)
            {
                return new UInt128(0, x._v0 << (shift - 64));
            }

            ulong shiftOut = x._v0 >> (64 - shift);

            return new UInt128(x._v0 << shift, (x._v1 << shift) | shiftOut);
        }

        public static UInt128 operator >>(UInt128 x, int shift)
        {
            if (shift == 0)
            {
                return new UInt128(x._v0, x._v1);
            }
            else if (shift >= 64)
            {
                return new UInt128(x._v1 >> (shift - 64), 0);
            }

            ulong shiftOut = x._v1 & ((1UL << shift) - 1);

            return new UInt128((x._v0 >> shift) | (shiftOut << (64 - shift)), x._v1 >> shift);
        }

        public static bool operator ==(UInt128 x, UInt128 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(UInt128 x, UInt128 y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is UInt128 other && Equals(other);
        }

        public bool Equals(UInt128 other)
        {
            return _v0 == other._v0 && _v1 == other._v1;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_v0, _v1);
        }
    }
}