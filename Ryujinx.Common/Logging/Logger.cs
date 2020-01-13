using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.Common.Logging
{
    public static class Logger
    {
        private static Stopwatch m_Time;

        private static readonly bool[] m_EnabledLevels;
        private static readonly bool[] m_EnabledClasses;

        private static readonly List<ILogTarget> m_LogTargets;

        public static event EventHandler<LogEventArgs> Updated;

        static Logger()
        {
            m_EnabledLevels  = new bool[Enum.GetNames(typeof(LogLevel)).Length];
            m_EnabledClasses = new bool[Enum.GetNames(typeof(LogClass)).Length];

            m_EnabledLevels[(int)LogLevel.Stub]      = true;
            m_EnabledLevels[(int)LogLevel.Info]      = true;
            m_EnabledLevels[(int)LogLevel.Warning]   = true;
            m_EnabledLevels[(int)LogLevel.Error]     = true;
            m_EnabledLevels[(int)LogLevel.Guest]     = true;
            m_EnabledLevels[(int)LogLevel.AccessLog] = true;

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
                AsyncLogTargetOverflowAction.Block));
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

            foreach(var target in m_LogTargets)
            {
                target.Dispose();
            }

            m_LogTargets.Clear();
        }

        public static void SetEnable(LogLevel logLevel, bool enabled)
        {
            m_EnabledLevels[(int)logLevel] = enabled;
        }

        public static void SetEnable(LogClass logClass, bool enabled)
        {
            m_EnabledClasses[(int)logClass] = enabled;
        }

        public static void PrintDebug(LogClass logClass, string message, [CallerMemberName] string caller = "")
        {
            Print(LogLevel.Debug, logClass, GetFormattedMessage(logClass, message, caller));
        }

        public static void PrintInfo(LogClass logClass, string message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Info, logClass, GetFormattedMessage(logClass, message, Caller));
        }

        public static void PrintWarning(LogClass logClass, string message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Warning, logClass, GetFormattedMessage(logClass, message, Caller));
        }

        public static void PrintError(LogClass logClass, string message, [CallerMemberName] string Caller = "")
        {
            Print(LogLevel.Error, logClass, GetFormattedMessage(logClass, message, Caller));
        }

        public static void PrintStub(LogClass logClass, string message = "", [CallerMemberName] string caller = "")
        {
            Print(LogLevel.Stub, logClass, GetFormattedMessage(logClass, "Stubbed. " + message, caller));
        }

        public static void PrintStub<T>(LogClass logClass, T obj, [CallerMemberName] string caller = "")
        {
            Print(LogLevel.Stub, logClass, GetFormattedMessage(logClass, "Stubbed.", caller), obj);
        }

        public static void PrintStub<T>(LogClass logClass, string message, T obj, [CallerMemberName] string caller = "")
        {
            Print(LogLevel.Stub, logClass, GetFormattedMessage(logClass, "Stubbed. " + message, caller), obj);
        }

        public static void PrintGuest(LogClass logClass, string message, [CallerMemberName] string caller = "")
        {
            Print(LogLevel.Guest, logClass, GetFormattedMessage(logClass, message, caller));
        }

        public static void PrintAccessLog(LogClass logClass, string message)
        {
            Print(LogLevel.AccessLog, logClass, message);
        }

        private static void Print(LogLevel logLevel, LogClass logClass, string message)
        {
            if (m_EnabledLevels[(int)logLevel] && m_EnabledClasses[(int)logClass])
            {
                Updated?.Invoke(null, new LogEventArgs(logLevel, m_Time.Elapsed, Thread.CurrentThread.Name, message));
            }
        }

        private static void Print(LogLevel logLevel, LogClass logClass, string message, object data)
        {
            if (m_EnabledLevels[(int)logLevel] && m_EnabledClasses[(int)logClass])
            {
                Updated?.Invoke(null, new LogEventArgs(logLevel, m_Time.Elapsed, Thread.CurrentThread.Name, message, data));
            }
        }

        private static string GetFormattedMessage(LogClass Class, string Message, string Caller)
        {
            return $"{Class} {Caller}: {Message}";
        }
    }
}
