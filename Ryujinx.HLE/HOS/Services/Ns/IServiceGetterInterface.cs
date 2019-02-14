using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IServiceGetterInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IServiceGetterInterface()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 7996, GetApplicationManagerInterface }
            };
        }

        public long GetApplicationManagerInterface(ServiceCtx context)
        {
            MakeObject(context, new IApplicationManagerInterface());

            return 0;
        }
    }
}