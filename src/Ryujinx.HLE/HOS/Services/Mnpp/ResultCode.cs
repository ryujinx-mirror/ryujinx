namespace Ryujinx.HLE.HOS.Services.Mnpp
{
    enum ResultCode
    {
        ModuleId = 239,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument = (100 << ErrorCodeShift) | ModuleId,
        InvalidBufferSize = (101 << ErrorCodeShift) | ModuleId,
    }
}
