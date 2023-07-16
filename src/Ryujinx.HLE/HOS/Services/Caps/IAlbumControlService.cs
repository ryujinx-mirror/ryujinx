namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:c")]
    class IAlbumControlService : IpcService
    {
        public IAlbumControlService(ServiceCtx context) { }

        [CommandCmif(33)] // 7.0.0+
        // SetShimLibraryVersion(pid, u64, nn::applet::AppletResourceUserId)
        public ResultCode SetShimLibraryVersion(ServiceCtx context)
        {
            return context.Device.System.CaptureManager.SetShimLibraryVersion(context);
        }
    }
}
