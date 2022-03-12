using Ryujinx.HLE.HOS.Services.Vi.RootService;
using Ryujinx.HLE.HOS.Services.Vi.Types;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:s")]
    class ISystemRootService : IpcService
    {
        // vi:u/m/s aren't on 3 separate threads but we can't put them together with the current ServerBase
        public ISystemRootService(ServiceCtx context) : base(context.Device.System.ViServerS) { }

        [CommandHipc(1)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public ResultCode GetDisplayService(ServiceCtx context)
        {
            ViServiceType serviceType = (ViServiceType)context.RequestData.ReadInt32();

            if (serviceType != ViServiceType.System)
            {
                return ResultCode.PermissionDenied;
            }

            MakeObject(context, new IApplicationDisplayService(serviceType));

            return ResultCode.Success;
        }
    }
}