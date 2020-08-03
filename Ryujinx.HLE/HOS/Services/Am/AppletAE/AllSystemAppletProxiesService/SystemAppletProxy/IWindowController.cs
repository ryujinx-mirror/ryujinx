using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IWindowController : IpcService
    {
        public IWindowController() { }

        [Command(1)]
        // GetAppletResourceUserId() -> nn::applet::AppletResourceUserId
        public ResultCode GetAppletResourceUserId(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [Command(10)]
        // AcquireForegroundRights()
        public ResultCode AcquireForegroundRights(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}