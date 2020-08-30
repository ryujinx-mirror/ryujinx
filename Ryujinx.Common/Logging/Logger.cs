using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.Common.Logging
{
    public static class Logger
    {
        private static readonly Stopwatch m_Time;

        private static readonly bool[] m_EnabledClasses;

        private static readonly List<ILogTarget> m_LogTargets;

        public static event EventHandler<LogEventArgs> Updated;

        public struct Log
        {
            internal readonly LogLevel Level;

            internal Log(LogLevel level)
            {
                Level = level;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PrintMsg(LogClass logClass, string message)
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, "", message)));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Print(LogClass logClass, string message, [CallerMemberName] string caller = "")
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, caller, message)));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Print(LogClass logClass, string message, object data, [CallerMemberName] string caller = "")
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, caller, message), data));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PrintStub(LogClass logClass, string message = "", [CallerMemberName] string caller = "")
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, caller, "Stubbed. " + message)));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PrintStub(LogClass logClass, object data, [CallerMemberName] string caller = "")
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, caller, "Stubbed."), data));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PrintStub(LogClass logClass, string message, object data, [CallerMemberName] string caller = "")
            {
                if (m_EnabledClasses[(int)logClass])
                {
                    Updated?.Invoke(null, new LogEventArgs(Level, m_Time.Elapsed, Thread.CurrentThread.Name, FormatMessage(logClass, caller, "Stubbed. " + message), data));
                }
            }            

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static string FormatMessage(LogClass Class, string Caller, string Message) => $"{Class} {Caller}: {Message}";
        }

        public static Log? Debug     { get; private set; }
        public static Log? Info      { get; private set; }
        public static Log? Warning   { get; private set; }
        public static Log? Error     { get; private set; }
        public static Log? Guest     { get; private set; }
        public static Log? AccessLog { get; private set; }
        public static Log? Stub      { get; private set; }
        public static Log  Notice    { get; } // Always enabled

        static Logger()
        {
            m_EnabledClasses = new bool[Enum.GetNames(typeof(LogClass)).Length];

            for (int index = 0; index < m_EnabledClasses.Length; index++)
            {
                m_EnabledClasses[index] = true;
            }

            m_LogTargets = new List<ILogTarget>();

            m_Time = Stopwatch.StartNew();

            // Logger should log to console by default
            AddTarget(new AsyncLogTargetWrapper(
                new ConsoleLogTarget("console"),
                1000,
                AsyncLogTargetOverflowAction.Discard));

            Notice = new Log(LogLevel.Notice);
            
            // Enable important log levels before configuration is loaded
            Error = new Log(LogLevel.Error);
            Warning = new Log(LogLevel.Warning);
            Info = new Log(LogLevel.Info);
        }

        public static void RestartTime()
        {
            m_Time.Restart();
        }

        private static ILogTarget GetTarget(string targetName)
        {
            foreach (var target in m_LogTargets)
            {
                if (target.Name.Equals(targetName))
                {
                    return target;
                }
            }

            return null;
        }

        public static void AddTarget(ILogTarget target)
        {
            m_LogTargets.Add(target);

            Updated += target.Log;
        }

        public static void RemoveTarget(string target)
        {
            ILogTarget logTarget = GetTarget(target);

            if (logTarget != null)
            {
                Updated -= logTarget.Log;

                m_LogTargets.Remove(logTarget);

                logTarget.Dispose();
            }
        }

        public static void Shutdown()
        {
            Updated = null;

            foreach (var target in m_LogTargets)
            {
                target.Dispose();
            }

            m_LogTargets.Clear();
        }

        public static IReadOnlyCollection<LogLevel> GetEnabledLevels()
        {
            var logs = new Log?[] { Debug, Info, Warning, Error, Guest, AccessLog, Stub };
            List<LogLevel> levels = new List<LogLevel>(logs.Length);
            foreach (var log in logs)
            {
                if (log.HasValue)
                {
                    levels.Add(log.Value.Level);
                }
            }

            return levels;
        }

        public static void SetEnable(LogLevel logLevel, bool enabled)
        {
            switch (logLevel)
            {
                case LogLevel.Debug     : Debug     = enabled ? new Log(LogLevel.Debug)    : new Log?(); break;
                case LogLevel.Info      : Info      = enabled ? new Log(LogLevel.Info)     : new Log?(); break;
                case LogLevel.Warning   : Warning   = enabled ? new Log(LogLevel.Warning)  : new Log?(); break;
                case LogLevel.Error     : Error     = enabled ? new Log(LogLevel.Error)    : new Log?(); break;
                case LogLevel.Guest     : Guest     = enabled ? new Log(LogLevel.Guest)    : new Log?(); break;
                case LogLevel.AccessLog : AccessLog = enabled ? new Log(LogLevel.AccessLog): new Log?(); break;
                case LogLevel.Stub      : Stub      = enabled ? new Log(LogLevel.Stub)     : new Log?(); break;
                default: throw new ArgumentException("Unknown Log Level");
            }
        }

        public static void SetEnable(LogClass logClass, bool enabled)
        {
            m_EnabledClasses[(int)logClass] = enabled;
        }
    }
}
