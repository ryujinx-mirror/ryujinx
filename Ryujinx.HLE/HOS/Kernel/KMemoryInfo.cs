namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryInfo
    {
        public ulong Address { get; private set; }
        public ulong Size    { get; private set; }

        public MemoryState      State      { get; private set; }
        public MemoryPermission Permission { get; private set; }
        public MemoryAttribute  Attribute  { get; private set; }

        public int IpcRefCount    { get; private set; }
        public int DeviceRefCount { get; private set; }

        public KMemoryInfo(
            ulong            Address,
            ulong            Size,
            MemoryState      State,
            MemoryPermission Permission,
            MemoryAttribute  Attribute,
            int              IpcRefCount,
            int              DeviceRefCount)
        {
            this.Address        = Address;
            this.Size           = Size;
            this.State          = State;
            this.Attribute      = Attribute;
            this.Permission     = Permission;
            this.IpcRefCount    = IpcRefCount;
            this.DeviceRefCount = DeviceRefCount;
        }
    }
}