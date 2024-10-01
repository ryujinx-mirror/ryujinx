using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Friends
{
    class FriendsIpcServer
    {
        private const int MaxSessionsCount = 8;
        private const int TotalMaxSessionsCount = MaxSessionsCount * 5;

        private const int PointerBufferSize = 0xA00;
        private const int MaxDomains = 64;
        private const int MaxDomainObjects = 16;
        private const int MaxPortsCount = 5;

        private static readonly ManagerOptions _managerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private FriendsServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new FriendsServerManager(allocator, _sm, MaxPortsCount, _managerOptions, TotalMaxSessionsCount);

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterServer((int)FriendsPortIndex.Admin,   ServiceName.Encode("friend:a"), MaxSessionsCount);
            _serverManager.RegisterServer((int)FriendsPortIndex.User,    ServiceName.Encode("friend:u"), MaxSessionsCount);
            _serverManager.RegisterServer((int)FriendsPortIndex.Viewer,  ServiceName.Encode("friend:v"), MaxSessionsCount);
            _serverManager.RegisterServer((int)FriendsPortIndex.Manager, ServiceName.Encode("friend:m"), MaxSessionsCount);
            _serverManager.RegisterServer((int)FriendsPortIndex.System,  ServiceName.Encode("friend:s"), MaxSessionsCount);
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
