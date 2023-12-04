namespace Ryujinx.HLE.HOS.Services.Caps
{
    enum ResultCode
    {
        ModuleId = 206,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument = (2 << ErrorCodeShift) | ModuleId,
        ShimLibraryVersionAlreadySet = (7 << ErrorCodeShift) | ModuleId,
        OutOfRange = (8 << ErrorCodeShift) | ModuleId,
        InvalidContentType = (14 << ErrorCodeShift) | ModuleId,
        NullOutputBuffer = (141 << ErrorCodeShift) | ModuleId,
        NullInputBuffer = (142 << ErrorCodeShift) | ModuleId,
        BlacklistedPid = (822 << ErrorCodeShift) | ModuleId,
    }
}
