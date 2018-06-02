using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Nfp
{
    class IUser : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IUser()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Initialize }
            };
        }

        public long Initialize(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }
    }
}