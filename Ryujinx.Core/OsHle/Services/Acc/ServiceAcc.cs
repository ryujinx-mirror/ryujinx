using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Acc
{
    class ServiceAcc : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceAcc()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {   3, ListOpenUsers                       },
                {   5, GetProfile                          },
                { 100, InitializeApplicationInfo           },
                { 101, GetBaasAccountManagerForApplication }
            };
        }

        public long ListOpenUsers(ServiceCtx Context)
        {
            return 0;
        }

        public long GetProfile(ServiceCtx Context)
        {
            MakeObject(Context, new IProfile());

            return 0;
        }

        public long InitializeApplicationInfo(ServiceCtx Context)
        {
            return 0;
        }

        public long GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            MakeObject(Context, new IManagerForApplication());

            return 0;
        }
    }
}