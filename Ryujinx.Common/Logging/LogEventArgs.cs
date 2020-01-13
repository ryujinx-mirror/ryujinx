using System;

namespace Ryujinx.Common.Logging
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel Level      { get; private set; }
        public TimeSpan Time       { get; private set; }
        public string   ThreadName { get; private set; }

        public string Message { get; private set; }
        public object Data    { get; private set; }

        public LogEventArgs(LogLevel level, TimeSpan time, string threadName, string message)
        {
            Level      = level;
            Time       = time;
            ThreadName = threadName;
            Message    = message;
        }

        public LogEventArgs(LogLevel level, TimeSpan time, string threadName, string message, object data)
        {
            Level      = level;
            Time       = time;
            ThreadName = threadName;
            Message    = message;
            Data       = data;
        }
    }
}