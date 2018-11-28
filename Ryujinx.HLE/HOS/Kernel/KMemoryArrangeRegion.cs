namespace Ryujinx.HLE.HOS.Kernel
{
    struct KMemoryArrangeRegion
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public ulong EndAddr => Address + Size;

        public KMemoryArrangeRegion(ulong Address, ulong Size)
        {
            this.Address = Address;
            this.Size    = Size;
        }
    }
}