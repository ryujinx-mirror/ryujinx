using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfp
{
    class IUserManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IUserManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetUserInterface }
            };
        }

        public long GetUserInterface(ServiceCtx Context)
        {
            MakeObject(Context, new IUser());

            return 0;
        }
    }
}