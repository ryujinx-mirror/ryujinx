using Ryujinx.Common.Logging.Formatters;
using System;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Logging
{
    internal class LogEventArgsJson
    {
        public LogLevel Level { get; }
        public TimeSpan Time { get; }
        public string ThreadName { get; }

        public string Message { get; }
        public string Data { get; }

        [JsonConstructor]
        public LogEventArgsJson(LogLevel level, TimeSpan time, string threadName, string message, string data = null)
        {
            Level = level;
            Time = time;
            ThreadName = threadName;
            Message = message;
            Data = data;
        }

        public static LogEventArgsJson FromLogEventArgs(LogEventArgs args)
        {
            return new LogEventArgsJson(args.Level, args.Time, args.ThreadName, args.Message, DynamicObjectFormatter.Format(args.Data));
        }
    }
}
