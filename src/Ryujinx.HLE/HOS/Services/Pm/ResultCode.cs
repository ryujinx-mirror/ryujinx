namespace Ryujinx.HLE.HOS.Services.Pm
{
    enum ResultCode
    {
        ModuleId = 15,
        ErrorCodeShift = 9,

        Success = 0,

        ProcessNotFound = (1 << ErrorCodeShift) | ModuleId,
        AlreadyStarted = (2 << ErrorCodeShift) | ModuleId,
        NotTerminated = (3 << ErrorCodeShift) | ModuleId,
        DebugHookInUse = (4 << ErrorCodeShift) | ModuleId,
        ApplicationRunning = (5 << ErrorCodeShift) | ModuleId,
        InvalidSize = (6 << ErrorCodeShift) | ModuleId,
    }
}
