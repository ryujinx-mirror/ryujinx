using Ryujinx.Horizon.Sdk.Diag;
using System.Text;

namespace Ryujinx.Horizon.LogManager.Types
{
    struct LogPacket
    {
        public string Message;
        public int Line;
        public string Filename;
        public string Function;
        public string Module;
        public string Thread;
        public long DropCount;
        public long Time;
        public string ProgramName;
        public LogSeverity Severity;

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine($"Guest Log:\n  Log level: {Severity}");

            if (Time > 0)
            {
                builder.AppendLine($"    Time: {Time}s");
            }

            if (DropCount > 0)
            {
                builder.AppendLine($"    DropCount: {DropCount}");
            }

            if (!string.IsNullOrEmpty(ProgramName))
            {
                builder.AppendLine($"    ProgramName: {ProgramName}");
            }

            if (!string.IsNullOrEmpty(Module))
            {
                builder.AppendLine($"    Module: {Module}");
            }

            if (!string.IsNullOrEmpty(Thread))
            {
                builder.AppendLine($"    Thread: {Thread}");
            }

            if (!string.IsNullOrEmpty(Filename))
            {
                builder.AppendLine($"    Filename: {Filename}");
            }

            if (Line > 0)
            {
                builder.AppendLine($"    Line: {Line}");
            }

            if (!string.IsNullOrEmpty(Function))
            {
                builder.AppendLine($"    Function: {Function}");
            }

            if (!string.IsNullOrEmpty(Message))
            {
                builder.AppendLine($"    Message: {Message}");
            }

            return builder.ToString();
        }
    }
}
