namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    struct KMemoryArrangeRegion
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public ulong EndAddr => Address + Size;

        public KMemoryArrangeRegion(ulong address, ulong size)
        {
            Address = address;
            Size    = size;
        }
    }
}