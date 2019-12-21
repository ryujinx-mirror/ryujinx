using System;
using System.IO;
using System.Text;

namespace Ryujinx.Common.Logging
{
    public class FileLogTarget : ILogTarget
    {
        private static readonly ObjectPool<StringBuilder> _stringBuilderPool = SharedPools.Default<StringBuilder>();

        private readonly StreamWriter  _logWriter;
        private readonly ILogFormatter _formatter;
        private readonly string        _name;

        string ILogTarget.Name { get => _name; }

        public FileLogTarget(string path, string name)
            : this(path, name, FileShare.Read, FileMode.Append)
        { }

        public FileLogTarget(string path, string name, FileShare fileShare, FileMode fileMode)
        {
            _name      = name;
            _logWriter = new StreamWriter(File.Open(path, fileMode, FileAccess.Write, fileShare));
            _formatter = new DefaultLogFormatter();
        }

        public void Log(object sender, LogEventArgs args)
        {
            _logWriter.WriteLine(_formatter.Format(args));
            _logWriter.Flush();
        }

        public void Dispose()
        {
            _logWriter.WriteLine("---- End of Log ----");
            _logWriter.Flush();
            _logWriter.Dispose();
        }
    }
}
