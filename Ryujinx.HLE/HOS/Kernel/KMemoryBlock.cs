namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryBlock
    {
        public ulong BaseAddress { get; set; }
        public ulong PagesCount  { get; set; }

        public MemoryState      State      { get; set; }
        public MemoryPermission Permission { get; set; }
        public MemoryAttribute  Attribute  { get; set; }

        public int IpcRefCount    { get; set; }
        public int DeviceRefCount { get; set; }

        public KMemoryBlock(
            ulong            BaseAddress,
            ulong            PagesCount,
            MemoryState      State,
            MemoryPermission Permission,
            MemoryAttribute  Attribute)
        {
            this.BaseAddress = BaseAddress;
            this.PagesCount  = PagesCount;
            this.State       = State;
            this.Attribute   = Attribute;
            this.Permission  = Permission;
        }

        public KMemoryInfo GetInfo()
        {
            ulong Size = PagesCount * KMemoryManager.PageSize;

            return new KMemoryInfo(
                BaseAddress,
                Size,
                State,
                Permission,
                Attribute,
                IpcRefCount,
                DeviceRefCount);
        }
    }
}