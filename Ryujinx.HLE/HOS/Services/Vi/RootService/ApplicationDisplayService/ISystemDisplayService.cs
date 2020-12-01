using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService
{
    class ISystemDisplayService : IpcService
    {
        private IApplicationDisplayService _applicationDisplayService;

        public ISystemDisplayService(IApplicationDisplayService applicationDisplayService)
        {
            _applicationDisplayService = applicationDisplayService;
        }

        [Command(2205)]
        // SetLayerZ(u64, u64)
        public ResultCode SetLayerZ(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [Command(2207)]
        // SetLayerVisibility(b8, u64)
        public ResultCode SetLayerVisibility(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [Command(2312)] // 1.0.0-6.2.0
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public ResultCode CreateStrayLayer(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return _applicationDisplayService.CreateStrayLayer(context);
        }

        [Command(3200)]
        // GetDisplayMode(u64) -> nn::vi::DisplayModeInfo
        public ResultCode GetDisplayMode(ServiceCtx context)
        {
            // TODO: De-hardcode resolution.
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);
            context.ResponseData.Write(60.0f);
            context.ResponseData.Write(0);

            return ResultCode.Success;
        }
    }
}