using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Vi
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
        public static long CreateManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            context.ResponseData.Write(0L); //LayerId

            return 0;
        }

        [Command(2011)]
        // DestroyManagedLayer(u64)
        public long DestroyManagedLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        [Command(2012)] // 7.0.0+
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public static long CreateStrayLayer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return _applicationDisplayService.CreateStrayLayer(context);
        }

        [Command(6000)]
        // AddToLayerStack(u32, u64)
        public static long AddToLayerStack(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }

        [Command(6002)]
        // SetLayerVisibility(b8, u64)
        public static long SetLayerVisibility(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceVi);

            return 0;
        }
    }
}