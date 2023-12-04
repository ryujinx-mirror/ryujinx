namespace Ryujinx.HLE.HOS.Services.Ldn.Lp2p
{
    [Service("lp2p:app")] // 9.0.0+
    [Service("lp2p:sys")] // 9.0.0+
    class IServiceCreator : IpcService
    {
        public IServiceCreator(ServiceCtx context) { }
    }
}
