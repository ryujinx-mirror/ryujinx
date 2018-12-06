using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Es
{
    class IeTicketService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private bool _isInitialized;

        public IeTicketService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {

            };
        }
    }
}
