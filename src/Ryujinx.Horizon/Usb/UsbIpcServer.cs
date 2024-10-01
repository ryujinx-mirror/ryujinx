using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Usb.Ipc;

namespace Ryujinx.Horizon.Usb
{
    class UsbIpcServer
    {
        private const int UsbDsMaxSessionsCount = 4;
        private const int UsbHsMaxSessionsCount = 20;
        private const int UsbHsAMaxSessionsCount = 3;
        private const int UsbObsvMaxSessionsCount = 2;
        private const int UsbPdMaxSessionsCount = 6;
        private const int UsbPdCMaxSessionsCount = 4;
        private const int UsbPdMMaxSessionsCount = 1;
        private const int UsbPmMaxSessionsCount = 5;
        private const int UsbQdbMaxSessionsCount = 4;
        private const int TotalMaxSessionsCount =
            UsbDsMaxSessionsCount +
            UsbHsMaxSessionsCount +
            UsbHsAMaxSessionsCount +
            UsbObsvMaxSessionsCount +
            UsbPdMaxSessionsCount +
            UsbPdCMaxSessionsCount +
            UsbPdMMaxSessionsCount +
            UsbPmMaxSessionsCount +
            UsbQdbMaxSessionsCount;

        private const int PointerBufferSize = 0;
        private const int MaxDomains = 0;
        private const int MaxDomainObjects = 0;
        private const int MaxPortsCount = 9;

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
            _serverManager.RegisterObjectForServer(new DsRootSession(),        ServiceName.Encode("usb:ds"),   UsbDsMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new ClientRootSession(),    ServiceName.Encode("usb:hs"),   UsbHsMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new ClientRootSession(),    ServiceName.Encode("usb:hs:a"), UsbHsAMaxSessionsCount);  // 7.0.0+
            _serverManager.RegisterObjectForServer(new PmObserverService(),    ServiceName.Encode("usb:obsv"), UsbObsvMaxSessionsCount); // 8.0.0+
            _serverManager.RegisterObjectForServer(new PdManager(),            ServiceName.Encode("usb:pd"),   UsbPdMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new PdCradleManager(),      ServiceName.Encode("usb:pd:c"), UsbPdCMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new PdManufactureManager(), ServiceName.Encode("usb:pd:m"), UsbPdMMaxSessionsCount);  // 1.0.0
            _serverManager.RegisterObjectForServer(new PmService(),            ServiceName.Encode("usb:pm"),   UsbPmMaxSessionsCount);
            _serverManager.RegisterObjectForServer(new QdbManager(),           ServiceName.Encode("usb:qdb"),  UsbQdbMaxSessionsCount);  // 7.0.0+
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
