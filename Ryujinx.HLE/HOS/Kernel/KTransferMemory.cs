namespace Ryujinx.HLE.HOS.Kernel
{
    class KTransferMemory
    {
        public ulong Address { get; }
        public ulong Size    { get; }

        public KTransferMemory(ulong address, ulong size)
        {
            Address = address;
            Size    = size;
        }
    }
}