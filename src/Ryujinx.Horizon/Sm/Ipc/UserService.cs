using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sm;
using Ryujinx.Horizon.Sm.Impl;

namespace Ryujinx.Horizon.Sm.Ipc
{
    partial class UserService : IUserService
    {
        private readonly ServiceManager _serviceManager;

        private ulong _clientProcessId;
        private bool _initialized;

        public UserService(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [CmifCommand(0)]
        public Result Initialize([ClientProcessId] ulong clientProcessId)
        {
            _clientProcessId = clientProcessId;
            _initialized = true;

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result GetService([MoveHandle] out int handle, ServiceName name)
        {
            if (!_initialized)
            {
                handle = 0;

                return SmResult.InvalidClient;
            }

            return _serviceManager.GetService(out handle, _clientProcessId, name);
        }

        [CmifCommand(2)]
        public Result RegisterService([MoveHandle] out int handle, ServiceName name, int maxSessions, bool isLight)
        {
            if (!_initialized)
            {
                handle = 0;

                return SmResult.InvalidClient;
            }

            return _serviceManager.RegisterService(out handle, _clientProcessId, name, maxSessions, isLight);
        }

        [CmifCommand(3)]
        public Result UnregisterService(ServiceName name)
        {
            if (!_initialized)
            {
                return SmResult.InvalidClient;
            }

            return _serviceManager.UnregisterService(_clientProcessId, name);
        }
    }
}
