namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryInfo
    {
        public ulong Address { get; }
        public ulong Size { get; }

        public MemoryState State { get; }
        public KMemoryPermission Permission { get; }
        public MemoryAttribute Attribute { get; }
        public KMemoryPermission SourcePermission { get; }

        public int IpcRefCount { get; }
        public int DeviceRefCount { get; }

        public KMemoryInfo(
            ulong address,
            ulong size,
            MemoryState state,
            KMemoryPermission permission,
            MemoryAttribute attribute,
            KMemoryPermission sourcePermission,
            int ipcRefCount,
            int deviceRefCount)
        {
            Address = address;
            Size = size;
            State = state;
            Permission = permission;
            Attribute = attribute;
            SourcePermission = sourcePermission;
            IpcRefCount = ipcRefCount;
            DeviceRefCount = deviceRefCount;
        }
    }
}
