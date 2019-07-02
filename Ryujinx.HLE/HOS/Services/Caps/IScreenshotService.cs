using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    class IScreenshotService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IScreenshotService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                // ...
            };
        }
    }
}