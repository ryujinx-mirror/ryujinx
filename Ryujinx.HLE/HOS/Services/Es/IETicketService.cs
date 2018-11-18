using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Es
{
    class IETicketService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private bool IsInitialized;

        public IETicketService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {

            };
        }
    }
}
