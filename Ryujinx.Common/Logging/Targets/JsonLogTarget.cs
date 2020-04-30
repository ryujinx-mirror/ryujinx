using System.IO;
using System.Text.Json;

namespace Ryujinx.Common.Logging
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
            string text = JsonSerializer.Serialize(e);

            using (BinaryWriter writer = new BinaryWriter(_stream))
            {
                writer.Write(text);
            }
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
