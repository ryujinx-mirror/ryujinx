namespace Ryujinx.HLE.HOS.Services.Mii
{
    [Service("miiimg")] // 5.0.0+
    class IImageDatabaseService : IpcService
    {
        public IImageDatabaseService(ServiceCtx context) { }
    }
}