using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Time
{
    class ITimeZoneService : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ITimeZoneService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                //...
            };
        }
    }
}