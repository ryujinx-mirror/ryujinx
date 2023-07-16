namespace Ryujinx.HLE.HOS.Services.Account
{
    enum ResultCode
    {
        ModuleId = 124,
        ErrorCodeShift = 9,

        Success = 0,

        NullArgument = (20 << ErrorCodeShift) | ModuleId,
        InvalidArgument = (22 << ErrorCodeShift) | ModuleId,
        NullInputBuffer = (30 << ErrorCodeShift) | ModuleId,
        InvalidBufferSize = (31 << ErrorCodeShift) | ModuleId,
        InvalidBuffer = (32 << ErrorCodeShift) | ModuleId,
        AsyncExecutionNotInitialized = (40 << ErrorCodeShift) | ModuleId,
        Unknown41 = (41 << ErrorCodeShift) | ModuleId,
        InternetRequestDenied = (59 << ErrorCodeShift) | ModuleId,
        UserNotFound = (100 << ErrorCodeShift) | ModuleId,
        NullObject = (302 << ErrorCodeShift) | ModuleId,
        Unknown341 = (341 << ErrorCodeShift) | ModuleId,
        MissingNetworkServiceLicenseKind = (400 << ErrorCodeShift) | ModuleId,
        InvalidIdTokenCacheBufferSize = (451 << ErrorCodeShift) | ModuleId,
    }
}
