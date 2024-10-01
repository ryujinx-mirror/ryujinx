namespace Ryujinx.HLE.HOS.Services.Nv
{
    enum NvResult : uint
    {
        Success = 0,
        NotImplemented = 1,
        NotSupported = 2,
        NotInitialized = 3,
        InvalidParameter = 4,
        Timeout = 5,
        InsufficientMemory = 6,
        ReadOnlyAttribute = 7,
        InvalidState = 8,
        InvalidAddress = 9,
        InvalidSize = 10,
        InvalidValue = 11,
        AlreadyAllocated = 13,
        Busy = 14,
        ResourceError = 15,
        CountMismatch = 16,
        SharedMemoryTooSmall = 0x1000,
        FileOperationFailed = 0x30003,
        DirectoryOperationFailed = 0x30004,
        NotAvailableInProduction = 0x30006,
        IoctlFailed = 0x3000F,
        AccessDenied = 0x30010,
        FileNotFound = 0x30013,
        ModuleNotPresent = 0xA000E,
    }
}
