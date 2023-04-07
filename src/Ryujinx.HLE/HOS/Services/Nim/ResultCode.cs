namespace Ryujinx.HLE.HOS.Services.Nim
{
    enum ResultCode
    {
        ModuleId       = 137,
        ErrorCodeShift = 9,

        Success = 0,

        NullArgument = (90 << ErrorCodeShift) | ModuleId
    }
}