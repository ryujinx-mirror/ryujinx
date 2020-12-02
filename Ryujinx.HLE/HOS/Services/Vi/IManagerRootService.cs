using Ryujinx.HLE.HOS.Services.Vi.RootService;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:m")]
    class IManagerRootService : IpcService
    {
        // vi:u/m/s aren't on 3 separate threads but we can't put them together with the current ServerBase
        public IManagerRootService(ServiceCtx context) : base(context.Device.System.ViServerM) { }

        [Command(2)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public ResultCode GetDisplayService(ServiceCtx context)
        {
            int serviceType = context.RequestData.ReadInt32();

            MakeObject(context, new IApplicationDisplayService());

            return ResultCode.Success;
        }
    }
}