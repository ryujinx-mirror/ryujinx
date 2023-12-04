namespace Ryujinx.HLE.HOS.Services.Ptm.Fgm
{
    [Service("fgm")]   // 9.0.0+
    [Service("fgm:0")] // 9.0.0+
    [Service("fgm:9")] // 9.0.0+
    class ISession : IpcService
    {
        public ISession(ServiceCtx context) { }
    }
}
