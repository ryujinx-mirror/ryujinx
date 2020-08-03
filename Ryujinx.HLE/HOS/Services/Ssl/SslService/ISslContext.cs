using Ryujinx.Common.Logging;
using System;

namespace Ryujinx.HLE.HOS.Services.Ssl.SslService
{
    class ISslContext : IpcService
    {
        public ISslContext(ServiceCtx context) { }
        
        [Command(4)]
        // ImportServerPki(nn::ssl::sf::CertificateFormat certificateFormat, buffer<bytes, 5> certificate) -> u64 certificateId
        public ResultCode ImportServerPki(ServiceCtx context)
        {
            int   certificateFormat       = context.RequestData.ReadInt32();
            long  certificateDataPosition = context.Request.SendBuff[0].Position;
            long  certificateDataSize     = context.Request.SendBuff[0].Size;
            ulong certificateId           = 1;

            context.ResponseData.Write(certificateId);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { certificateFormat, certificateDataPosition, certificateDataSize });

            return ResultCode.Success;
        }

    }
}
