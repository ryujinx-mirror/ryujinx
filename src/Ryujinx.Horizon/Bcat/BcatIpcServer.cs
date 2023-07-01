using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Bcat
{
    internal class BcatIpcServer
    {
        private const int BcatMaxSessionsCount = 8;
        private const int BcatTotalMaxSessionsCount = BcatMaxSessionsCount * 4;

        private const int PointerBufferSize = 0x400;
        private const int MaxDomains = 64;
        private const int MaxDomainObjects = 64;
        private const int MaxPortsCount = 4;

        private SmApi _sm;
        private BcatServerManager _serverManager;

        private static readonly ManagerOptions _bcatManagerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        internal void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new BcatServerManager(allocator, _sm, MaxPortsCount, _bcatManagerOptions, BcatTotalMaxSessionsCount);

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterServer((int)BcatPortIndex.Admin,   ServiceName.Encode("bcat:a"), BcatMaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.Manager, ServiceName.Encode("bcat:m"), BcatMaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.User,    ServiceName.Encode("bcat:u"), BcatMaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.System,  ServiceName.Encode("bcat:s"), BcatMaxSessionsCount);
#pragma warning restore IDE0055
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _serverManager.Dispose();
        }
    }
}
