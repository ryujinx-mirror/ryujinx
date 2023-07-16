namespace Ryujinx.HLE.HOS.Services.Pcv
{
    enum ResultCode
    {
        ModuleId = 30,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument = (5 << ErrorCodeShift) | ModuleId,
    }
}
