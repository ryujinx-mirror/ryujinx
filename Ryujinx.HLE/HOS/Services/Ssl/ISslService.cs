using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ssl.SslService;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    [Service("ssl")]
    class ISslService : IpcService
    {
        public ISslService(ServiceCtx context) { }

        [Command(0)]
        // CreateContext(nn::ssl::sf::SslVersion, u64, pid) -> object<nn::ssl::sf::ISslContext>
        public ResultCode CreateContext(ServiceCtx context)
        {
            int  sslVersion = context.RequestData.ReadInt32();
            long unknown    = context.RequestData.ReadInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { sslVersion, unknown });

            MakeObject(context, new ISslContext(context));

            return ResultCode.Success;
        }

        [Command(5)]
        // SetInterfaceVersion(u32)
        public ResultCode SetInterfaceVersion(ServiceCtx context)
        {
            int version = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { version });

            return ResultCode.Success;
        }
    }
}