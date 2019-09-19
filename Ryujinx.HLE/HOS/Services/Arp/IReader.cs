namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("arp:r")]
    class IReader : IpcService
    {
        public IReader(ServiceCtx context) { }
    }
}