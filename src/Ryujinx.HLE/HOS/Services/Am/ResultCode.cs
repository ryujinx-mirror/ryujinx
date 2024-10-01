namespace Ryujinx.HLE.HOS.Services.Am
{
    enum ResultCode
    {
        ModuleId = 128,
        ErrorCodeShift = 9,

        Success = 0,

        NotAvailable = (2 << ErrorCodeShift) | ModuleId,
        NoMessages = (3 << ErrorCodeShift) | ModuleId,
        AppletLaunchFailed = (35 << ErrorCodeShift) | ModuleId,
        TitleIdNotFound = (37 << ErrorCodeShift) | ModuleId,
        ObjectInvalid = (500 << ErrorCodeShift) | ModuleId,
        IStorageInUse = (502 << ErrorCodeShift) | ModuleId,
        OutOfBounds = (503 << ErrorCodeShift) | ModuleId,
        BufferNotAcquired = (504 << ErrorCodeShift) | ModuleId,
        BufferAlreadyAcquired = (505 << ErrorCodeShift) | ModuleId,
        InvalidParameters = (506 << ErrorCodeShift) | ModuleId,
        OpenedAsWrongType = (511 << ErrorCodeShift) | ModuleId,
        UnbalancedFatalSection = (512 << ErrorCodeShift) | ModuleId,
        NullObject = (518 << ErrorCodeShift) | ModuleId,
        MemoryAllocationFailed = (600 << ErrorCodeShift) | ModuleId,
        StackPoolExhausted = (712 << ErrorCodeShift) | ModuleId,
        DebugModeNotEnabled = (974 << ErrorCodeShift) | ModuleId,
        DevFunctionNotEnabled = (980 << ErrorCodeShift) | ModuleId,
        NotImplemented = (998 << ErrorCodeShift) | ModuleId,
        Stubbed = (999 << ErrorCodeShift) | ModuleId,
    }
}
