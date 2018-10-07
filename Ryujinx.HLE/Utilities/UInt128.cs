using Ryujinx.HLE.Utilities;
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

            byte[] Bytes = new byte[16];

            int Index = Bytes.Length;

            void WriteBytes(long Value)
            {
                for (int Byte = 0; Byte < 8; Byte++)
                {
                    Bytes[--Index] = (byte)(Value >> Byte * 8);
                }
            }

            WriteBytes(Low);
            WriteBytes(High);
        }

        public UInt128(string UInt128Hex)
        {
            if (UInt128Hex == null || UInt128Hex.Length != 32 || !UInt128Hex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid Hex value!", nameof(UInt128Hex));
            }

            Low  = Convert.ToInt64(UInt128Hex.Substring(16),16);
            High = Convert.ToInt64(UInt128Hex.Substring(0, 16), 16);
        }

        public void Write(BinaryWriter BinaryWriter)
        {
            BinaryWriter.Write(High);
            BinaryWriter.Write(Low);
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