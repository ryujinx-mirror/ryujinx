namespace Ryujinx.HLE.HOS.Services.Time
{
    public enum ResultCode
    {
        ModuleId       = 116,
        ErrorCodeShift = 9,

        Success = 0,

        TimeNotFound             = (200 << ErrorCodeShift) | ModuleId,
        Overflow                 = (201 << ErrorCodeShift) | ModuleId,
        LocationNameTooLong      = (801 << ErrorCodeShift) | ModuleId,
        OutOfRange               = (902 << ErrorCodeShift) | ModuleId,
        TimeZoneConversionFailed = (903 << ErrorCodeShift) | ModuleId,
        TimeZoneNotFound         = (989 << ErrorCodeShift) | ModuleId
    }
}