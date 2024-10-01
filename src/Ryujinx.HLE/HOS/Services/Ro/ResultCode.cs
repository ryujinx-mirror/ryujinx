namespace Ryujinx.HLE.HOS.Services.Ro
{
    enum ResultCode
    {
        ModuleId = 22,
        ErrorCodeShift = 9,

        Success = 0,

        InsufficientAddressSpace = (2 << ErrorCodeShift) | ModuleId,
        AlreadyLoaded = (3 << ErrorCodeShift) | ModuleId,
        InvalidNro = (4 << ErrorCodeShift) | ModuleId,
        InvalidNrr = (6 << ErrorCodeShift) | ModuleId,
        TooManyNro = (7 << ErrorCodeShift) | ModuleId,
        TooManyNrr = (8 << ErrorCodeShift) | ModuleId,
        NotAuthorized = (9 << ErrorCodeShift) | ModuleId,

        InvalidNrrType = (10 << ErrorCodeShift) | ModuleId,

        InvalidAddress = (1025 << ErrorCodeShift) | ModuleId,
        InvalidSize = (1026 << ErrorCodeShift) | ModuleId,
        NotLoaded = (1028 << ErrorCodeShift) | ModuleId,
        NotRegistered = (1029 << ErrorCodeShift) | ModuleId,
        InvalidSession = (1030 << ErrorCodeShift) | ModuleId,
        InvalidProcess = (1031 << ErrorCodeShift) | ModuleId,
    }
}
