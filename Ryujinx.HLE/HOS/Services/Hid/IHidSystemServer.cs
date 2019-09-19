namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:sys")]
    class IHidSystemServer : IpcService
    {
        public IHidSystemServer(ServiceCtx context) { }
    }
}