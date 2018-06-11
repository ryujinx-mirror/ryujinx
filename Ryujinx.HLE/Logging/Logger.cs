using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.Logging
{
    public class Logger
    {
        private bool[] EnabledLevels;
        private bool[] EnabledClasses;

        public event EventHandler<LogEventArgs> Updated;

        private Stopwatch Time;

        public Logger()
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

        public void SetEnable(LogLevel Level, bool Enabled)
        {
            EnabledLevels[(int)Level] = Enabled;
        }

        public void SetEnable(LogClass Class, bool Enabled)
        {
            EnabledClasses[(int)Class] = Enabled;
        }

        internal void PrintDebug(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Debug, Class, GetFormattedMessage(Class, Message, Caller));
        }

        internal void PrintStub(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Stub, Class, GetFormattedMessage(Class, Message, Caller));
        }

        internal void PrintInfo(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Info, Class, GetFormattedMessage(Class, Message, Caller));
        }

        internal void PrintWarning(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Warning, Class, GetFormattedMessage(Class, Message, Caller));
        }

        internal void PrintError(LogClass Class, string Message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Error, Class, GetFormattedMessage(Class, Message, Caller));
        }

        private void Print(LogLevel Level, LogClass Class, string Message)
        {
            if (EnabledLevels[(int)Level] && EnabledClasses[(int)Class])
            {
                Updated?.Invoke(this, new LogEventArgs(Level, Time.Elapsed, Message));
            }
        }

        private string GetFormattedMessage(LogClass Class, string Message, string Caller)
        {
            return $"{Class} {Caller}: {Message}";
        }
    }
}