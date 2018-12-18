namespace Ryujinx.HLE.HOS.Kernel.Memory
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
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      state,
            MemoryPermission permission,
            MemoryAttribute  attribute)
        {
            BaseAddress = baseAddress;
            PagesCount  = pagesCount;
            State       = state;
            Attribute   = attribute;
            Permission  = permission;
        }

        public KMemoryInfo GetInfo()
        {
            ulong size = PagesCount * KMemoryManager.PageSize;

            return new KMemoryInfo(
                BaseAddress,
                size,
                State,
                Permission,
                Attribute,
                IpcRefCount,
                DeviceRefCount);
        }
    }
}