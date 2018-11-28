namespace Ryujinx.HLE.HOS.Kernel
{
    class KTransferMemory
    {
        public ulong Address { get; private set; }
        public ulong Size     { get; private set; }

        public KTransferMemory(ulong Address, ulong Size)
        {
            this.Address = Address;
            this.Size    = Size;
        }
    }
}