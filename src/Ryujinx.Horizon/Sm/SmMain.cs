using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Sm.Impl;
using Ryujinx.Horizon.Sm.Types;

namespace Ryujinx.Horizon.Sm
{
    public class SmMain
    {
        private const int SmMaxSessionsCount = 64;
        private const int SmmMaxSessionsCount = 1;
        private const int SmTotalMaxSessionsCount = SmMaxSessionsCount + SmmMaxSessionsCount;

        private const int MaxPortsCount = 2;

        private SmServerManager _serverManager;

        private readonly ServiceManager _serviceManager = new();

        public void Main()
        {
            HorizonStatic.Syscall.ManageNamedPort(out int smHandle, "sm:", SmMaxSessionsCount).AbortOnFailure();

            _serverManager = new SmServerManager(_serviceManager, null, null, MaxPortsCount, ManagerOptions.Default, SmTotalMaxSessionsCount);

            _serverManager.RegisterServer((int)SmPortIndex.User, smHandle);
            _serviceManager.RegisterServiceForSelf(out int smmHandle, ServiceName.Encode("sm:m"), SmmMaxSessionsCount).AbortOnFailure();
            _serverManager.RegisterServer((int)SmPortIndex.Manager, smmHandle);
            _serverManager.ServiceRequests();
        }
    }
}
