using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ncm
{
    class IContentStorage : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IContentStorage()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {

            };
        }
    }
}
