using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryBlock
    {
        public ulong BaseAddress { get; private set; }
        public ulong PagesCount  { get; private set; }

        public MemoryState      State            { get; private set; }
        public KMemoryPermission Permission       { get; private set; }
        public MemoryAttribute  Attribute        { get; private set; }
        public KMemoryPermission SourcePermission { get; private set; }

        public int IpcRefCount    { get; private set; }
        public int DeviceRefCount { get; private set; }

        public KMemoryBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      state,
            KMemoryPermission permission,
            MemoryAttribute  attribute,
            int              ipcRefCount    = 0,
            int              deviceRefCount = 0)
        {
            BaseAddress    = baseAddress;
            PagesCount     = pagesCount;
            State          = state;
            Attribute      = attribute;
            Permission     = permission;
            IpcRefCount    = ipcRefCount;
            DeviceRefCount = deviceRefCount;
        }

        public void SetState(KMemoryPermission permission, MemoryState state, MemoryAttribute attribute)
        {
            Permission = permission;
            State      = state;
            Attribute &= MemoryAttribute.IpcAndDeviceMapped;
            Attribute |= attribute;
        }

        public void SetIpcMappingPermission(KMemoryPermission newPermission)
        {
            int oldIpcRefCount = IpcRefCount++;

            if ((ushort)IpcRefCount == 0)
            {
                throw new InvalidOperationException("IPC reference count increment overflowed.");
            }

            if (oldIpcRefCount == 0)
            {
                SourcePermission = Permission;

                Permission &= ~KMemoryPermission.ReadAndWrite;
                Permission |=  KMemoryPermission.ReadAndWrite & newPermission;
            }

            Attribute |= MemoryAttribute.IpcMapped;
        }

        public void RestoreIpcMappingPermission()
        {
            int oldIpcRefCount = IpcRefCount--;

            if (oldIpcRefCount == 0)
            {
                throw new InvalidOperationException("IPC reference count decrement underflowed.");
            }

            if (oldIpcRefCount == 1)
            {
                Permission = SourcePermission;

                SourcePermission = KMemoryPermission.None;

                Attribute &= ~MemoryAttribute.IpcMapped;
            }
        }

        public KMemoryBlock SplitRightAtAddress(ulong address)
        {
            ulong leftAddress = BaseAddress;

            ulong leftPagesCount = (address - leftAddress) / KMemoryManager.PageSize;

            BaseAddress = address;

            PagesCount -= leftPagesCount;

            return new KMemoryBlock(
                leftAddress,
                leftPagesCount,
                State,
                Permission,
                Attribute,
                IpcRefCount,
                DeviceRefCount);
        }

        public void AddPages(ulong pagesCount)
        {
            PagesCount += pagesCount;
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
                SourcePermission,
                IpcRefCount,
                DeviceRefCount);
        }
    }
}