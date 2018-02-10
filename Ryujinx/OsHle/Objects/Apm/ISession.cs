using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Apm
{
    class ISession : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISession()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, SetPerformanceConfiguration }
            };
        }

        public long SetPerformanceConfiguration(ServiceCtx Context)
        {
            int PerfMode   = Context.RequestData.ReadInt32();
            int PerfConfig = Context.RequestData.ReadInt32();

            return 0;
        }
    }
}