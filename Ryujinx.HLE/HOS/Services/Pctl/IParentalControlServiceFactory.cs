using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pctl
{
    [Service("pctl")]
    [Service("pctl:a")]
    [Service("pctl:r")]
    [Service("pctl:s")]
    class IParentalControlServiceFactory : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IParentalControlServiceFactory(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateService                  },
                { 1, CreateServiceWithoutInitialize }
            };
        }

        public long CreateService(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService());

            return 0;
        }

        public long CreateServiceWithoutInitialize(ServiceCtx context)
        {
            MakeObject(context, new IParentalControlService(false));

            return 0;
        }
    }
}