using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Pctl
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
                Context.Ns.Log.PrintWarning(LogClass.ServicePctl, "Service is already initialized!");
            }

            return 0;
        }
    }
}