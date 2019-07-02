using System;

namespace Ryujinx.Common.Logging
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel Level    { get; private set; }
        public TimeSpan Time     { get; private set; }
        public int      ThreadId { get; private set; }

        public string Message { get; private set; }
        public object Data    { get; private set; }

        public LogEventArgs(LogLevel level, TimeSpan time, int threadId, string message)
        {
            Level    = level;
            Time     = time;
            ThreadId = threadId;
            Message  = message;
        }

        public LogEventArgs(LogLevel level, TimeSpan time, int threadId, string message, object data)
        {
            Level    = level;
            Time     = time;
            ThreadId = threadId;
            Message  = message;
            Data     = data;
        }
    }
}