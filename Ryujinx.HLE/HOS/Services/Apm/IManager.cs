using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Apm
{
    class IManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, OpenSession }
            };
        }

        public long OpenSession(ServiceCtx context)
        {
            MakeObject(context, new ISession());

            return 0;
        }
    }
}