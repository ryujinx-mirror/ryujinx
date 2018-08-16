namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryInfo
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }

        public MemoryState      State      { get; private set; }
        public MemoryPermission Permission { get; private set; }
        public MemoryAttribute  Attribute  { get; private set; }

        public int IpcRefCount    { get; private set; }
        public int DeviceRefCount { get; private set; }

        public KMemoryInfo(
            long             Position,
            long             Size,
            MemoryState      State,
            MemoryPermission Permission,
            MemoryAttribute  Attribute,
            int              IpcRefCount,
            int              DeviceRefCount)
        {
            this.Position       = Position;
            this.Size           = Size;
            this.State          = State;
            this.Attribute      = Attribute;
            this.Permission     = Permission;
            this.IpcRefCount    = IpcRefCount;
            this.DeviceRefCount = DeviceRefCount;
        }
    }
}