namespace Ryujinx.HLE.HOS.Services.Pctl
{
    enum ResultCode
    {
        ModuleId = 142,
        ErrorCodeShift = 9,

        Success = 0,

        FreeCommunicationDisabled = (101 << ErrorCodeShift) | ModuleId,
        StereoVisionDenied = (104 << ErrorCodeShift) | ModuleId,
        InvalidPid = (131 << ErrorCodeShift) | ModuleId,
        PermissionDenied = (133 << ErrorCodeShift) | ModuleId,
        StereoVisionRestrictionConfigurableDisabled = (181 << ErrorCodeShift) | ModuleId,
    }
}
