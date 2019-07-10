using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    [Service("appletAE")]
    class IAllSystemAppletProxiesService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAllSystemAppletProxiesService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 100, OpenSystemAppletProxy }
            };
        }

        public long OpenSystemAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new ISystemAppletProxy());

            return 0;
        }
    }
}