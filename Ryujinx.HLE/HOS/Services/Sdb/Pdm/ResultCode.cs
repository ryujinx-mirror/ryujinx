namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm
{
    enum ResultCode
    {
        ModuleId       = 178,
        ErrorCodeShift = 9,

        Success = 0,

        UserNotFound       = (101 << ErrorCodeShift) | ModuleId,
        ServiceUnavailable = (150 << ErrorCodeShift) | ModuleId
    }
}