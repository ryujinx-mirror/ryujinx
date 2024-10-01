using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Bcat
{
    internal class BcatIpcServer
    {
        private const int MaxSessionsCount = 8;
        private const int TotalMaxSessionsCount = MaxSessionsCount * 4;

        private const int PointerBufferSize = 0x400;
        private const int MaxDomains = 64;
        private const int MaxDomainObjects = 64;
        private const int MaxPortsCount = 4;

        private SmApi _sm;
        private BcatServerManager _serverManager;

        private static readonly ManagerOptions _managerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        internal void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new BcatServerManager(allocator, _sm, MaxPortsCount, _managerOptions, TotalMaxSessionsCount);

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterServer((int)BcatPortIndex.Admin,   ServiceName.Encode("bcat:a"), MaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.Manager, ServiceName.Encode("bcat:m"), MaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.User,    ServiceName.Encode("bcat:u"), MaxSessionsCount);
            _serverManager.RegisterServer((int)BcatPortIndex.System,  ServiceName.Encode("bcat:s"), MaxSessionsCount);
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
