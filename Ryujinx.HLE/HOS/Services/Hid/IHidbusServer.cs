namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hidbus")]
    class IHidbusServer : IpcService
    {
        public IHidbusServer(ServiceCtx context) { }
    }
}