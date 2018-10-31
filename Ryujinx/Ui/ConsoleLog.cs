using Ryujinx.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx
{
    static class ConsoleLog
    {
        private static Thread _messageThread;

        private static BlockingCollection<LogEventArgs> _messageQueue;

        private static Dictionary<LogLevel, ConsoleColor> _logColors;

        private static object _consoleLock;

        static ConsoleLog()
        {
            _logColors = new Dictionary<LogLevel, ConsoleColor>()
            {
                { LogLevel.Stub,    ConsoleColor.DarkGray },
                { LogLevel.Info,    ConsoleColor.White    },
                { LogLevel.Warning, ConsoleColor.Yellow   },
                { LogLevel.Error,   ConsoleColor.Red      }
            };

            _messageQueue = new BlockingCollection<LogEventArgs>();

            _consoleLock = new object();

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
            string formattedTime = e.Time.ToString(@"hh\:mm\:ss\.fff");

            string currentThread = Thread.CurrentThread.ManagedThreadId.ToString("d4");
            
            string message = formattedTime + " | " + currentThread + " " + e.Message;

            if (_logColors.TryGetValue(e.Level, out ConsoleColor color))
            {
                lock (_consoleLock)
                {
                    Console.ForegroundColor = color;

                    Console.WriteLine(message);
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine(message);
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