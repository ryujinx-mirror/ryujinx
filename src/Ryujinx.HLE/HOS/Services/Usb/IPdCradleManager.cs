namespace Ryujinx.HLE.HOS.Services.Usb
{
    [Service("usb:pd:c")]
    class IPdCradleManager : IpcService
    {
        public IPdCradleManager(ServiceCtx context) { }
    }
}