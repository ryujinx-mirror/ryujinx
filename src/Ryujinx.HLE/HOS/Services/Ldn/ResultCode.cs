namespace Ryujinx.HLE.HOS.Services.Ldn
{
    enum ResultCode
    {
        ModuleId = 203,
        ErrorCodeShift = 9,

        Success = 0,

        DeviceNotAvailable = (16 << ErrorCodeShift) | ModuleId,
        DeviceDisabled = (22 << ErrorCodeShift) | ModuleId,
        InvalidState = (32 << ErrorCodeShift) | ModuleId,
        NodeNotFound = (48 << ErrorCodeShift) | ModuleId,
        ConnectFailure = (64 << ErrorCodeShift) | ModuleId,
        ConnectNotFound = (65 << ErrorCodeShift) | ModuleId,
        ConnectTimeout = (66 << ErrorCodeShift) | ModuleId,
        ConnectRejected = (67 << ErrorCodeShift) | ModuleId,
        InvalidArgument = (96 << ErrorCodeShift) | ModuleId,
        InvalidObject = (97 << ErrorCodeShift) | ModuleId,
        VersionTooLow = (113 << ErrorCodeShift) | ModuleId,
        VersionTooHigh = (114 << ErrorCodeShift) | ModuleId,
        TooManyPlayers = (144 << ErrorCodeShift) | ModuleId,
    }
}
