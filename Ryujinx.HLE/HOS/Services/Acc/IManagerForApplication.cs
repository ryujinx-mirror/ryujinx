using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Acc
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
            Context.Device.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetAccountId(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            Context.ResponseData.Write(0xcafeL);

            return 0;
        }
    }
}