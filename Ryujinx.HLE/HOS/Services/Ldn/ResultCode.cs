namespace Ryujinx.HLE.HOS.Services.Ldn
{
    enum ResultCode
    {
        ModuleId       = 203,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceDisabled  = (22 << ErrorCodeShift) | ModuleId,
        InvalidState    = (32 << ErrorCodeShift) | ModuleId,
        Unknown1        = (48 << ErrorCodeShift) | ModuleId,
        InvalidArgument = (96 << ErrorCodeShift) | ModuleId,
        InvalidOjbect   = (97 << ErrorCodeShift) | ModuleId,
    }
}