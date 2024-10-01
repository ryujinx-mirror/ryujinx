namespace Ryujinx.HLE.HOS.Services.Ectx
{
    [Service("ectx:w")] // 11.0.0+
    class IWriterForSystem : IpcService
    {
        public IWriterForSystem(ServiceCtx context) { }
    }
}
