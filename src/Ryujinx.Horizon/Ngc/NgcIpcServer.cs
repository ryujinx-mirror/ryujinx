using Ryujinx.Horizon.Ngc.Ipc;
using Ryujinx.Horizon.Sdk.Fs;
using Ryujinx.Horizon.Sdk.Ngc.Detail;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Ngc
{
    class NgcIpcServer
    {
        private const int MaxSessionsCount = 4;

        private const int PointerBufferSize = 0;
        private const int MaxDomains = 0;
        private const int MaxDomainObjects = 0;
        private const int MaxPortsCount = 1;

        private static readonly ManagerOptions _options = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;
        private ProfanityFilter _profanityFilter;

        public void Initialize(IFsClient fsClient)
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _profanityFilter = new(fsClient);
            _profanityFilter.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _options, MaxSessionsCount);

            _serverManager.RegisterObjectForServer(new Service(_profanityFilter), ServiceName.Encode("ngc:u"), MaxSessionsCount);
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _serverManager.Dispose();
            _profanityFilter.Dispose();
            _sm.Dispose();
        }
    }
}
