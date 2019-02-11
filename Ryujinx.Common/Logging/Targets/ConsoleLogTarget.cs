using System;
using System.Collections.Concurrent;

namespace Ryujinx.Common.Logging
{
    public class ConsoleLogTarget : ILogTarget
    {
        private static readonly ConcurrentDictionary<LogLevel, ConsoleColor> _logColors;

        private readonly ILogFormatter _formatter;

        static ConsoleLogTarget()
        {
            _logColors = new ConcurrentDictionary<LogLevel, ConsoleColor> {
                [ LogLevel.Stub    ] = ConsoleColor.DarkGray,
                [ LogLevel.Info    ] = ConsoleColor.White,
                [ LogLevel.Warning ] = ConsoleColor.Yellow,
                [ LogLevel.Error   ] = ConsoleColor.Red
            };
        }

        public ConsoleLogTarget()
        {
            _formatter = new DefaultLogFormatter();
        }

        public void Log(object sender, LogEventArgs args)
        {
            if (_logColors.TryGetValue(args.Level, out ConsoleColor color))
            {
                Console.ForegroundColor = color;

                Console.WriteLine(_formatter.Format(args));

                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(_formatter.Format(args));
            }
        }

        public void Dispose()
        {
            Console.ResetColor();
        }
    }
}
