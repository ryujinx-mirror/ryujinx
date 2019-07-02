using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pctl
{
    class IParentalControlService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private bool _initialized = false;

        private bool _needInitialize;

        public IParentalControlService(bool needInitialize = true)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 1,    Initialize                       },
                { 1001, CheckFreeCommunicationPermission }
            };

            _needInitialize = needInitialize;
        }

        public long Initialize(ServiceCtx context)
        {
            if (_needInitialize && !_initialized)
            {
                _initialized = true;
            }
            else
            {
                Logger.PrintWarning(LogClass.ServicePctl, "Service is already initialized!");
            }

            return 0;
        }

        public long CheckFreeCommunicationPermission(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServicePctl);

            return 0;
        }
    }
}