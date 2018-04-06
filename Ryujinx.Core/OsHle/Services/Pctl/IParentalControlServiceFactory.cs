using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Pctl
{
    class IParentalControlServiceFactory : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IParentalControlServiceFactory()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, CreateService }
            };
        }

        public static long CreateService(ServiceCtx Context)
        {
            MakeObject(Context, new IParentalControlService());

            return 0;
        }
    }
}