using System;

namespace Ryujinx.Common.Logging
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel Level { get; private set; }
        public TimeSpan Time  { get; private set; }

        public string Message { get; private set; }

        public LogEventArgs(LogLevel Level, TimeSpan Time, string Message)
        {
            this.Level   = Level;
            this.Time    = Time;
            this.Message = Message;
        }
    }
}