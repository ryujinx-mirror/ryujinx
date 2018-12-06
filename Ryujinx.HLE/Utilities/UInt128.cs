using System;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.Utilities
{
    public struct UInt128
    {
        public long High { get; private set; }
        public long Low  { get; private set; }

        public UInt128(long low, long high)
        {
            Low  = low;
            High = high;
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

        public bool IsZero()
        {
            return (Low | High) == 0;
        }
    }
}
