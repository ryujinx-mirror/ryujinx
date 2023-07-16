namespace Ryujinx.HLE.HOS.Services.Sm
{
    enum ResultCode
    {
        ModuleId = 21,
        ErrorCodeShift = 9,

        Success = 0,

        NotInitialized = (2 << ErrorCodeShift) | ModuleId,
        AlreadyRegistered = (4 << ErrorCodeShift) | ModuleId,
        InvalidName = (6 << ErrorCodeShift) | ModuleId,
        NotRegistered = (7 << ErrorCodeShift) | ModuleId,
    }
}
