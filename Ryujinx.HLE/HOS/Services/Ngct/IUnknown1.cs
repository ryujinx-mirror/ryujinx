namespace Ryujinx.HLE.HOS.Services.Ngct
{
    [Service("ngct:s")] // 9.0.0+
    [Service("ngct:u")] // 9.0.0+
    class IUnknown1 : IpcService
    {
        public IUnknown1(ServiceCtx context) { }
    }
}