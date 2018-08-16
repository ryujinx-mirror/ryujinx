using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Bcat
{
    class IBcatService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IBcatService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                //...
            };
        }

    }
}
