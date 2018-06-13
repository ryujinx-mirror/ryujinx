using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Pctl
{
    class IParentalControlServiceFactory : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IParentalControlServiceFactory()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, CreateService                  },
                { 1, CreateServiceWithoutInitialize }
            };
        }

        public long CreateService(ServiceCtx Context)
        {
            MakeObject(Context, new IParentalControlService());

            return 0;
        }

        public long CreateServiceWithoutInitialize(ServiceCtx Context)
        {
            MakeObject(Context, new IParentalControlService(false));

            return 0;
        }
    }
}