using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    [Flags]
    enum MemoryState : uint
    {
        Unmapped = 0x0,
        Io = Mapped | 0x1,
        Normal = Mapped | QueryPhysicalAddressAllowed | 0x2,
        CodeStatic = ForceReadWritableByDebugSyscalls |
            IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            ProcessPermissionChangeAllowed |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            MapProcessAllowed |
            LinearMapped |
            0x3,
        CodeMutable = PermissionChangeAllowed |
            IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            MapAllowed |
            TransferMemoryAllowed |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IpcBufferAllowed |
            IsPoolAllocated |
            MapProcessAllowed |
            AttributeChangeAllowed |
            CodeMemoryAllowed |
            LinearMapped |
            PermissionLockAllowed |
            0x4,
        Heap = PermissionChangeAllowed |
            IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            MapAllowed |
            TransferMemoryAllowed |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IpcBufferAllowed |
            IsPoolAllocated |
            AttributeChangeAllowed |
            CodeMemoryAllowed |
            LinearMapped |
            0x5,
        SharedMemory = Mapped | IsPoolAllocated | LinearMapped | 0x6,
        ModCodeStatic = ForceReadWritableByDebugSyscalls |
            IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            ProcessPermissionChangeAllowed |
            UnmapProcessCodeMemoryAllowed |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            MapProcessAllowed |
            LinearMapped |
            0x8,
        ModCodeMutable = PermissionChangeAllowed |
            IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            MapAllowed |
            UnmapProcessCodeMemoryAllowed |
            TransferMemoryAllowed |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IpcBufferAllowed |
            IsPoolAllocated |
            MapProcessAllowed |
            AttributeChangeAllowed |
            CodeMemoryAllowed |
            LinearMapped |
            PermissionLockAllowed |
            0x9,
        IpcBuffer0 = IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            LinearMapped |
            0xA,
        Stack = IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            LinearMapped |
            0xB,
        ThreadLocal = Mapped | IsPoolAllocated | LinearMapped | 0xC,
        TransferMemoryIsolated = IpcSendAllowedType0 |
            IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            AttributeChangeAllowed |
            LinearMapped |
            0xD,
        TransferMemory = IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            LinearMapped |
            0xE,
        ProcessMemory = IpcSendAllowedType3 | IpcSendAllowedType1 | Mapped | IsPoolAllocated | LinearMapped | 0xF,
        Reserved = 0x10,
        IpcBuffer1 = IpcSendAllowedType3 |
            IpcSendAllowedType1 |
            Mapped |
            QueryPhysicalAddressAllowed |
            MapDeviceAllowed |
            MapDeviceAlignedAllowed |
            IsPoolAllocated |
            LinearMapped |
            0x11,
        IpcBuffer3 = IpcSendAllowedType3 | Mapped | QueryPhysicalAddressAllowed | MapDeviceAllowed | IsPoolAllocated | LinearMapped | 0x12,
        KernelStack = Mapped | 0x13,
        CodeReadOnly = ForceReadWritableByDebugSyscalls | Mapped | IsPoolAllocated | LinearMapped | 0x14,
        CodeWritable = Mapped | IsPoolAllocated | LinearMapped | 0x15,
        UserMask = 0xFF,
        Mask = 0xFFFFFFFF,

        PermissionChangeAllowed = 1 << 8,
        ForceReadWritableByDebugSyscalls = 1 << 9,
        IpcSendAllowedType0 = 1 << 10,
        IpcSendAllowedType3 = 1 << 11,
        IpcSendAllowedType1 = 1 << 12,
        Mapped = 1 << 13,
        ProcessPermissionChangeAllowed = 1 << 14,
        MapAllowed = 1 << 15,
        UnmapProcessCodeMemoryAllowed = 1 << 16,
        TransferMemoryAllowed = 1 << 17,
        QueryPhysicalAddressAllowed = 1 << 18,
        MapDeviceAllowed = 1 << 19,
        MapDeviceAlignedAllowed = 1 << 20,
        IpcBufferAllowed = 1 << 21,
        IsPoolAllocated = 1 << 22,
        MapProcessAllowed = 1 << 23,
        AttributeChangeAllowed = 1 << 24,
        CodeMemoryAllowed = 1 << 25,
        LinearMapped = 1 << 26,
        PermissionLockAllowed = 1 << 27,
    }
}
