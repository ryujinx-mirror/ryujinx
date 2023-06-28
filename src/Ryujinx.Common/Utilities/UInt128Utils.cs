using System;
using System.Globalization;

namespace Ryujinx.Common.Utilities
{
    public static class UInt128Utils
    {
        public static UInt128 FromHex(string hex)
        {
            return new UInt128(ulong.Parse(hex.AsSpan(0, 16), NumberStyles.HexNumber), ulong.Parse(hex.AsSpan(16), NumberStyles.HexNumber));
        }

        public static UInt128 CreateRandom()
        {
            return new UInt128((ulong)Random.Shared.NextInt64(), (ulong)Random.Shared.NextInt64());
        }
    }
}
