namespace Ryujinx.HLE.HOS.Services.Apm
{
    enum ResultCode
    {
        ModuleId = 148,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidParameters = (1 << ErrorCodeShift) | ModuleId,
    }
}
