using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    class ISslService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISslService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateContext       },
                { 5, SetInterfaceVersion }
            };
        }

        // CreateContext(nn::ssl::sf::SslVersion, u64, pid) -> object<nn::ssl::sf::ISslContext>
        public long CreateContext(ServiceCtx context)
        {
            int  sslVersion = context.RequestData.ReadInt32();
            long unknown    = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceSsl, new { sslVersion, unknown });

            MakeObject(context, new ISslContext());

            return 0;
        }

        // SetInterfaceVersion(u32)
        public long SetInterfaceVersion(ServiceCtx context)
        {
            int version = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceSsl, new { version });

            return 0;
        }
    }
}