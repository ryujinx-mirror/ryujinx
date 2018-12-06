using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Lm
{
    class ILogger : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ILogger()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Log }
            };
        }

        public long Log(ServiceCtx context)
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

                switch((LmLogLevel)level)
                {
                    case LmLogLevel.Trace:    Logger.PrintDebug  (LogClass.ServiceLm, text); break;
                    case LmLogLevel.Info:     Logger.PrintInfo   (LogClass.ServiceLm, text); break;
                    case LmLogLevel.Warning:  Logger.PrintWarning(LogClass.ServiceLm, text); break;
                    case LmLogLevel.Error:    Logger.PrintError  (LogClass.ServiceLm, text); break;
                    case LmLogLevel.Critical: Logger.PrintError  (LogClass.ServiceLm, text); break;
                }
            }

            return 0;
        }
    }
}
