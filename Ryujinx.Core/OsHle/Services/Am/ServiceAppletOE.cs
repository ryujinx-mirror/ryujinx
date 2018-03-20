using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class ServiceAppletOE : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceAppletOE()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, OpenApplicationProxy }
            };
        }

        public long OpenApplicationProxy(ServiceCtx Context)
        {
            MakeObject(Context, new IApplicationProxy());

            return 0;
        }
    }
}