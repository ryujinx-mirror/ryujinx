namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryBlock
    {
        public long BasePosition { get; set; }
        public long PagesCount   { get; set; }

        public MemoryState      State      { get; set; }
        public MemoryPermission Permission { get; set; }
        public MemoryAttribute  Attribute  { get; set; }

        public int IpcRefCount    { get; set; }
        public int DeviceRefCount { get; set; }

        public KMemoryBlock(
            long             BasePosition,
            long             PagesCount,
            MemoryState      State,
            MemoryPermission Permission,
            MemoryAttribute  Attribute)
        {
            this.BasePosition = BasePosition;
            this.PagesCount   = PagesCount;
            this.State        = State;
            this.Attribute    = Attribute;
            this.Permission   = Permission;
        }

        public KMemoryInfo GetInfo()
        {
            long Size = PagesCount * KMemoryManager.PageSize;

            return new KMemoryInfo(
                BasePosition,
                Size,
                State,
                Permission,
                Attribute,
                IpcRefCount,
                DeviceRefCount);
        }
    }
}