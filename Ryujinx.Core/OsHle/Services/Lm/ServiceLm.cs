using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Lm
{
    class ServiceLm : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceLm()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Initialize }
            };
        }

        public long Initialize(ServiceCtx Context)
        {
            MakeObject(Context, new ILogger());

            return 0;
        }
    }
}