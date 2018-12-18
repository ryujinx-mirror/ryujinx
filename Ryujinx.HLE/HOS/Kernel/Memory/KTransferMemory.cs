namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KTransferMemory
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public KTransferMemory(ulong address, ulong size)
        {
            Address = address;
            Size    = size;
        }
    }
}