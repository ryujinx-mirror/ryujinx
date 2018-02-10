using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Am
{
    class IDebugFunctions : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IDebugFunctions()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                //...
            };
        }
    }
}