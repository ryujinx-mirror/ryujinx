using Ryujinx.HLE.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx
{
    static class ConsoleLog
    {
        private static Thread MessageThread;

        private static BlockingCollection<LogEventArgs> MessageQueue;

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

            MessageQueue = new BlockingCollection<LogEventArgs>();

            ConsoleLock = new object();

            MessageThread = new Thread(() =>
            {
                while (!MessageQueue.IsCompleted)
                {
                    try
                    {
                        PrintLog(MessageQueue.Take());
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

            MessageThread.IsBackground = true;
            MessageThread.Start();
        }

        private static void PrintLog(LogEventArgs e)
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

        public static void Log(object sender, LogEventArgs e)
        {
            if (!MessageQueue.IsAddingCompleted)
            {
                MessageQueue.Add(e);
            }
        }
    }
}