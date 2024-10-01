using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    [Flags]
    enum MemoryAttribute : byte
    {
        None = 0,
        Mask = 0xff,

        Borrowed = 1 << 0,
        IpcMapped = 1 << 1,
        DeviceMapped = 1 << 2,
        Uncached = 1 << 3,
        PermissionLocked = 1 << 4,

        IpcAndDeviceMapped = IpcMapped | DeviceMapped,
        BorrowedAndIpcMapped = Borrowed | IpcMapped,
        DeviceMappedAndUncached = DeviceMapped | Uncached,
    }
}
