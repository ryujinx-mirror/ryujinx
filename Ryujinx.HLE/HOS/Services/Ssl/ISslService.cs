using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ssl.SslService;
using Ryujinx.HLE.HOS.Services.Ssl.Types;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    [Service("ssl")]
    class ISslService : IpcService
    {
        // NOTE: The SSL service is used by games to connect it to various official online services, which we do not intend to support.
        //       In this case it is acceptable to stub all calls of the service.
        public ISslService(ServiceCtx context) { }

        [CommandHipc(0)]
        // CreateContext(nn::ssl::sf::SslVersion, u64, pid) -> object<nn::ssl::sf::ISslContext>
        public ResultCode CreateContext(ServiceCtx context)
        {
            SslVersion sslVersion     = (SslVersion)context.RequestData.ReadUInt32();
            ulong      pidPlaceholder = context.RequestData.ReadUInt64();

            MakeObject(context, new ISslContext(context));

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { sslVersion });

            return ResultCode.Success;
        }

        [CommandHipc(5)]
        // SetInterfaceVersion(u32)
        public ResultCode SetInterfaceVersion(ServiceCtx context)
        {
            // 1 = 3.0.0+, 2 = 5.0.0+, 3 = 6.0.0+
            uint interfaceVersion = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { interfaceVersion });

            return ResultCode.Success;
        }
    }
}