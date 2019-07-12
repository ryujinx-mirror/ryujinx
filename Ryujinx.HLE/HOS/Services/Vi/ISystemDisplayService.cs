using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class ISystemDisplayService : IpcService
    {
        private static IApplicationDisplayService _applicationDisplayService;

        public ISystemDisplayService(IApplicationDisplayService applicationDisplayService)
        {
            _applicationDisplayService = applicationDisplayService;
        }

        [Command(2205)]
        // SetLayerZ(u64, u64)
        public static long SetLayerZ(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        [Command(2207)]
        // SetLayerVisibility(b8, u64)
        public static long SetLayerVisibility(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        [Command(2312)] // 1.0.0-6.2.0
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public static long CreateStrayLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return _applicationDisplayService.CreateStrayLayer(context);
        }

        [Command(3200)]
        // GetDisplayMode(u64) -> nn::vi::DisplayModeInfo
        public static long GetDisplayMode(ServiceCtx context)
        {
            // TODO: De-hardcode resolution.
            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);
            context.ResponseData.Write(60.0f);
            context.ResponseData.Write(0);

            return 0;
        }
    }
}