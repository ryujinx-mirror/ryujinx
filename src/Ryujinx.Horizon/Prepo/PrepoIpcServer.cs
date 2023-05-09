using Ryujinx.Horizon.Prepo.Types;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Prepo
{
    class PrepoIpcServer
    {
        private const int PrepoMaxSessionsCount      = 12;
        private const int PrepoTotalMaxSessionsCount = PrepoMaxSessionsCount * 6;

        private const int PointerBufferSize = 0x80;
        private const int MaxDomains        = 64;
        private const int MaxDomainObjects  = 16;
        private const int MaxPortsCount     = 6;

        private static readonly ManagerOptions _prepoManagerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private PrepoServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new PrepoServerManager(allocator, _sm, MaxPortsCount, _prepoManagerOptions, PrepoTotalMaxSessionsCount);

            _serverManager.RegisterServer((int)PrepoPortIndex.Admin,   ServiceName.Encode("prepo:a"),  PrepoMaxSessionsCount); // 1.0.0-5.1.0
            _serverManager.RegisterServer((int)PrepoPortIndex.Admin2,  ServiceName.Encode("prepo:a2"), PrepoMaxSessionsCount); // 6.0.0+
            _serverManager.RegisterServer((int)PrepoPortIndex.Manager, ServiceName.Encode("prepo:m"),  PrepoMaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.User,    ServiceName.Encode("prepo:u"),  PrepoMaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.System,  ServiceName.Encode("prepo:s"),  PrepoMaxSessionsCount);
            _serverManager.RegisterServer((int)PrepoPortIndex.Debug,   ServiceName.Encode("prepo:d"),  PrepoMaxSessionsCount); // 1.0.0
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