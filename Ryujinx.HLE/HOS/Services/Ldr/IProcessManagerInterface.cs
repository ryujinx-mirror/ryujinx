namespace Ryujinx.HLE.HOS.Services.Ldr
{
    [Service("ldr:pm")]
    class IProcessManagerInterface : IpcService
    {
        public IProcessManagerInterface(ServiceCtx context) { }
    }
}