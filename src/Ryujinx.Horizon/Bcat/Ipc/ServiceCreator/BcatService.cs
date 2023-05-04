using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Bcat;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Bcat.Ipc
{
    partial class BcatService : IBcatService
    {
        private readonly BcatServicePermissionLevel _permissionLevel;

        public BcatService(BcatServicePermissionLevel permissionLevel)
        {
            _permissionLevel = permissionLevel;
        }

        [CmifCommand(10100)]
        public Result RequestSyncDeliveryCache(out IDeliveryCacheProgressService deliveryCacheProgressService)
        {
            deliveryCacheProgressService = new DeliveryCacheProgressService();

            return Result.Success;
        }
    }
}
