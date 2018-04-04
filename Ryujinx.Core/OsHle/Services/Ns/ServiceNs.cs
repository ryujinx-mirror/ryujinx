using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Ns
{
    class ServiceNs : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceNs()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 2, CountAddOnContent }
            };
        }

        public static long CountAddOnContent(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }
    }
}