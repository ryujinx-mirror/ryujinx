using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Sm.Impl;

namespace Ryujinx.Horizon.Sm
{
    public class SmMain
    {
        private enum PortIndex
        {
            User,
            Manager
        }

        private const int MaxPortsCount = 2;

        private readonly ServerManager _serverManager = new ServerManager(null, null, MaxPortsCount, ManagerOptions.Default, 0);
        private readonly ServiceManager _serviceManager = new ServiceManager();

        public void Main()
        {
            HorizonStatic.Syscall.ManageNamedPort(out int smHandle, "sm:", 64).AbortOnFailure();

            _serverManager.RegisterServer((int)PortIndex.User, smHandle);
            _serviceManager.RegisterServiceForSelf(out int smmHandle, ServiceName.Encode("sm:m"), 1).AbortOnFailure();
            _serverManager.RegisterServer((int)PortIndex.Manager, smmHandle);
            _serverManager.ServiceRequests();
        }
    }
}
