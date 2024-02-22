using Ryujinx.Horizon.Prepo.Types;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Prepo
{
    class PrepoIpcServer
    {
        private const int MaxSessionsCount = 12;
        private const int TotalMaxSessionsCount = MaxSessionsCount * 6;

        private const int PointerBufferSize = 0x80;
        private const int MaxDomains = 64;
        private const int MaxDomainObjects = 16;
        private const int MaxPortsCount = 6;

        private static readonly ManagerOptions _managerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ArpApi _arp;
        private PrepoServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _arp = new ArpApi(allocator);

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new PrepoServerManager(allocator, _sm, _arp, MaxPortsCount, _managerOptions, TotalMaxSessionsCount);

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterServer((int)PrepoPortIndex.Admin,   ServiceName.Encode("prepo:a"),  MaxSessionsCount); // 1.0.0-5.1.0
            _serverManager.RegisterServer((int)PrepoPortIndex.Admin2,  ServiceName.Encode("prepo:a2"), MaxSessionsCount); // 6.0.0+
            _serverManager.RegisterServer((int)PrepoPortIndex.Manager, ServiceName.Encode("prepo:m"),  MaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.User,    ServiceName.Encode("prepo:u"),  MaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.System,  ServiceName.Encode("prepo:s"),  MaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.Debug,   ServiceName.Encode("prepo:d"),  MaxSessionsCount); // 1.0.0
#pragma warning restore IDE0055
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _arp.Dispose();
            _serverManager.Dispose();
            _sm.Dispose();
        }
    }
}
