using Ryujinx.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Ryujinx
{
    static class ConsoleLog
    {
        private static Thread _messageThread;

        private static BlockingCollection<LogEventArgs> _messageQueue;

        private static Dictionary<LogLevel, ConsoleColor> _logColors;

        static ConsoleLog()
        {
            _logColors = new Dictionary<LogLevel, ConsoleColor>()
            {
                { LogLevel.Stub,    ConsoleColor.DarkGray },
                { LogLevel.Info,    ConsoleColor.White    },
                { LogLevel.Warning, ConsoleColor.Yellow   },
                { LogLevel.Error,   ConsoleColor.Red      }
            };

            _messageQueue = new BlockingCollection<LogEventArgs>(10);

            _messageThread = new Thread(() =>
            {
                while (!_messageQueue.IsCompleted)
                {
                    try
                    {
                        PrintLog(_messageQueue.Take());
                    }
                    catch (InvalidOperationException)
                    {
                        // IOE means that Take() was called on a completed collection.
                        // Some other thread can call CompleteAdding after we pass the
                        // IsCompleted check but before we call Take.
                        // We can simply catch the exception since the loop will break
                        // on the next iteration.
                    }
                }
            });

            _messageThread.IsBackground = true;
            _messageThread.Start();
        }

        private static void PrintLog(LogEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"{0:hh\:mm\:ss\.fff}", e.Time);
            sb.Append(" | ");
            sb.AppendFormat("{0:d4}", e.ThreadId);
            sb.Append(' ');
            sb.Append(e.Message);

            if (e.Data != null)
            {
                PropertyInfo[] props = e.Data.GetType().GetProperties();

                sb.Append(' ');

                foreach (var prop in props)
                {
                    sb.Append(prop.Name);
                    sb.Append(": ");
                    sb.Append(prop.GetValue(e.Data));
                    sb.Append(" - ");
                }

                // We remove the final '-' from the string
                if (props.Length > 0)
                {
                    sb.Remove(sb.Length - 3, 3);
                }
            }

            if (_logColors.TryGetValue(e.Level, out ConsoleColor color))
            {
                Console.ForegroundColor = color;

                Console.WriteLine(sb.ToString());

                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(sb.ToString());
            }
        }

        public static void Log(object sender, LogEventArgs e)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                _messageQueue.Add(e);
            }
        }
    }
}