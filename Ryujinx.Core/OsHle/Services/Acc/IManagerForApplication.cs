using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Acc
{
    class IManagerForApplication : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IManagerForApplication()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, CheckAvailability },
                { 1, GetAccountId      }
            };
        }

        public long CheckAvailability(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceAcc, "Stubbed");

            return 0;
        }

        public long GetAccountId(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceAcc, "AccountId = 0xcafeL");

            Context.ResponseData.Write(0xcafeL);

            return 0;
        }
    }
}