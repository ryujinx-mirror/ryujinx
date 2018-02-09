using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ryujinx
{
    public static class Logging
    {
        private static Stopwatch ExecutionTime = new Stopwatch();
        private static string LogFileName = "Ryujinx.log";

        public static bool EnableInfo = Config.LoggingEnableInfo;
        public static bool EnableTrace = Config.LoggingEnableTrace;
        public static bool EnableDebug = Config.LoggingEnableDebug;
        public static bool EnableWarn = Config.LoggingEnableWarn;
        public static bool EnableError = Config.LoggingEnableError;
        public static bool EnableFatal = Config.LoggingEnableFatal;
        public static bool EnableLogFile = Config.LoggingEnableLogFile;

        static Logging()
        {
            ExecutionTime.Start();

            if (File.Exists(LogFileName)) File.Delete(LogFileName);
        }

        public static string GetExecutionTime()
        {
            return ExecutionTime.ElapsedMilliseconds.ToString().PadLeft(8, '0') + "ms";
        }

        private static string WhoCalledMe()
        {
            return new StackTrace().GetFrame(2).GetMethod().Name;
        }

        private static void LogFile(string Message)
        {
            if (EnableLogFile)
            {
                using (StreamWriter Writer = File.AppendText(LogFileName))
                {
                    Writer.WriteLine(Message);
                }
            }
        }

        public static void Info(string Message)
        {
            if (EnableInfo)
            {
                string Text = $"{GetExecutionTime()} | INFO  > {Message}";

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }

        public static void Trace(string Message)
        {
            if (EnableTrace)
            {
                string Text = $"{GetExecutionTime()} | TRACE > {WhoCalledMe()} - {Message}";

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }

        public static void Debug(string Message)
        {
            if (EnableDebug)
            {
                string Text = $"{GetExecutionTime()} | DEBUG > {WhoCalledMe()} - {Message}";

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }

        public static void Warn(string Message)
        {
            if (EnableWarn)
            {
                string Text = $"{GetExecutionTime()} | WARN  > {WhoCalledMe()} - {Message}";

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }

        public static void Error(string Message)
        {
            if (EnableError)
            {
                string Text = $"{GetExecutionTime()} | ERROR > {WhoCalledMe()} - {Message}";

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }

        public static void Fatal(string Message)
        {
            if (EnableFatal)
            {
                string Text = $"{GetExecutionTime()} | FATAL > {WhoCalledMe()} - {Message}";

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
                Console.ResetColor();

                LogFile(Text);
            }
        }
    }
}
