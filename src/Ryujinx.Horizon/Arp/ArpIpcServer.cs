using Ryujinx.Horizon.Arp.Ipc;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Arp.Detail;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Arp
{
    class ArpIpcServer
    {
        private const int ArpRMaxSessionsCount = 16;
        private const int ArpWMaxSessionsCount = 8;
        private const int MaxSessionsCount = ArpRMaxSessionsCount + ArpWMaxSessionsCount;

        private const int PointerBufferSize = 0x1000;
        private const int MaxDomains = 24;
        private const int MaxDomainObjects = 32;
        private const int MaxPortsCount = 2;

        private static readonly ManagerOptions _managerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;
        private ApplicationInstanceManager _applicationInstanceManager;

        public IReader Reader { get; private set; }
        public IWriter Writer { get; private set; }

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _managerOptions, MaxSessionsCount);

            _applicationInstanceManager = new ApplicationInstanceManager();

            Reader reader = new(_applicationInstanceManager);
            Reader = reader;

            Writer writer = new(_applicationInstanceManager);
            Writer = writer;

            _serverManager.RegisterObjectForServer(reader, ServiceName.Encode("arp:r"), ArpRMaxSessionsCount);
            _serverManager.RegisterObjectForServer(writer, ServiceName.Encode("arp:w"), ArpWMaxSessionsCount);
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _applicationInstanceManager.Dispose();
            _serverManager.Dispose();
            _sm.Dispose();
        }
    }
}
