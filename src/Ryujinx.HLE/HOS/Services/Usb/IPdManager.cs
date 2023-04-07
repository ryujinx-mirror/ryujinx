namespace Ryujinx.HLE.HOS.Services.Usb
{
    [Service("usb:pd")]
    class IPdManager : IpcService
    {
        public IPdManager(ServiceCtx context) { }
    }
}