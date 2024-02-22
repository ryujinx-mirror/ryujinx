using Ryujinx.Horizon.Ins.Ipc;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Ins
{
    class InsIpcServer
    {
        private const int InsMaxSessionsCount = 8;
        private const int TotalMaxSessionsCount = InsMaxSessionsCount * 2;

        private const int PointerBufferSize = 0x200;
        private const int MaxDomains = 0;
        private const int MaxDomainObjects = 0;
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

#pragma warning disable IDE0055 // Disable formatting
            _serverManager.RegisterObjectForServer(new ReceiverManager(), ServiceName.Encode("ins:r"), InsMaxSessionsCount); // 9.0.0+
            _serverManager.RegisterObjectForServer(new SenderManager(),   ServiceName.Encode("ins:s"), InsMaxSessionsCount); // 9.0.0+
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
