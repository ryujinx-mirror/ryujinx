namespace Ryujinx.HLE.HOS.Services.Arp
{
    [Service("arp:w")]
    class IWriter : IpcService
    {
        public IWriter(ServiceCtx context) { }
    }
}