namespace Ryujinx.HLE.HOS.Services.Olsc
{
    enum ResultCode
    {
        ModuleId = 179,
        ErrorCodeShift = 9,

        Success = 0,

        NullArgument = (100 << ErrorCodeShift) | ModuleId,
        NotInitialized = (101 << ErrorCodeShift) | ModuleId,
    }
}
