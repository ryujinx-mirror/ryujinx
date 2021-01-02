using LibHac.Account;
using Ryujinx.HLE.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UserId : IEquatable<UserId>
    {
        public readonly long High;
        public readonly long Low;

        public bool IsNull => (Low | High) == 0;

        public static UserId Null => new UserId(0, 0);

        public UserId(long low, long high)
        {
            Low  = low;
            High = high;
        }

        public UserId(byte[] bytes)
        {
            High = BitConverter.ToInt64(bytes, 0);
            Low  = BitConverter.ToInt64(bytes, 8);
        }

        public UserId(string hex)
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
            binaryWriter.Write(High);
            binaryWriter.Write(Low);
        }

        public override string ToString()
        {
            return High.ToString("x16") + Low.ToString("x16");
        }

        public static bool operator ==(UserId x, UserId y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(UserId x, UserId y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is UserId userId && Equals(userId);
        }

        public bool Equals(UserId cmpObj)
        {
            return Low == cmpObj.Low && High == cmpObj.High;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Low, High);
        }

        public readonly Uid ToLibHacUid()
        {
            return new Uid((ulong)High, (ulong)Low);
        }

        public readonly UInt128 ToUInt128()
        {
            return new UInt128(Low, High);
        }
    }
}