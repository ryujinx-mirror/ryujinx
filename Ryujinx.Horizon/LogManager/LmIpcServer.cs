using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Sm;

namespace Ryujinx.Horizon.LogManager
{
    class LmIpcServer
    {
        private const int LogMaxSessionsCount = 42;

        private const int PointerBufferSize = 0x400;
        private const int MaxDomains = 31;
        private const int MaxDomainObjects = 61;

        private const int MaxPortsCount = 1;

        private static readonly ManagerOptions _logManagerOptions = new ManagerOptions(
            PointerBufferSize,
            MaxDomains,
            MaxDomainObjects,
            false);

        private static readonly ServiceName _logServiceName = ServiceName.Encode("lm");

        private SmApi _sm;
        private ServerManager _serverManager;

        private LmLog _logServiceObject;

        public void Initialize()
        {
            HeapAllocator allocator = new HeapAllocator();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _logManagerOptions, LogMaxSessionsCount);

            _logServiceObject = new LmLog();

            _serverManager.RegisterObjectForServer(_logServiceObject, _logServiceName, LogMaxSessionsCount);
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
