using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ssl.Types;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Ssl.SslService
{
    class ISslContext : IpcService
    {
        private uint _connectionCount;

        private readonly ulong _processId;
        private readonly SslVersion _sslVersion;
        private ulong _serverCertificateId;
        private ulong _clientCertificateId;

        public ISslContext(ulong processId, SslVersion sslVersion)
        {
            _processId = processId;
            _sslVersion = sslVersion;
        }

        [CommandCmif(2)]
        // CreateConnection() -> object<nn::ssl::sf::ISslConnection>
        public ResultCode CreateConnection(ServiceCtx context)
        {
            MakeObject(context, new ISslConnection(_processId, _sslVersion));

            _connectionCount++;

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetConnectionCount() -> u32 count
        public ResultCode GetConnectionCount(ServiceCtx context)
        {
            context.ResponseData.Write(_connectionCount);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { _connectionCount });

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // ImportServerPki(nn::ssl::sf::CertificateFormat certificateFormat, buffer<bytes, 5> certificate) -> u64 certificateId
        public ResultCode ImportServerPki(ServiceCtx context)
        {
            CertificateFormat certificateFormat = (CertificateFormat)context.RequestData.ReadUInt32();

#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong certificateDataPosition = context.Request.SendBuff[0].Position;
            ulong certificateDataSize = context.Request.SendBuff[0].Size;
#pragma warning restore IDE0059

            context.ResponseData.Write(_serverCertificateId++);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { certificateFormat });

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // ImportClientPki(buffer<bytes, 5> certificate, buffer<bytes, 5> ascii_password) -> u64 certificateId
        public ResultCode ImportClientPki(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong certificateDataPosition = context.Request.SendBuff[0].Position;
            ulong certificateDataSize = context.Request.SendBuff[0].Size;
#pragma warning restore IDE0059

            ulong asciiPasswordDataPosition = context.Request.SendBuff[1].Position;
            ulong asciiPasswordDataSize = context.Request.SendBuff[1].Size;

            byte[] asciiPasswordData = new byte[asciiPasswordDataSize];

            context.Memory.Read(asciiPasswordDataPosition, asciiPasswordData);

            string asciiPassword = Encoding.ASCII.GetString(asciiPasswordData).Trim('\0');

            context.ResponseData.Write(_clientCertificateId++);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { asciiPassword });

            return ResultCode.Success;
        }
    }
}
