namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:ss")] // 2.0.0+
    class IScreenshotService : IpcService
    {
        public IScreenshotService(ServiceCtx context) { }
    }
}
