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
        BufferSizeTooSmall    = (4 << ErrorCodeShift) | ModuleId,
        OpusInvalidInput      = (6 << ErrorCodeShift) | ModuleId,
        TooManyBuffersInUse   = (8 << ErrorCodeShift) | ModuleId,
        InvalidChannelCount   = (10 << ErrorCodeShift) | ModuleId,
        InvalidOperation      = (513 << ErrorCodeShift) | ModuleId,
        InvalidHandle         = (1536 << ErrorCodeShift) | ModuleId,
        OutputAlreadyStarted  = (1540 << ErrorCodeShift) | ModuleId
    }
}
