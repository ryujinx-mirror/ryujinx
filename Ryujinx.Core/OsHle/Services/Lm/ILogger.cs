using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.Core.OsHle.IpcServices.Lm
{
    class ILogger : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ILogger()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Log }
            };
        }

        enum Flags
        {
            Padding,
            IsHead,
            IsTail
        }

        enum Severity
        {
            Trace,
            Info,
            Warning,
            Error,
            Critical
        }

        enum Field
        {
            Padding,
            Skip,
            Message,
            Line,
            Filename,
            Function,
            Module,
            Thread
        }

        public long Log(ServiceCtx Context)
        {
            long BufferPosition = Context.Request.PtrBuff[0].Position;
            long BufferLen      = Context.Request.PtrBuff[0].Size;

            byte[] LogBuffer = AMemoryHelper.ReadBytes(Context.Memory, BufferPosition, (int)BufferLen);

            MemoryStream LogMessage = new MemoryStream(LogBuffer);
            BinaryReader bReader = new BinaryReader(LogMessage);

            //Header reading.
            long Pid       = bReader.ReadInt64();
            long ThreadCxt = bReader.ReadInt64();
            int Infos      = bReader.ReadInt32();
            int PayloadLen = bReader.ReadInt32();

            int iFlags     = Infos & 0xFFFF;
            int iSeverity  = (Infos >> 17) & 0x7F;
            int iVerbosity = (Infos >> 25) & 0x7F;

            //ToDo: For now we don't care about Head or Tail Log.
            bool IsHeadLog = Convert.ToBoolean(iFlags & (int)Flags.IsHead);
            bool IsTailLog = Convert.ToBoolean(iFlags & (int)Flags.IsTail);

            string LogString = "nn::diag::detail::LogImpl()" + Environment.NewLine + Environment.NewLine +
                               "Header:" + Environment.NewLine +
                               $"   Pid: {Pid}" + Environment.NewLine +
                               $"   ThreadContext: {ThreadCxt}" + Environment.NewLine +
                               $"   Flags: {IsHeadLog}/{IsTailLog}" + Environment.NewLine +
                               $"   Severity: {Enum.GetName(typeof(Severity), iSeverity)}" + Environment.NewLine +
                               $"   Verbosity: {iVerbosity}";

            LogString += Environment.NewLine + Environment.NewLine + "Message:" + Environment.NewLine;

            string StrMessage = "", StrLine = "", StrFilename = "", StrFunction = "",
                   StrModule = "", StrThread = "";

            do
            {
                byte FieldType = bReader.ReadByte();
                byte FieldSize = bReader.ReadByte();

                if ((Field)FieldType != Field.Skip || FieldSize != 0)
                {
                    byte[] Message = bReader.ReadBytes(FieldSize);
                    switch ((Field)FieldType)
                    {
                        case Field.Message:
                            StrMessage = Encoding.UTF8.GetString(Message);
                            break;

                        case Field.Line:
                            StrLine = BitConverter.ToInt32(Message, 0).ToString();
                            break;

                        case Field.Filename:
                            StrFilename = Encoding.UTF8.GetString(Message);
                            break;

                        case Field.Function:
                            StrFunction = Encoding.UTF8.GetString(Message);
                            break;

                        case Field.Module:
                            StrModule = Encoding.UTF8.GetString(Message);
                            break;

                        case Field.Thread:
                            StrThread = Encoding.UTF8.GetString(Message);
                            break;
                    }
                }
                
            }
            while (LogMessage.Position != PayloadLen + 0x18); // 0x18 - Size of Header LogMessage.

            LogString += StrModule + " > " + StrThread + ": " + StrFilename + "@" + StrFunction + "(" + StrLine + ") '" + StrMessage + "'" + Environment.NewLine;

            switch((Severity)iSeverity)
            {
                case Severity.Trace:    Logging.Trace(LogString); break;
                case Severity.Info:     Logging.Info(LogString);  break;
                case Severity.Warning:  Logging.Warn(LogString);  break;
                case Severity.Error:    Logging.Error(LogString); break;
                case Severity.Critical: Logging.Fatal(LogString); break;
            }

            return 0;
        }
    }
}
 