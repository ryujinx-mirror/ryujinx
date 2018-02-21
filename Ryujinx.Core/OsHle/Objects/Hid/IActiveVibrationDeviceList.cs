using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Objects.Hid
{
    class IActiveApplicationDeviceList : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IActiveApplicationDeviceList()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>() { };
        }
    }
}