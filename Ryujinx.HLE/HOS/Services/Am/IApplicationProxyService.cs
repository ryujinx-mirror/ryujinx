using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    [Service("appletOE")]
    class IApplicationProxyService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IApplicationProxyService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, OpenApplicationProxy }
            };
        }

        public long OpenApplicationProxy(ServiceCtx context)
        {
            MakeObject(context, new IApplicationProxy());

            return 0;
        }
    }
}