using System;

namespace Ryujinx.Common.Utilities
{
    public static class UInt128Utils
    {
        public static UInt128 FromHex(string hex)
        {
            return new UInt128((ulong)Convert.ToInt64(hex.Substring(0, 16), 16), (ulong)Convert.ToInt64(hex.Substring(16), 16));
        }

        public static UInt128 CreateRandom()
        {
            return new UInt128((ulong)Random.Shared.NextInt64(), (ulong)Random.Shared.NextInt64());
        }
    }
}