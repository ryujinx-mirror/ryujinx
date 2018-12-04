namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryInfo
    {
        public ulong Address { get; }
        public ulong Size    { get; }

        public MemoryState      State      { get; }
        public MemoryPermission Permission { get; }
        public MemoryAttribute  Attribute  { get; }

        public int IpcRefCount    { get; }
        public int DeviceRefCount { get; }

        public KMemoryInfo(
            ulong            address,
            ulong            size,
            MemoryState      state,
            MemoryPermission permission,
            MemoryAttribute  attribute,
            int              ipcRefCount,
            int              deviceRefCount)
        {
            Address        = address;
            Size           = size;
            State          = state;
            Attribute      = attribute;
            Permission     = permission;
            IpcRefCount    = ipcRefCount;
            DeviceRefCount = deviceRefCount;
        }
    }
}