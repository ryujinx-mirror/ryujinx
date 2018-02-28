using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.Nifm
{
    class ServiceNifm : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceNifm()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, CreateGeneralServiceOld }
            };
        }

        public long CreateGeneralServiceOld(ServiceCtx Context)
        {
            MakeObject(Context, new IGeneralService());

            return 0;
        }
    }
}