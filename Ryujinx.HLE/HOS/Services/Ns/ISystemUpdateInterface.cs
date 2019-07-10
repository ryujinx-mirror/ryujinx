using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:su")]
    class ISystemUpdateInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISystemUpdateInterface(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                // ...
            };
        }
    }
}