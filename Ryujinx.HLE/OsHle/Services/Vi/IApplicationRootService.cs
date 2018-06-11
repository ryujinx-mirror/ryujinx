using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Vi
{
    class IApplicationRootService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IApplicationRootService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetDisplayService }
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