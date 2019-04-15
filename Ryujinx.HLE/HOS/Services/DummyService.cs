using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services
{
    class DummyService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public string ServiceName { get; set; }

        public DummyService(string serviceName)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>();
            ServiceName = serviceName;
        }
    }
}
