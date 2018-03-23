using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Vi
{
    class ServiceVi : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceVi()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetDisplayService },
                { 1, GetDisplayService },
                { 2, GetDisplayService }
            };
        }

        public long GetDisplayService(ServiceCtx Context)
        {
            int ServiceType = Context.RequestData.ReadInt32();

            MakeObject(Context, new IApplicationDisplayService());

            return 0;
        }
    }
}