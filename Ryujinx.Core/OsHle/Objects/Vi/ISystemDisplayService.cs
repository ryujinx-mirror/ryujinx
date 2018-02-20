using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Objects.Vi
{
    class ISystemDisplayService : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISystemDisplayService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 2205, SetLayerZ }
            };
        }

        public static long SetLayerZ(ServiceCtx Context)
        {
            return 0;
        }
    }
}