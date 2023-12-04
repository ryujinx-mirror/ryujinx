namespace Ryujinx.HLE.HOS.Services.Loader
{
    [Service("ldr:pm")]
    class IProcessManagerInterface : IpcService
    {
        public IProcessManagerInterface(ServiceCtx context) { }
    }
}
