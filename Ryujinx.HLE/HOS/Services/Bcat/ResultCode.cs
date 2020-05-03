namespace Ryujinx.HLE.HOS.Services.Bcat
{
    enum ResultCode
    {
        ModuleId       = 122,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument                   = (1 << ErrorCodeShift) | ModuleId,
        NotFound                          = (2 << ErrorCodeShift) | ModuleId,
        TargetLocked                      = (3 << ErrorCodeShift) | ModuleId,
        TargetAlreadyMounted              = (4 << ErrorCodeShift) | ModuleId,
        TargetNotMounted                  = (5 << ErrorCodeShift) | ModuleId,
        AlreadyOpen                       = (6 << ErrorCodeShift) | ModuleId,
        NotOpen                           = (7 << ErrorCodeShift) | ModuleId,
        InternetRequestDenied             = (8 << ErrorCodeShift) | ModuleId,
        ServiceOpenLimitReached           = (9 << ErrorCodeShift) | ModuleId,
        SaveDataNotFound                  = (10 << ErrorCodeShift) | ModuleId,
        NetworkServiceAccountNotAvailable = (31 << ErrorCodeShift) | ModuleId,
        PassphrasePathNotFound            = (80 << ErrorCodeShift) | ModuleId,
        DataVerificationFailed            = (81 << ErrorCodeShift) | ModuleId,
        PermissionDenied                  = (90 << ErrorCodeShift) | ModuleId,
        AllocationFailed                  = (91 << ErrorCodeShift) | ModuleId,
        InvalidOperation                  = (98 << ErrorCodeShift) | ModuleId,
        InvalidDeliveryCacheStorageFile   = (204 << ErrorCodeShift) | ModuleId,
        StorageOpenLimitReached           = (205 << ErrorCodeShift) | ModuleId
    }
}
