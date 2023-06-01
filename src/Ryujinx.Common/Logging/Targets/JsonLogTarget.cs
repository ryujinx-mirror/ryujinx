using Ryujinx.Common.Utilities;
using System.IO;

namespace Ryujinx.Common.Logging.Targets
{
    public class JsonLogTarget : ILogTarget
    {
        private Stream _stream;
        private bool   _leaveOpen;
        private string _name;

        string ILogTarget.Name { get => _name; }

        public JsonLogTarget(Stream stream, string name)
        {
            _stream = stream;
            _name   = name;
        }

        public JsonLogTarget(Stream stream, bool leaveOpen)
        {
            _stream    = stream;
            _leaveOpen = leaveOpen;
        }

        public void Log(object sender, LogEventArgs e)
        {
            var logEventArgsJson = LogEventArgsJson.FromLogEventArgs(e);
            JsonHelper.SerializeToStream(_stream, logEventArgsJson, LogEventJsonSerializerContext.Default.LogEventArgsJson);
        }

        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }
        }
    }
}
