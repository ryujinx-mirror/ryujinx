using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Account
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 0x8)]
    public readonly record struct Uid
    {
        public readonly ulong High;
        public readonly ulong Low;

        public bool IsNull => (Low | High) == 0;

        public static Uid Null => new(0, 0);

        public Uid(ulong low, ulong high)
        {
            Low = low;
            High = high;
        }

        public Uid(byte[] bytes)
        {
            High = BitConverter.ToUInt64(bytes, 0);
            Low = BitConverter.ToUInt64(bytes, 8);
        }

        public Uid(string hex)
        {
            if (hex == null || hex.Length != 32 || !hex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid Hex value!", nameof(hex));
            }

            Low = Convert.ToUInt64(hex[16..], 16);
            High = Convert.ToUInt64(hex[..16], 16);
        }

        public void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(High);
            binaryWriter.Write(Low);
        }

        public override string ToString()
        {
            return High.ToString("x16") + Low.ToString("x16");
        }

        public LibHac.Account.Uid ToLibHacUid()
        {
            return new LibHac.Account.Uid((ulong)High, (ulong)Low);
        }

        public UInt128 ToUInt128()
        {
            return new UInt128((ulong)High, (ulong)Low);
        }
    }
}
