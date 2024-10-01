using Ryujinx.Horizon.Psc.Ipc;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Psc
{
    class PscIpcServer
    {
        private const int PscCMaxSessionsCount = 1;
        private const int PscMMaxSessionsCount = 50;
        private const int PscLMaxSessionsCount = 5;
        private const int TotalMaxSessionsCount = PscCMaxSessionsCount + PscMMaxSessionsCount + PscLMaxSessionsCount;

        private const int PointerBufferSize = 0;
        private const int MaxDomains = 0;
        private const int MaxDomainObjects = 0;
        private const int MaxPortsCount = 3;

        private static readonly ManagerOptions _options = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _options, TotalMaxSessionsCount);

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterObjectForServer(new PmControl(),   ServiceName.Encode("psc:c"), PscCMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new PmService(),   ServiceName.Encode("psc:m"), PscMMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new PmStateLock(), ServiceName.Encode("psc:l"), PscLMaxSessionsCount); // 9.0.0+
#pragma warning restore IDE0055
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
