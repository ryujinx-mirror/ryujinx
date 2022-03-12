using Ryujinx.HLE.HOS.Services.Vi.RootService;
using Ryujinx.HLE.HOS.Services.Vi.Types;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:u")]
    class IApplicationRootService : IpcService
    {
        public IApplicationRootService(ServiceCtx context) : base(context.Device.System.ViServer) { }

        [CommandHipc(0)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public ResultCode GetDisplayService(ServiceCtx context)
        {
            ViServiceType serviceType = (ViServiceType)context.RequestData.ReadInt32();

            if (serviceType != ViServiceType.Application)
            {
                return ResultCode.PermissionDenied;
            }

            MakeObject(context, new IApplicationDisplayService(serviceType));

            return ResultCode.Success;
        }
    }
}