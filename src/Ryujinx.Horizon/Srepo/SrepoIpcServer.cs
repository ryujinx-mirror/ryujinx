using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Srepo.Ipc;

namespace Ryujinx.Horizon.Srepo
{
    class SrepoIpcServer
    {
        private const int SrepoAMaxSessionsCount = 2;
        private const int SrepoUMaxSessionsCount = 30;
        private const int TotalMaxSessionsCount = SrepoAMaxSessionsCount + SrepoUMaxSessionsCount;

        private const int PointerBufferSize = 0x80;
        private const int MaxDomains = 32;
        private const int MaxDomainObjects = 192;
        private const int MaxPortsCount = 2;

        private static readonly ManagerOptions _options = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _options, TotalMaxSessionsCount);

            _serverManager.RegisterObjectForServer(new SrepoService(), ServiceName.Encode("srepo:a"), SrepoAMaxSessionsCount); // 5.0.0+
            _serverManager.RegisterObjectForServer(new SrepoService(), ServiceName.Encode("srepo:u"), SrepoUMaxSessionsCount); // 5.0.0+
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _serverManager.Dispose();
            _sm.Dispose();
        }
    }
}
