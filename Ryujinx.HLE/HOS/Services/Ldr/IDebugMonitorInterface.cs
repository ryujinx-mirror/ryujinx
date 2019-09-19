namespace Ryujinx.HLE.HOS.Services.Ldr
{
    [Service("ldr:dmnt")]
    class IDebugMonitorInterface : IpcService
    {
        public IDebugMonitorInterface(ServiceCtx context) { }
    }
}