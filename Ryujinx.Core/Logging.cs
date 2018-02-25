using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ryujinx.Core
{
    public static class Logging
    {
        private static Stopwatch ExecutionTime = new Stopwatch();
        private const string LogFileName = "Ryujinx.log";

        private static bool EnableInfo    = Config.LoggingEnableInfo;
        private static bool EnableTrace   = Config.LoggingEnableTrace;
        private static bool EnableDebug   = Config.LoggingEnableDebug;
        private static bool EnableWarn    = Config.LoggingEnableWarn;
        private static bool EnableError   = Config.LoggingEnableError;
        private static bool EnableFatal   = Config.LoggingEnableFatal;
        private static bool EnableIpc     = Config.LoggingEnableIpc;
        private static bool EnableLogFile = Config.LoggingEnableLogFile;

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
    }
}
