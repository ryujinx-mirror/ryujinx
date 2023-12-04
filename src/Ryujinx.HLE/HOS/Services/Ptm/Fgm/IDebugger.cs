namespace Ryujinx.HLE.HOS.Services.Ptm.Fgm
{
    [Service("fgm:dbg")] // 9.0.0+
    class IDebugger : IpcService
    {
        public IDebugger(ServiceCtx context) { }
    }
}
