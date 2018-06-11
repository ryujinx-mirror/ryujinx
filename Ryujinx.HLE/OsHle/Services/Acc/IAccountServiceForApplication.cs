using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Acc
{
    class IAccountServiceForApplication : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAccountServiceForApplication()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,   GetUserCount                        },
                { 1,   GetUserExistence                    },
                { 2,   ListAllUsers                        },
                { 3,   ListOpenUsers                       },
                { 4,   GetLastOpenedUser                   },
                { 5,   GetProfile                          },
                { 100, InitializeApplicationInfo           },
                { 101, GetBaasAccountManagerForApplication }
            };
        }

        public long GetUserCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetUserExistence(ServiceCtx Context)
        {
            Context.ResponseData.Write(1);

            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long ListAllUsers(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long ListOpenUsers(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetLastOpenedUser(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);

            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetProfile(ServiceCtx Context)
        {
            MakeObject(Context, new IProfile());

            return 0;
        }

        public long InitializeApplicationInfo(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            return 0;
        }

        public long GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            MakeObject(Context, new IManagerForApplication());

            return 0;
        }
    }
}
