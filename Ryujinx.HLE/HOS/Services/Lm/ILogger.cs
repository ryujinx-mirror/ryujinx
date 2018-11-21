using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Lm
{
    class ILogger : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ILogger()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Log }
            };
        }

        public long Log(ServiceCtx Context)
        {

            (long BufPos, long BufSize) = Context.Request.GetBufferType0x21();
            byte[] LogBuffer = Context.Memory.ReadBytes(BufPos, BufSize);

            using (MemoryStream MS = new MemoryStream(LogBuffer))
            {
                BinaryReader Reader = new BinaryReader(MS);

                long  Pid           = Reader.ReadInt64();
                long  ThreadContext = Reader.ReadInt64();
                short Flags         = Reader.ReadInt16();
                byte  Level         = Reader.ReadByte();
                byte  Verbosity     = Reader.ReadByte();
                int   PayloadLength = Reader.ReadInt32();

                StringBuilder SB = new StringBuilder();

                SB.AppendLine("Guest log:");

                while (MS.Position < MS.Length)
                {
                    byte Type = Reader.ReadByte();
                    byte Size = Reader.ReadByte();

                    LmLogField Field = (LmLogField)Type;

                    string FieldStr = string.Empty;

                    if (Field == LmLogField.Start)
                    {
                        Reader.ReadBytes(Size);

                        continue;
                    }
                    else if (Field == LmLogField.Stop)
                    {
                        break;
                    }
                    else if (Field == LmLogField.Line)
                    {
                        FieldStr = $"{Field}: {Reader.ReadInt32()}";
                    }
                    else if (Field == LmLogField.DropCount)
                    {
                        FieldStr = $"{Field}: {Reader.ReadInt64()}";
                    }
                    else if (Field == LmLogField.Time)
                    {
                        FieldStr = $"{Field}: {Reader.ReadInt64()}s";
                    }
                    else if (Field < LmLogField.Count)
                    {
                        FieldStr = $"{Field}: '{Encoding.UTF8.GetString(Reader.ReadBytes(Size)).TrimEnd()}'";
                    }
                    else
                    {
                        FieldStr = $"Field{Field}: '{Encoding.UTF8.GetString(Reader.ReadBytes(Size)).TrimEnd()}'";
                    }

                    SB.AppendLine(" " + FieldStr);
                }

                string Text = SB.ToString();

                switch((LmLogLevel)Level)
                {
                    case LmLogLevel.Trace:    Logger.PrintDebug  (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Info:     Logger.PrintInfo   (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Warning:  Logger.PrintWarning(LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Error:    Logger.PrintError  (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Critical: Logger.PrintError  (LogClass.ServiceLm, Text); break;
                }
            }

            return 0;
        }
    }
}
