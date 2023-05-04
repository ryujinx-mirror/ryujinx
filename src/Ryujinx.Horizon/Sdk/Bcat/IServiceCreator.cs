using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Bcat
{
    internal interface IServiceCreator : IServiceObject
    {
        Result CreateBcatService(out IBcatService service, ulong pid);
        Result CreateDeliveryCacheStorageService(out IDeliveryCacheStorageService service, ulong pid);
        Result CreateDeliveryCacheStorageServiceWithApplicationId(out IDeliveryCacheStorageService service, Ncm.ApplicationId applicationId);
    }
}
