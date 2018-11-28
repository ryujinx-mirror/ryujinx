namespace Ryujinx.HLE.HOS.Kernel
{
    struct KPageNode
    {
        public ulong Address;
        public ulong PagesCount;

        public KPageNode(ulong Address, ulong PagesCount)
        {
            this.Address    = Address;
            this.PagesCount = PagesCount;
        }
    }
}