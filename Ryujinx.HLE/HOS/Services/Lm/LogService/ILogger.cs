using Ryujinx.Common.Logging;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Lm.LogService
{
    class ILogger : IpcService
    {
        public ILogger() { }

        [Command(0)]
        // Log(buffer<unknown, 0x21>)
        public ResultCode Log(ServiceCtx context)
        {
            (long bufPos, long bufSize) = context.Request.GetBufferType0x21();
            byte[] logBuffer = context.Memory.ReadBytes(bufPos, bufSize);

            using (MemoryStream ms = new MemoryStream(logBuffer))
            {
                BinaryReader reader = new BinaryReader(ms);

                long  pid           = reader.ReadInt64();
                long  threadContext = reader.ReadInt64();
                short flags         = reader.ReadInt16();
                byte  level         = reader.ReadByte();
                byte  verbosity     = reader.ReadByte();
                int   payloadLength = reader.ReadInt32();

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Guest log:");

                sb.AppendLine($" Log level: {(LmLogLevel)level}");

                while (ms.Position < ms.Length)
                {
                    byte type = reader.ReadByte();
                    byte size = reader.ReadByte();

                    LmLogField field = (LmLogField)type;

                    string fieldStr = string.Empty;

                    if (field == LmLogField.Start)
                    {
                        reader.ReadBytes(size);

                        continue;
                    }
                    else if (field == LmLogField.Stop)
                    {
                        break;
                    }
                    else if (field == LmLogField.Line)
                    {
                        fieldStr = $"{field}: {reader.ReadInt32()}";
                    }
                    else if (field == LmLogField.DropCount)
                    {
                        fieldStr = $"{field}: {reader.ReadInt64()}";
                    }
                    else if (field == LmLogField.Time)
                    {
                        fieldStr = $"{field}: {reader.ReadInt64()}s";
                    }
                    else if (field < LmLogField.Count)
                    {
                        fieldStr = $"{field}: '{Encoding.UTF8.GetString(reader.ReadBytes(size)).TrimEnd()}'";
                    }
                    else
                    {
                        fieldStr = $"Field{field}: '{Encoding.UTF8.GetString(reader.ReadBytes(size)).TrimEnd()}'";
                    }

                    sb.AppendLine(" " + fieldStr);
                }

                string text = sb.ToString();

                Logger.PrintGuest(LogClass.ServiceLm, text);
            }

            return ResultCode.Success;
        }
    }
}