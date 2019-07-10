using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    [Service("vi:u")]
    class IApplicationRootService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IApplicationRootService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetDisplayService }
            };
        }

        public long GetDisplayService(ServiceCtx context)
        {
            int serviceType = context.RequestData.ReadInt32();

            MakeObject(context, new IApplicationDisplayService());

            return 0;
        }
    }
}