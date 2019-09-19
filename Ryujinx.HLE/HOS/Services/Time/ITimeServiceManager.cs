namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:su")] // 9.0.0+
    class ITimeServiceManager : IpcService
    {
        public ITimeServiceManager(ServiceCtx context) { }
    }
}