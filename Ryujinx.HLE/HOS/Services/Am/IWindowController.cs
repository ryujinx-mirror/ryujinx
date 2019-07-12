using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IWindowController : IpcService
    {
        public IWindowController() { }

        [Command(1)]
        // GetAppletResourceUserId() -> nn::applet::AppletResourceUserId
        public long GetAppletResourceUserId(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);

            return 0;
        }

        [Command(10)]
        // AcquireForegroundRights()
        public long AcquireForegroundRights(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }
    }
}