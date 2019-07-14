namespace Ryujinx.HLE.HOS.Services.Lr
{
    enum ResultCode
    {
        ModuleId       = 8,
        ErrorCodeShift = 9,

        Success = 0,

        ProgramLocationEntryNotFound = (2 << ErrorCodeShift) | ModuleId,
        AccessDenied                 = (5 << ErrorCodeShift) | ModuleId
    }
}