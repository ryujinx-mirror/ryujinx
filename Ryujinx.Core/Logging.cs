using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Ryujinx.Core
{
    public static class Logging
    {
        private static Stopwatch ExecutionTime;

        private const string LogFileName = "Ryujinx.log";

        private static bool   EnableInfo         = Config.LoggingEnableInfo;
        private static bool   EnableTrace        = Config.LoggingEnableTrace;
        private static bool   EnableDebug        = Config.LoggingEnableDebug;
        private static bool   EnableWarn         = Config.LoggingEnableWarn;
        private static bool   EnableError        = Config.LoggingEnableError;
        private static bool   EnableFatal        = Config.LoggingEnableFatal;
        private static bool   EnableStub         = Config.LoggingEnableStub;
        private static bool   EnableIpc          = Config.LoggingEnableIpc;
        private static bool   EnableFilter       = Config.LoggingEnableFilter;
        private static bool   EnableLogFile      = Config.LoggingEnableLogFile;
        private static bool[] FilteredLogClasses = Config.LoggingFilteredClasses;

        private enum LogLevel
        {
            Debug,
            Error,
            Fatal,
            Info,
            Stub,
            Trace,
            Warn
        }

        static Logging()
        {
            if (File.Exists(LogFileName)) File.Delete(LogFileName);

            ExecutionTime = new Stopwatch();

            ExecutionTime.Start();
        }

        public static string GetExecutionTime() => ExecutionTime.ElapsedMilliseconds.ToString().PadLeft(8, '0') + "ms";

        private static void LogMessage(LogEntry LogEntry)
        {
            if (EnableFilter)
                if (!FilteredLogClasses[(int)LogEntry.LogClass])
                    return;

            ConsoleColor consoleColor = ConsoleColor.White;

            switch (LogEntry.LogLevel)
            {
                case LogLevel.Debug:
                    consoleColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Error:
                    consoleColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    consoleColor = ConsoleColor.Magenta;
                    break;
                case LogLevel.Info:
                    consoleColor = ConsoleColor.White;
                    break;
                case LogLevel.Stub:
                    consoleColor = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.Trace:
                    consoleColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Warn:
                    consoleColor = ConsoleColor.Yellow;
                    break;
            }

            LogEntry.ManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            string Text = $"{LogEntry.ExecutionTime} | {LogEntry.ManagedThreadId} > {LogEntry.LogClass} > " +
                $"{LogEntry.LogLevel.ToString()}  > {LogEntry.CallingMember} > {LogEntry.Message}";

            Console.ForegroundColor = consoleColor;
            Console.WriteLine(Text.PadLeft(Text.Length + 1, ' '));
            Console.ResetColor();

            LogFile(Text);
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

        public static void Info(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableInfo)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Info,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Trace(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableTrace)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Trace,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Stub(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableStub)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Stub,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Debug(LogClass LogClass,string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableDebug)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Debug,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Warn(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableWarn)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Warn,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Error(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableError)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Error,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Fatal(LogClass LogClass, string Message, [CallerMemberName] string CallingMember = "")
        {
            if (EnableFatal)
            {
                LogMessage(new LogEntry
                {
                    CallingMember = CallingMember,
                    LogLevel      = LogLevel.Fatal,
                    LogClass      = LogClass,
                    Message       = Message,
                    ExecutionTime = GetExecutionTime()
                });
            }
        }

        public static void Ipc(byte[] Data, long CmdPtr, bool Domain)
        {
            if (EnableIpc)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(IpcLog.Message(Data, CmdPtr, Domain));
                Console.ResetColor();
            }
        }

        //https://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }

                result.Append(line);
            }
            return result.ToString();
        }

        private struct LogEntry
        {
            public string   CallingMember;
            public string   ExecutionTime;
            public string   Message;
            public int      ManagedThreadId;
            public LogClass LogClass;
            public LogLevel LogLevel;
        }
    }
}
