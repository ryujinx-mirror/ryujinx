namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:dbg")]
    class IHidDebugServer : IpcService
    {
        public IHidDebugServer(ServiceCtx context) { }
    }
}
