using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    [Flags]
    enum MemoryState : uint
    {
        Unmapped = 0x00000000,
        Io = 0x00002001,
        Normal = 0x00042002,
        CodeStatic = 0x00DC7E03,
        CodeMutable = 0x03FEBD04,
        Heap = 0x037EBD05,
        SharedMemory = 0x00402006,
        ModCodeStatic = 0x00DD7E08,
        ModCodeMutable = 0x03FFBD09,
        IpcBuffer0 = 0x005C3C0A,
        Stack = 0x005C3C0B,
        ThreadLocal = 0x0040200C,
        TransferMemoryIsolated = 0x015C3C0D,
        TransferMemory = 0x005C380E,
        ProcessMemory = 0x0040380F,
        Reserved = 0x00000010,
        IpcBuffer1 = 0x005C3811,
        IpcBuffer3 = 0x004C2812,
        KernelStack = 0x00002013,
        CodeReadOnly = 0x00402214,
        CodeWritable = 0x00402015,
        UserMask = 0xff,
        Mask = 0xffffffff,

        PermissionChangeAllowed = 1 << 8,
        ForceReadWritableByDebugSyscalls = 1 << 9,
        IpcSendAllowedType0 = 1 << 10,
        IpcSendAllowedType3 = 1 << 11,
        IpcSendAllowedType1 = 1 << 12,
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
    }
}
