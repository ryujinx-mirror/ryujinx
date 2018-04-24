using Ryujinx.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx
{
    static class ConsoleLog
    {
        private static Dictionary<LogLevel, ConsoleColor> LogColors;

        private static object ConsoleLock;

        static ConsoleLog()
        {
            LogColors = new Dictionary<LogLevel, ConsoleColor>()
            {
                { LogLevel.Stub,    ConsoleColor.DarkGray },
                { LogLevel.Info,    ConsoleColor.White    },
                { LogLevel.Warning, ConsoleColor.Yellow   },
                { LogLevel.Error,   ConsoleColor.Red      }
            };

            ConsoleLock = new object();
        }

        public static void PrintLog(object sender, LogEventArgs e)
        {
            string FormattedTime = e.Time.ToString(@"hh\:mm\:ss\.fff");

            string CurrentThread = Thread.CurrentThread.ManagedThreadId.ToString("d4");

            string Message = FormattedTime + " | " + CurrentThread + " " + e.Message;

            if (LogColors.TryGetValue(e.Level, out ConsoleColor Color))
            {
                lock (ConsoleLock)
                {
                    Console.ForegroundColor = Color;

                    Console.WriteLine(Message);
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine(Message);
            }
        }
    }
}