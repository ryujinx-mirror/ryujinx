using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pctl
{
    class IParentalControlService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private bool Initialized = false;

        private bool NeedInitialize;

        public IParentalControlService(bool NeedInitialize = true)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1, Initialize }
            };

            this.NeedInitialize = NeedInitialize;
        }

        public long Initialize(ServiceCtx Context)
        {
            if (NeedInitialize && !Initialized)
            {
                Initialized = true;
            }
            else
            {
                Logger.PrintWarning(LogClass.ServicePctl, "Service is already initialized!");
            }

            return 0;
        }
    }
}