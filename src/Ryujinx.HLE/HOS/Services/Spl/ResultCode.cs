namespace Ryujinx.HLE.HOS.Services.Spl
{
    enum ResultCode
    {
        ModuleId = 26,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArguments = (101 << ErrorCodeShift) | ModuleId,
    }
}
