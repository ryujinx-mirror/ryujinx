using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Lm
{
    class ILogService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ILogService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Initialize }
            };
        }

        public long Initialize(ServiceCtx context)
        {
            MakeObject(context, new ILogger());

            return 0;
        }
    }
}