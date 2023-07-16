namespace Ryujinx.HLE.HOS.Services.Fs
{
    enum ResultCode
    {
        ModuleId = 2,
        ErrorCodeShift = 9,

        Success = 0,

        PathDoesNotExist = (1 << ErrorCodeShift) | ModuleId,
        PathAlreadyExists = (2 << ErrorCodeShift) | ModuleId,
        PathAlreadyInUse = (7 << ErrorCodeShift) | ModuleId,
        PartitionNotFound = (1001 << ErrorCodeShift) | ModuleId,
        InvalidInput = (6001 << ErrorCodeShift) | ModuleId,
    }
}
