using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Common.Logging
{
    public static class Logger
    {
        private static bool[] EnabledLevels;
        private static bool[] EnabledClasses;

        public static event EventHandler<LogEventArgs> Updated;

        private static Stopwatch Time;

        static Logger()
        {
            EnabledLevels  = new bool[Enum.GetNames(typeof(LogLevel)).Length];
            EnabledClasses = new bool[Enum.GetNames(typeof(LogClass)).Length];

            EnabledLevels[(int)LogLevel.Stub]    = true;
            EnabledLevels[(int)LogLevel.Info]    = true;
            EnabledLevels[(int)LogLevel.Warning] = true;
            EnabledLevels[(int)LogLevel.Error]   = true;

            for (int Index = 0; Index < EnabledClasses.Length; Index++)
            {
                EnabledClasses[Index] = true;
            }

            Time = new Stopwatch();

            Time.Start();
        }

        public static void SetEnable(LogLevel Level, bool Enabled)
        {
            EnabledLevels[(int)Level] = Enabled;
        }

        public static void SetEnable(LogClass Class, bool Enabled)
        {
            EnabledClasses[(int)Class] = Enabled;
        }

        public static void PrintDebug(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Debug, Class, GetFormattedMessage(Class, Message, Caller));
        }

        public static void PrintStub(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Stub, Class, GetFormattedMessage(Class, Message, Caller));
        }

        public static void PrintInfo(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Info, Class, GetFormattedMessage(Class, Message, Caller));
        }

        public static void PrintWarning(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Warning, Class, GetFormattedMessage(Class, Message, Caller));
        }

        public static void PrintError(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Error, Class, GetFormattedMessage(Class, Message, Caller));
        }

        private static void Print(LogLevel Level, LogClass Class, string Message)
        {
            if (EnabledLevels[(int)Level] && EnabledClasses[(int)Class])
            {
                Updated?.Invoke(null, new LogEventArgs(Level, Time.Elapsed, Message));
            }
        }

        private static string GetFormattedMessage(LogClass Class, string Message, string Caller)
        {
            return $"{Class} {Caller}: {Message}";
        }
    }
}