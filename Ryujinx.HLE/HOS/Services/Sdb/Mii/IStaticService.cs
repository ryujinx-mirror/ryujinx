namespace Ryujinx.HLE.HOS.Services.Sdb.Mii
{
    [Service("mii:e")]
    [Service("mii:u")]
    class IStaticService : IpcService
    {
        public IStaticService(ServiceCtx context) { }
    }
}