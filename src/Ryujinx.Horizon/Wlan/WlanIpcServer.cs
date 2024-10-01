using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Wlan.Ipc;

namespace Ryujinx.Horizon.Wlan
{
    class WlanIpcServer
    {
        private const int WlanOtherMaxSessionsCount = 10;
        private const int WlanDtcMaxSessionsCount = 4;
        private const int WlanMaxSessionsCount = 30;
        private const int WlanNdMaxSessionsCount = 5;
        private const int WlanPMaxSessionsCount = 30;
        private const int TotalMaxSessionsCount = WlanDtcMaxSessionsCount + WlanMaxSessionsCount + WlanNdMaxSessionsCount + WlanPMaxSessionsCount + WlanOtherMaxSessionsCount * 6;

        private const int PointerBufferSize = 0x1000;
        private const int MaxDomains = 16;
        private const int MaxDomainObjects = 10;
        private const int MaxPortsCount = 10;

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
            _serverManager.RegisterObjectForServer(new GeneralServiceCreator(),  ServiceName.Encode("wlan"),     WlanMaxSessionsCount);      // 15.0.0+
            _serverManager.RegisterObjectForServer(new DetectManager(),          ServiceName.Encode("wlan:dtc"), WlanDtcMaxSessionsCount);   // 6.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new InfraManager(),           ServiceName.Encode("wlan:inf"), WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new LocalManager(),           ServiceName.Encode("wlan:lcl"), WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new LocalGetFrame(),          ServiceName.Encode("wlan:lg"),  WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new LocalGetActionFrame(),    ServiceName.Encode("wlan:lga"), WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new SfDriverServiceCreator(), ServiceName.Encode("wlan:nd"),  WlanNdMaxSessionsCount);    // 15.0.0+
            _serverManager.RegisterObjectForServer(new PrivateServiceCreator(),  ServiceName.Encode("wlan:p"),   WlanPMaxSessionsCount);     // 15.0.0+
            _serverManager.RegisterObjectForServer(new SocketGetFrame(),         ServiceName.Encode("wlan:sg"),  WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
            _serverManager.RegisterObjectForServer(new SocketManager(),          ServiceName.Encode("wlan:soc"), WlanOtherMaxSessionsCount); // 1.0.0-14.1.2
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
