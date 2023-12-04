using LibHac.Account;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct UserId
    {
        public readonly long High;
        public readonly long Low;

        public bool IsNull => (Low | High) == 0;

        public static UserId Null => new(0, 0);

        public UserId(long low, long high)
        {
            Low = low;
            High = high;
        }

        public UserId(byte[] bytes)
        {
            High = BitConverter.ToInt64(bytes, 0);
            Low = BitConverter.ToInt64(bytes, 8);
        }

        public UserId(string hex)
        {
            if (hex == null || hex.Length != 32 || !hex.All("0123456789abcdefABCDEF".Contains))
            {
                throw new ArgumentException("Invalid Hex value!", nameof(hex));
            }

            Low = long.Parse(hex.AsSpan(16), NumberStyles.HexNumber);
            High = long.Parse(hex.AsSpan(0, 16), NumberStyles.HexNumber);
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

        public Uid ToLibHacUid()
        {
            return new Uid((ulong)High, (ulong)Low);
        }

        public UInt128 ToUInt128()
        {
            return new UInt128((ulong)High, (ulong)Low);
        }
    }
}
