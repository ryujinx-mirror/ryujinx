namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:m")] // 9.0.0+
    class IPowerStateRequestHandler : IpcService
    {
        public IPowerStateRequestHandler(ServiceCtx context) { }
    }
}