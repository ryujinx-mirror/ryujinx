namespace Ryujinx.Audio
{
    public enum ResultCode
    {
        ModuleId = 153,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceNotFound = (1 << ErrorCodeShift) | ModuleId,
        OperationFailed = (2 << ErrorCodeShift) | ModuleId,
        UnsupportedSampleRate = (3 << ErrorCodeShift) | ModuleId,
        WorkBufferTooSmall = (4 << ErrorCodeShift) | ModuleId,
        BufferRingFull = (8 << ErrorCodeShift) | ModuleId,
        UnsupportedChannelConfiguration = (10 << ErrorCodeShift) | ModuleId,
        InvalidUpdateInfo = (41 << ErrorCodeShift) | ModuleId,
        InvalidAddressInfo = (42 << ErrorCodeShift) | ModuleId,
        InvalidMixSorting = (43 << ErrorCodeShift) | ModuleId,
        UnsupportedOperation = (513 << ErrorCodeShift) | ModuleId,
        InvalidExecutionContextOperation = (514 << ErrorCodeShift) | ModuleId,
    }
}
