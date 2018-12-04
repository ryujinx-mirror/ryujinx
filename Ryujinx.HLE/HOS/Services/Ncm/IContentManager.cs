using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ncm
{
    class IContentManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IContentManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {

            };
        }
    }
}
