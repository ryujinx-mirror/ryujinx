namespace Ryujinx.OsHle
{
    enum MemoryType
    {
        Unmapped               = 0,
        Io                     = 1,
        Normal                 = 2,
        CodeStatic             = 3,
        CodeMutable            = 4,
        Heap                   = 5,
        SharedMemory           = 6,
        ModCodeStatic          = 8,
        ModCodeMutable         = 9,
        IpcBuffer0             = 10,
        MappedMemory           = 11,
        ThreadLocal            = 12,
        TransferMemoryIsolated = 13,
        TransferMemory         = 14,
        ProcessMemory          = 15,
        Reserved               = 16,
        IpcBuffer1             = 17,
        IpcBuffer3             = 18,
        KernelStack            = 19
    }
}