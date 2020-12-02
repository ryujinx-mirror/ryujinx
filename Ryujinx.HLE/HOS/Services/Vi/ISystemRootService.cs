using Ryujinx.HLE.HOS.Services.Vi.RootService;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:s")]
    class ISystemRootService : IpcService
    {
        // vi:u/m/s aren't on 3 separate threads but we can't put them together with the current ServerBase
        public ISystemRootService(ServiceCtx context) : base(context.Device.System.ViServerS) { }

        [Command(1)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public ResultCode GetDisplayService(ServiceCtx context)
        {
            int serviceType = context.RequestData.ReadInt32();

            MakeObject(context, new IApplicationDisplayService());

            return ResultCode.Success;
        }
    }
}