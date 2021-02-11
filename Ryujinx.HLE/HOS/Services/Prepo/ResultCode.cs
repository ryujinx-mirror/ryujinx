namespace Ryujinx.HLE.HOS.Services.Prepo
{
    enum ResultCode
    {
        ModuleId       = 129,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument   = (1  << ErrorCodeShift) | ModuleId,
        InvalidState      = (5  << ErrorCodeShift) | ModuleId,
        InvalidBufferSize = (9  << ErrorCodeShift) | ModuleId,
        PermissionDenied  = (90 << ErrorCodeShift) | ModuleId
    }
}