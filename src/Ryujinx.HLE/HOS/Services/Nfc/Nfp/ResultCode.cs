namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    public enum ResultCode
    {
        ModuleId = 115,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceNotFound = (64 << ErrorCodeShift) | ModuleId,
        WrongArgument = (65 << ErrorCodeShift) | ModuleId,
        WrongDeviceState = (73 << ErrorCodeShift) | ModuleId,
        NfcDisabled = (80 << ErrorCodeShift) | ModuleId,
        TagNotFound = (97 << ErrorCodeShift) | ModuleId,
        ApplicationAreaIsNull = (128 << ErrorCodeShift) | ModuleId,
        ApplicationAreaAlreadyCreated = (168 << ErrorCodeShift) | ModuleId,
    }
}
