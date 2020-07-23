namespace Ryujinx.HLE.HOS.Services.Vi
{
    enum ResultCode
    {
        ModuleId       = 114,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArguments   = (1 << ErrorCodeShift) | ModuleId,
        InvalidLayerSize   = (4 << ErrorCodeShift) | ModuleId,
        InvalidScalingMode = (6 << ErrorCodeShift) | ModuleId
    }
}