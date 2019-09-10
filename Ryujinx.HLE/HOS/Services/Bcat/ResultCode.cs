namespace Ryujinx.HLE.HOS.Services.Bcat
{
    enum ResultCode
    {
        ModuleId       = 122,
        ErrorCodeShift = 9,

        Success = 0,

        NullArgument = (2  << ErrorCodeShift) | ModuleId,
        NullSaveData = (31 << ErrorCodeShift) | ModuleId,
        NullObject   = (91 << ErrorCodeShift) | ModuleId
    }
}