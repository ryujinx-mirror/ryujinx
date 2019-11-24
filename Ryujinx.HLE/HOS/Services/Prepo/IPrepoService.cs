using Ryujinx.Common.Logging;
using Ryujinx.HLE.Utilities;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

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
            UInt128 userId   = withUserID ? new UInt128(context.RequestData.ReadBytes(0x10)) : new UInt128();
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

        public string ReadReportBuffer(byte[] buffer, string room, UInt128 userId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("PlayReport log:");

            if (!userId.IsNull)
            {
                sb.AppendLine($" UserId: {userId.ToString()}");
            }

            sb.AppendLine($" Room: {room}");

            using (MemoryStream stream = new MemoryStream(buffer))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte  unknown1 = reader.ReadByte();  // Version ?
                short unknown2 = reader.ReadInt16(); // Size ?

                bool isValue = false;

                string fieldStr = string.Empty;

                while (stream.Position != stream.Length)
                {
                    byte descriptor = reader.ReadByte();

                    if (!isValue)
                    {
                        byte[] key = reader.ReadBytes(descriptor - 0xA0);

                        fieldStr = $"  Key: {Encoding.ASCII.GetString(key)}";

                        isValue = true;
                    }
                    else
                    {
                        if (descriptor > 0xD0) // Int value.
                        {
                            if (descriptor - 0xD0 == 1)
                            {
                                fieldStr += $", Value: {BinaryPrimitives.ReverseEndianness(reader.ReadUInt16())}";
                            }
                            else if (descriptor - 0xD0 == 2)
                            {
                                fieldStr += $", Value: {BinaryPrimitives.ReverseEndianness(reader.ReadInt32())}";
                            }
                            else if (descriptor - 0xD0 == 4)
                            {
                                fieldStr += $", Value: {BinaryPrimitives.ReverseEndianness(reader.ReadInt64())}";
                            }
                            else
                            {
                                // Unknown.
                                break;
                            }
                        }
                        else if (descriptor > 0xA0 && descriptor < 0xD0) // String value, max size = 0x20 bytes ?
                        {
                            int    size      = descriptor - 0xA0;
                            string value     = string.Empty;
                            byte[] rawValues = new byte[0];

                            for (int i = 0; i < size; i++)
                            {
                                byte chr = reader.ReadByte();

                                if (chr >= 0x20 && chr < 0x7f)
                                {
                                    value += (char)chr;
                                }
                                else
                                {
                                    Array.Resize(ref rawValues, rawValues.Length + 1);

                                    rawValues[rawValues.Length - 1] = chr;
                                }
                            }

                            if (value != string.Empty)
                            { 
                                fieldStr += $", Value: {value}";
                            }

                            // TODO(Ac_K): Determine why there are non-alphanumeric values sometimes.
                            if (rawValues.Length > 0)
                            {
                                fieldStr += $", RawValue: 0x{BitConverter.ToString(rawValues).Replace("-", "")}";
                            }
                        }
                        else // Byte value.
                        {
                            fieldStr += $", Value: {descriptor}";
                        }

                        sb.AppendLine(fieldStr);

                        isValue = false;
                    }
                }
            }

            return sb.ToString();
        }
    }
}
