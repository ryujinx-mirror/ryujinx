using System.IO;
using Utf8Json;

namespace Ryujinx.Common.Logging
{
    public class JsonLogTarget : ILogTarget
    {
        private Stream _stream;
        private bool   _leaveOpen;

        public JsonLogTarget(Stream stream)
        {
            _stream = stream;
        }

        public JsonLogTarget(Stream stream, bool leaveOpen)
        {
            _stream    = stream;
            _leaveOpen = leaveOpen;
        }

        public void Log(object sender, LogEventArgs e)
        {
            JsonSerializer.Serialize(_stream, e);
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
