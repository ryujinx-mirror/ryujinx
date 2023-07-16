namespace Ryujinx.HLE.HOS.Services.Srepo
{
    [Service("srepo:a")] // 5.0.0+
    [Service("srepo:u")] // 5.0.0+
    class ISrepoService : IpcService
    {
        public ISrepoService(ServiceCtx context) { }
    }
}
