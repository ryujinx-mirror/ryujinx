namespace Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService
{
    class AndroidSurfaceComposerClient
    {
        // NOTE: This is android::SurfaceComposerClient::getDisplayInfo.
        public static (ulong, ulong) GetDisplayInfo(ServiceCtx context, ulong displayId = 0)
        {
            // TODO: This need to be REd, it should returns the driver resolution and more.
            if (context.Device.System.State.DockedMode)
            {
                return (1920, 1080);
            }
            else
            {
                return (1280, 720);
            }
        }
    }
}
