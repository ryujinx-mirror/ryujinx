namespace Ryujinx.HLE.HOS.Services.Nifm
{
    enum ResultCode
    {
        ModuleId       = 110,
        ErrorCodeShift = 9,

        Success = 0,

        NoInternetConnection = (300 << ErrorCodeShift) | ModuleId,
        ObjectIsNull         = (350 << ErrorCodeShift) | ModuleId
    }
}