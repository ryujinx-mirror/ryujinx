namespace Ryujinx.HLE.HOS.Services.Sockets.Nsd
{
    enum ResultCode
    {
        ModuleId = 141,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidSettingsValue = (1 << ErrorCodeShift) | ModuleId,
        InvalidObject1 = (3 << ErrorCodeShift) | ModuleId,
        InvalidObject2 = (4 << ErrorCodeShift) | ModuleId,
        NullOutputObject = (5 << ErrorCodeShift) | ModuleId,
        SettingsNotLoaded = (6 << ErrorCodeShift) | ModuleId,
        InvalidArgument = (8 << ErrorCodeShift) | ModuleId,
        SettingsNotInitialized = (10 << ErrorCodeShift) | ModuleId,
        ServiceNotInitialized = (400 << ErrorCodeShift) | ModuleId,
    }
}
