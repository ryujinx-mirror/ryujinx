using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class IManagerRootService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IManagerRootService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
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