using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IDaemonSuspendSessionService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IDaemonSuspendSessionService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                //...
            };
        }
    }
}