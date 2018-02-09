using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx
{
    public static class Config
    {
        public static bool LoggingEnableInfo { get; private set; }
        public static bool LoggingEnableTrace { get; private set; }
        public static bool LoggingEnableDebug { get; private set; }
        public static bool LoggingEnableWarn { get; private set; }
        public static bool LoggingEnableError { get; private set; }
        public static bool LoggingEnableFatal { get; private set; }
        public static bool LoggingEnableLogFile { get; private set; }

        public static void Read()
        {
            IniParser Parser = new IniParser(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Ryujinx.conf"));

            LoggingEnableInfo    = Convert.ToBoolean(Parser.Value("Logging_Enable_Info"));
            LoggingEnableTrace   = Convert.ToBoolean(Parser.Value("Logging_Enable_Trace"));
            LoggingEnableDebug   = Convert.ToBoolean(Parser.Value("Logging_Enable_Debug"));
            LoggingEnableWarn    = Convert.ToBoolean(Parser.Value("Logging_Enable_Warn"));
            LoggingEnableError   = Convert.ToBoolean(Parser.Value("Logging_Enable_Error"));
            LoggingEnableFatal   = Convert.ToBoolean(Parser.Value("Logging_Enable_Fatal"));
            LoggingEnableLogFile = Convert.ToBoolean(Parser.Value("Logging_Enable_LogFile"));
        }
    }

    // https://stackoverflow.com/a/37772571
    public class IniParser
    {
        private Dictionary<string, string> Values;

        public IniParser(string Path)
        {
            Values = File.ReadLines(Path)
            .Where(Line => (!String.IsNullOrWhiteSpace(Line) && !Line.StartsWith("#")))
            .Select(Line => Line.Split(new char[] { '=' }, 2, 0))
            .ToDictionary(Parts => Parts[0].Trim(), Parts => Parts.Length > 1 ? Parts[1].Trim() : null);
        }

        public string Value(string Name, string Value = null)
        {
            if (Values != null && Values.ContainsKey(Name))
            {
                return Values[Name];
            }
            return Value;
        }
    }
}
