namespace Ryujinx.HLE.Utilities
{
    static class EndianSwap
    {
        public static ushort Swap16(ushort Value) => (ushort)(((Value >> 8) & 0xff) | (Value << 8));

        public static int Swap32(int Value)
        {
            uint UintVal = (uint)Value;

            return (int)(((UintVal >> 24) & 0x000000ff) |
                         ((UintVal >>  8) & 0x0000ff00) |
                         ((UintVal <<  8) & 0x00ff0000) |
                         ((UintVal << 24) & 0xff000000));
        }
    }
}
