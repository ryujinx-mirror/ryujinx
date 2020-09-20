using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:su")] // 6.0.0+
    class IScreenShotApplicationService : IpcService
    {
        public IScreenShotApplicationService(ServiceCtx context) { }

        [Command(32)] // 7.0.0+
        // SetShimLibraryVersion(pid, u64, nn::applet::AppletResourceUserId)
        public ResultCode SetShimLibraryVersion(ServiceCtx context)
        {
            ulong shimLibraryVersion   = context.RequestData.ReadUInt64();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceCaps, new { shimLibraryVersion, appletResourceUserId });

            return ResultCode.Success;
        }
    }
}