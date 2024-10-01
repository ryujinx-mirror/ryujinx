namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:p")] // 9.0.0+
    class IPowerStateRequestHandler : IpcService
    {
        public IPowerStateRequestHandler(ServiceCtx context) { }
    }
}
