using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ssl.Types;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Ssl.SslService
{
    class ISslConnection : IpcService
    {
        public ISslConnection() { }

        [CommandHipc(0)]
        // SetSocketDescriptor(u32) -> u32
        public ResultCode SetSocketDescriptor(ServiceCtx context)
        {
            uint socketFd          = context.RequestData.ReadUInt32();
            uint duplicateSocketFd = 0;

            context.ResponseData.Write(duplicateSocketFd);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { socketFd });

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // SetHostName(buffer<bytes, 5>)
        public ResultCode SetHostName(ServiceCtx context)
        {
            ulong hostNameDataPosition = context.Request.SendBuff[0].Position;
            ulong hostNameDataSize     = context.Request.SendBuff[0].Size;

            byte[] hostNameData = new byte[hostNameDataSize];

            context.Memory.Read(hostNameDataPosition, hostNameData);

            string hostName = Encoding.ASCII.GetString(hostNameData).Trim('\0');

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { hostName });

            return ResultCode.Success;
        }

        [CommandHipc(2)]
        // SetVerifyOption(nn::ssl::sf::VerifyOption)
        public ResultCode SetVerifyOption(ServiceCtx context)
        {
            VerifyOption verifyOption = (VerifyOption)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { verifyOption });

            return ResultCode.Success;
        }

        [CommandHipc(3)]
        // SetIoMode(nn::ssl::sf::IoMode)
        public ResultCode SetIoMode(ServiceCtx context)
        {
            IoMode ioMode = (IoMode)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { ioMode });

            return ResultCode.Success;
        }

        [CommandHipc(8)]
        // DoHandshake()
        public ResultCode DoHandshake(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSsl);

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // Write(buffer<bytes, 5>) -> u32
        public ResultCode Write(ServiceCtx context)
        {
            ulong inputDataPosition = context.Request.SendBuff[0].Position;
            ulong inputDataSize     = context.Request.SendBuff[0].Size;

            byte[] data = new byte[inputDataSize];

            context.Memory.Read(inputDataPosition, data);

            // NOTE: Tell the guest everything is transferred.
            uint transferredSize = (uint)inputDataSize;

            context.ResponseData.Write(transferredSize);

            Logger.Stub?.PrintStub(LogClass.ServiceSsl);

            return ResultCode.Success;
        }

        [CommandHipc(17)]
        // SetSessionCacheMode(nn::ssl::sf::SessionCacheMode)
        public ResultCode SetSessionCacheMode(ServiceCtx context)
        {
            SessionCacheMode sessionCacheMode = (SessionCacheMode)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { sessionCacheMode });

            return ResultCode.Success;
        }

        [CommandHipc(22)]
        // SetOption(b8, nn::ssl::sf::OptionType)
        public ResultCode SetOption(ServiceCtx context)
        {
            bool       optionEnabled = context.RequestData.ReadBoolean();
            OptionType optionType    = (OptionType)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { optionType, optionEnabled });

            return ResultCode.Success;
        }
    }
}