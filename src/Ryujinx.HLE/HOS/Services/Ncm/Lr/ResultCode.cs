namespace Ryujinx.HLE.HOS.Services.Ncm.Lr
{
    enum ResultCode
    {
        ModuleId = 8,
        ErrorCodeShift = 9,

        Success = 0,

        ProgramLocationEntryNotFound = (2 << ErrorCodeShift) | ModuleId,
        InvalidContextForControlLocation = (3 << ErrorCodeShift) | ModuleId,
        StorageNotFound = (4 << ErrorCodeShift) | ModuleId,
        AccessDenied = (5 << ErrorCodeShift) | ModuleId,
        OfflineManualHTMLLocationEntryNotFound = (6 << ErrorCodeShift) | ModuleId,
        TitleIsNotRegistered = (7 << ErrorCodeShift) | ModuleId,
        ControlLocationEntryForHostNotFound = (8 << ErrorCodeShift) | ModuleId,
        LegalInfoHTMLLocationEntryNotFound = (9 << ErrorCodeShift) | ModuleId,
        ProgramLocationForDebugEntryNotFound = (10 << ErrorCodeShift) | ModuleId,
    }
}
