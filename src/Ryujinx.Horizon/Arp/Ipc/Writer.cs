using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Arp;
using Ryujinx.Horizon.Sdk.Arp.Detail;
using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Arp.Ipc
{
    partial class Writer : IWriter, IServiceObject
    {
        private readonly ApplicationInstanceManager _applicationInstanceManager;

        public Writer(ApplicationInstanceManager applicationInstanceManager)
        {
            _applicationInstanceManager = applicationInstanceManager;
        }

        [CmifCommand(0)]
        public Result AcquireRegistrar(out IRegistrar registrar)
        {
            if (_applicationInstanceManager.Entries[0] != null)
            {
                if (_applicationInstanceManager.Entries[1] != null)
                {
                    registrar = null;

                    return ArpResult.NoFreeInstance;
                }
                else
                {
                    _applicationInstanceManager.Entries[1] = new ApplicationInstance();

                    registrar = new Registrar(_applicationInstanceManager.Entries[1]);
                }
            }
            else
            {
                _applicationInstanceManager.Entries[0] = new ApplicationInstance();

                registrar = new Registrar(_applicationInstanceManager.Entries[0]);
            }

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result UnregisterApplicationInstance(ulong applicationInstanceId)
        {
            if (_applicationInstanceManager.Entries[applicationInstanceId] != null)
            {
                _applicationInstanceManager.Entries[applicationInstanceId] = null;
            }

            Os.SignalSystemEvent(ref _applicationInstanceManager.SystemEvent);

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result AcquireApplicationProcessPropertyUpdater(out IUpdater updater, ulong applicationInstanceId)
        {
            updater = new Updater(_applicationInstanceManager, applicationInstanceId, false);

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result AcquireApplicationCertificateUpdater(out IUpdater updater, ulong applicationInstanceId)
        {
            updater = new Updater(_applicationInstanceManager, applicationInstanceId, true);

            return Result.Success;
        }
    }
}
