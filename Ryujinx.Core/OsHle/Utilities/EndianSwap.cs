namespace Ryujinx.Core.OsHle.Utilities
{
    static class EndianSwap
    {
        public static short Swap16(short Value) => (short)(((Value >> 8) & 0xff) | (Value << 8));
    }
}
