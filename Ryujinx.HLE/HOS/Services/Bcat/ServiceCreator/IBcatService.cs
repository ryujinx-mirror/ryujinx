using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Bcat.ServiceCreator
{
    class IBcatService : IpcService
    {
        public IBcatService(ApplicationLaunchProperty applicationLaunchProperty) { }

        [Command(10100)]
        // RequestSyncDeliveryCache() -> object<nn::bcat::detail::ipc::IDeliveryCacheProgressService>
        public ResultCode RequestSyncDeliveryCache(ServiceCtx context)
        {
            MakeObject(context, new IDeliveryCacheProgressService(context));

            return ResultCode.Success;
        }
    }
}