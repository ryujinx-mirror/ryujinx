using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Nifm
{
    class IStaticService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IStaticService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, CreateGeneralServiceOld },
                { 5, CreateGeneralService    }
            };
        }

        public long CreateGeneralServiceOld(ServiceCtx Context)
        {
            MakeObject(Context, new IGeneralService());

            return 0;
        }

        public long CreateGeneralService(ServiceCtx Context)
        {
            MakeObject(Context, new IGeneralService());

            return 0;
        }
    }
}