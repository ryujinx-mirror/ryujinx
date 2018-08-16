using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IGlobalStateController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IGlobalStateController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                //...
            };
        }
    }
}