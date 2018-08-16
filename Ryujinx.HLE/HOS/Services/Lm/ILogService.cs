using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Lm
{
    class ILogService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ILogService()
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