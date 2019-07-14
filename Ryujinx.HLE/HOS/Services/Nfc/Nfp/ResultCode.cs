namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    public enum ResultCode
    {
        ModuleId       = 115,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceNotFound      = (64 << ErrorCodeShift) | ModuleId,
        DevicesBufferIsNull = (65 << ErrorCodeShift) | ModuleId
    }
}