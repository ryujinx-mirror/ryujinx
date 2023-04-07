namespace Ryujinx.HLE.HOS.Services.Friend
{
    enum ResultCode
    {
        ModuleId       = 121,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument        = (2  << ErrorCodeShift) | ModuleId,
        InternetRequestDenied  = (6  << ErrorCodeShift) | ModuleId,
        NotificationQueueEmpty = (15 << ErrorCodeShift) | ModuleId
    }
}
