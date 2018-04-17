using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Acc
{
    class IAccountServiceForApplication : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAccountServiceForApplication()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {   0, GetUserCount                        },
                {   3, ListOpenUsers                       },
                {   5, GetProfile                          },
                { 100, InitializeApplicationInfo           },
                { 101, GetBaasAccountManagerForApplication }
            };
        }

        public long GetUserCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            Logging.Stub(LogClass.ServiceAcc, "Stubbed");

            return 0;
        }

        public long ListOpenUsers(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceAcc, "Stubbed");

            return 0;
        }

        public long GetProfile(ServiceCtx Context)
        {
            MakeObject(Context, new IProfile());

            return 0;
        }

        public long InitializeApplicationInfo(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceAcc, "Stubbed");

            return 0;
        }

        public long GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            MakeObject(Context, new IManagerForApplication());

            return 0;
        }
    }
}
