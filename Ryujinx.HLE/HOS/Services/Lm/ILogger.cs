using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
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
            byte[] LogBuffer = Context.Memory.ReadBytes(
                Context.Request.PtrBuff[0].Position,
                Context.Request.PtrBuff[0].Size);

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

                    if (Field == LmLogField.Skip)
                    {
                        Reader.ReadByte();

                        continue;
                    }
                    else if (Field == LmLogField.Line)
                    {
                        FieldStr = Field + ": " + Reader.ReadInt32();
                    }
                    else
                    {
                        FieldStr = Field + ": \"" + Encoding.UTF8.GetString(Reader.ReadBytes(Size)) + "\"";
                    }

                    SB.AppendLine(" " + FieldStr);
                }

                string Text = SB.ToString();

                switch((LmLogLevel)Level)
                {
                    case LmLogLevel.Trace:    Context.Device.Log.PrintDebug  (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Info:     Context.Device.Log.PrintInfo   (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Warning:  Context.Device.Log.PrintWarning(LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Error:    Context.Device.Log.PrintError  (LogClass.ServiceLm, Text); break;
                    case LmLogLevel.Critical: Context.Device.Log.PrintError  (LogClass.ServiceLm, Text); break;
                }
            }

            return 0;
        }
    }
}
