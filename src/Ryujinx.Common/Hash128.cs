using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Hash128 : IEquatable<Hash128>
    {
        public ulong Low;
        public ulong High;

        public Hash128(ulong low, ulong high)
        {
            Low = low;
            High = high;
        }

        public readonly override string ToString()
        {
            return $"{High:x16}{Low:x16}";
        }

        public static bool operator ==(Hash128 x, Hash128 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Hash128 x, Hash128 y)
        {
            return !x.Equals(y);
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Hash128 hash128 && Equals(hash128);
        }

        public readonly bool Equals(Hash128 cmpObj)
        {
            return Low == cmpObj.Low && High == cmpObj.High;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Low, High);
        }
    }
}
