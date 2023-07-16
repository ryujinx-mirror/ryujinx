namespace Ryujinx.HLE.HOS.Services.Nifm
{
    enum ResultCode
    {
        ModuleId = 110,
        ErrorCodeShift = 9,

        Success = 0,

        Unknown112 = (112 << ErrorCodeShift) | ModuleId, // IRequest::GetResult
        Unknown180 = (180 << ErrorCodeShift) | ModuleId, // IRequest::GetAppletInfo
        NoInternetConnection = (300 << ErrorCodeShift) | ModuleId,
        ObjectIsNull = (350 << ErrorCodeShift) | ModuleId,
    }
}
