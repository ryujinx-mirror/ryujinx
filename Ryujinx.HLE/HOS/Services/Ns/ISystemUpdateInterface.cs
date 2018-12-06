using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class ISystemUpdateInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISystemUpdateInterface()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                //...
            };
        }
    }
}