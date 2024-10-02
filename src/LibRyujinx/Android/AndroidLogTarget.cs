using Ryujinx.Common.Logging;
using Ryujinx.Common.Logging.Formatters;
using Ryujinx.Common.Logging.Targets;
using System;

namespace LibRyujinx
{
    public class AndroidLogTarget : ILogTarget
    {
        private readonly string _name;
        private readonly DefaultLogFormatter _formatter;

        string ILogTarget.Name { get => _name; }

        public AndroidLogTarget(string name)
        {
            _name = name;
            _formatter = new DefaultLogFormatter();
        }

        public void Log(object sender, LogEventArgs args)
        {
            Logcat.AndroidLogPrint(GetLogLevel(args.Level), _name, _formatter.Format(args));
        }

        private static Logcat.LogLevel GetLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Debug => Logcat.LogLevel.Debug,
                LogLevel.Stub => Logcat.LogLevel.Info,
                LogLevel.Info => Logcat.LogLevel.Info,
                LogLevel.Warning => Logcat.LogLevel.Warn,
                LogLevel.Error => Logcat.LogLevel.Error,
                LogLevel.Guest => Logcat.LogLevel.Info,
                LogLevel.AccessLog => Logcat.LogLevel.Info,
                LogLevel.Notice => Logcat.LogLevel.Info,
                LogLevel.Trace => Logcat.LogLevel.Verbose,
                _ => throw new NotImplementedException(),

            };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
