namespace Ryujinx.HLE.HOS.Services.Hid.Irs
{
    public enum ResultCode
    {
        ModuleId = 205,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidCameraHandle = (204 << ErrorCodeShift) | ModuleId,
        InvalidBufferSize = (207 << ErrorCodeShift) | ModuleId,
        HandlePointerIsNull = (212 << ErrorCodeShift) | ModuleId,
        NpadIdOutOfRange = (709 << ErrorCodeShift) | ModuleId,
    }
}
