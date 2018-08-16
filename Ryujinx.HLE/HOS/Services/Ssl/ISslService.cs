using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    class ISslService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISslService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 5, SetInterfaceVersion }
            };
        }

        public long SetInterfaceVersion(ServiceCtx Context)
        {
            int Version = Context.RequestData.ReadInt32();

            Context.Device.Log.PrintStub(LogClass.ServiceSsl, "Stubbed.");

            return 0;
        }
    }
}