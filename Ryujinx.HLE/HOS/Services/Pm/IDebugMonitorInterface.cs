namespace Ryujinx.HLE.HOS.Services.Pm
{
    [Service("pm:dmnt")]
    class IDebugMonitorInterface : IpcService
    {
        public IDebugMonitorInterface(ServiceCtx context) { }
    }
}