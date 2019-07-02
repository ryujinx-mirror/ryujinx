using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    class ISslContext : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISslContext()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                // ...
            };
        }
    }
}