using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger;

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
            long layerFlags = context.RequestData.ReadInt64();
            long displayId  = context.RequestData.ReadInt64();

            context.Device.System.SurfaceFlinger.CreateLayer(context.Process, out long layerId);

            context.ResponseData.Write(layerId);

            return ResultCode.Success;
        }

        [Command(2011)]
        // DestroyManagedLayer(u64)
        public ResultCode DestroyManagedLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();

            context.Device.System.SurfaceFlinger.CloseLayer(layerId);

            return ResultCode.Success;
        }

        [Command(2012)] // 7.0.0+
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public ResultCode CreateStrayLayer(ServiceCtx context)
        {
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