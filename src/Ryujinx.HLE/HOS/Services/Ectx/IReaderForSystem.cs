namespace Ryujinx.HLE.HOS.Services.Ectx
{
    [Service("ectx:r")] // 11.0.0+
    class IReaderForSystem : IpcService
    {
        public IReaderForSystem(ServiceCtx context) { }
    }
}
