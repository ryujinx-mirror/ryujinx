namespace Ryujinx.HLE.HOS.Services.Hid
{
    enum ResultCode
    {
        ModuleId       = 202,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidNpadIdType = (710 << ErrorCodeShift) | ModuleId
    }
} 