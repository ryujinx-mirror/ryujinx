using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IDebugFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IDebugFunctions()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                //...
            };
        }
    }
}