using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Ssl
{
    class ServiceSsl : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceSsl()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                //{ 0, Function }
            };
        }
    }
}