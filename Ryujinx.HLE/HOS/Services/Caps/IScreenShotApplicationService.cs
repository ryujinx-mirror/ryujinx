namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:su")] // 6.0.0+
    class IScreenShotApplicationService : IpcService
    {
        public IScreenShotApplicationService(ServiceCtx context) { }
    }
}