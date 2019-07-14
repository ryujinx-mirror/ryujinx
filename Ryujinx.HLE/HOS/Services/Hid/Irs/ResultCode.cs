namespace Ryujinx.HLE.HOS.Services.Hid.Irs
{
    public enum ResultCode
    {
        ModuleId       = 205,
        ErrorCodeShift = 9,

        Success = 0,

        HandlePointerIsNull = (212 << ErrorCodeShift) | ModuleId,
        NpadIdOutOfRange    = (709 << ErrorCodeShift) | ModuleId
    }
}