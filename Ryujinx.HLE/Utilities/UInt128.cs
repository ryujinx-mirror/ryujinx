using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt128 : IEquatable<UInt128>
    {
        public readonly long Low;
        public readonly long High;

        public bool IsNull => (Low | High) == 0;

        public UInt128(long low, long high)
        {
            Low  = low;
            High = high;
        }

        public UInt128(byte[] bytes)
        {
            Low  = BitConverter.ToInt64(bytes, 0);
            High = BitConverter.ToInt64(bytes, 8);
        }

        public UInt128(string hex)
        {
            if (hex == null || hex.Length != 32 || !hex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid Hex value!", nameof(hex));
            }

            Low  = Convert.ToInt64(hex.Substring(16), 16);
            High = Convert.ToInt64(hex.Substring(0, 16), 16);
        }

        public void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Low);
            binaryWriter.Write(High);
        }

        public override string ToString()
        {
            return High.ToString("x16") + Low.ToString("x16");
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
            return obj is UInt128 uint128 && Equals(uint128);
        }

        public bool Equals(UInt128 cmpObj)
        {
            return Low == cmpObj.Low && High == cmpObj.High;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Low, High);
        }
    }
}