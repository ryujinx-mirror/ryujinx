namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm
{
    enum ResultCode
    {
        ModuleId = 178,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidUserID = (100 << ErrorCodeShift) | ModuleId,
        UserNotFound = (101 << ErrorCodeShift) | ModuleId,
        ServiceUnavailable = (150 << ErrorCodeShift) | ModuleId,
        FileStorageFailure = (200 << ErrorCodeShift) | ModuleId,
    }
}
