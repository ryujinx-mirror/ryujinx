using System;

namespace Ryujinx.Common.Logging
{
    public class LogEventArgs : EventArgs
    {
        public readonly LogLevel Level;
        public readonly TimeSpan Time;
        public readonly string   ThreadName;

        public readonly string Message;
        public readonly object Data;

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