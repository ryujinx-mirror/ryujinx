namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    readonly struct PointerAndSize
    {
        public static PointerAndSize Empty => new(0UL, 0UL);

        public ulong Address { get; }
        public ulong Size { get; }
        public bool IsEmpty => Size == 0UL;

        public PointerAndSize(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }
    }
}
