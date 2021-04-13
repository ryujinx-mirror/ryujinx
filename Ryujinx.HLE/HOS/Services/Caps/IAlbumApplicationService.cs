using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:u")]
    class IAlbumApplicationService : IpcService
    {
        public IAlbumApplicationService(ServiceCtx context) { }

        [CommandHipc(32)] // 7.0.0+
        // SetShimLibraryVersion(pid, u64, nn::applet::AppletResourceUserId)
        public ResultCode SetShimLibraryVersion(ServiceCtx context)
        {
            return context.Device.System.CaptureManager.SetShimLibraryVersion(context);
        }
    }
}