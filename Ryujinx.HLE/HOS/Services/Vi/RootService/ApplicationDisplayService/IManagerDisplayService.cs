using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService
{
    class IManagerDisplayService : IpcService
    {
        private static IApplicationDisplayService _applicationDisplayService;

        public IManagerDisplayService(IApplicationDisplayService applicationDisplayService)
        {
            _applicationDisplayService = applicationDisplayService;
        }

        [Command(2010)]
        // CreateManagedLayer(u32, u64, nn::applet::AppletResourceUserId) -> u64
        public ResultCode CreateManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            context.ResponseData.Write(0L); //LayerId

            return ResultCode.Success;
        }

        [Command(2011)]
        // DestroyManagedLayer(u64)
        public ResultCode DestroyManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [Command(2012)] // 7.0.0+
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public ResultCode CreateStrayLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return _applicationDisplayService.CreateStrayLayer(context);
        }

        [Command(6000)]
        // AddToLayerStack(u32, u64)
        public ResultCode AddToLayerStack(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [Command(6002)]
        // SetLayerVisibility(b8, u64)
        public ResultCode SetLayerVisibility(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }
    }
}