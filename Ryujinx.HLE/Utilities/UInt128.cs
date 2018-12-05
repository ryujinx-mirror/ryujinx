using System;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.Utilities
{
    public struct UInt128
    {
        public long High { get; private set; }
        public long Low  { get; private set; }

        public UInt128(long Low, long High)
        {
            this.Low  = Low;
            this.High = High;
        }

        public UInt128(string UInt128Hex)
        {
            if (UInt128Hex == null || UInt128Hex.Length != 32 || !UInt128Hex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid Hex value!", nameof(UInt128Hex));
            }

            Low  = Convert.ToInt64(UInt128Hex.Substring(16), 16);
            High = Convert.ToInt64(UInt128Hex.Substring(0, 16), 16);
        }

        public void Write(BinaryWriter BinaryWriter)
        {
            BinaryWriter.Write(Low);
            BinaryWriter.Write(High);
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
