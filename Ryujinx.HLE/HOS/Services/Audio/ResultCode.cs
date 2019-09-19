namespace Ryujinx.HLE.HOS.Services.Audio
{
    enum ResultCode
    {
        ModuleId       = 153,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceNotFound        = (1 << ErrorCodeShift) | ModuleId,
        UnsupportedRevision   = (2 << ErrorCodeShift) | ModuleId,
        UnsupportedSampleRate = (3 << ErrorCodeShift) | ModuleId,
        OpusInvalidInput      = (6 << ErrorCodeShift) | ModuleId
    }
}