using MsgPack;
using MsgPack.Serialization;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.Utilities;
using System.IO;
using System.Text;
using Utf8Json;

namespace Ryujinx.HLE.HOS.Services.Prepo
{
    [Service("prepo:a")]
    [Service("prepo:a2")]
    [Service("prepo:u")]
    class IPrepoService : IpcService
    {
        public IPrepoService(ServiceCtx context) { }

        [Command(10100)] // 1.0.0-5.1.0
        // SaveReport(u64, pid, buffer<u8, 9>, buffer<bytes, 5>)
        public ResultCode SaveReportOld(ServiceCtx context)
        {
            // We don't care about the differences since we don't use the play report.
            return ProcessReport(context, withUserID: false);
        }

        [Command(10101)] // 1.0.0-5.1.0
        // SaveReportWithUserOld(nn::account::Uid, u64, pid, buffer<u8, 9>, buffer<bytes, 5>)
        public ResultCode SaveReportWithUserOld(ServiceCtx context)
        {
            // We don't care about the differences since we don't use the play report.
            return ProcessReport(context, withUserID: true);
        }

        [Command(10102)] // 6.0.0+
        // SaveReport(u64, pid, buffer<u8, 9>, buffer<bytes, 5>)
        public ResultCode SaveReport(ServiceCtx context)
        {
            // We don't care about the differences since we don't use the play report.
            return ProcessReport(context, withUserID: false);
        }

        [Command(10103)] // 6.0.0+
        // SaveReportWithUser(nn::account::Uid, u64, pid, buffer<u8, 9>, buffer<bytes, 5>)
        public ResultCode SaveReportWithUser(ServiceCtx context)
        {
            // We don't care about the differences since we don't use the play report.
            return ProcessReport(context, withUserID: true);
        }

        private ResultCode ProcessReport(ServiceCtx context, bool withUserID)
        {
            UserId  userId   = withUserID ? context.RequestData.ReadStruct<UserId>() : new UserId();
            string  gameRoom = StringUtils.ReadUtf8String(context);

            if (withUserID)
            {
                if (userId.IsNull)
                {
                    return ResultCode.InvalidArgument;
                }
            }

            if (gameRoom == string.Empty)
            {
                return ResultCode.InvalidState;
            }

            long inputPosition = context.Request.SendBuff[0].Position;
            long inputSize     = context.Request.SendBuff[0].Size;

            if (inputSize == 0)
            {
                return ResultCode.InvalidBufferSize;
            }

            byte[] inputBuffer = context.Memory.ReadBytes(inputPosition, inputSize);

            Logger.PrintInfo(LogClass.ServicePrepo, ReadReportBuffer(inputBuffer, gameRoom, userId));

            return ResultCode.Success;
        }

        private string ReadReportBuffer(byte[] buffer, string room, UserId userId)
        {
            StringBuilder     builder = new StringBuilder();
            MessagePackObject deserializedReport = MessagePackSerializer.UnpackMessagePackObject(buffer);

            builder.AppendLine();
            builder.AppendLine("PlayReport log:");

            if (!userId.IsNull)
            {
                builder.AppendLine($" UserId: {userId.ToString()}");
            }

            builder.AppendLine($" Room: {room}");
            builder.AppendLine($" Report: {deserializedReport}");

            return builder.ToString();
        }
    }
}