using Ryujinx.Core.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx.Core
{
    public static class Config
    {
        public static bool   EnableMemoryChecks     { get; private set; }
        public static bool   LoggingEnableInfo      { get; private set; }
        public static bool   LoggingEnableTrace     { get; private set; }
        public static bool   LoggingEnableDebug     { get; private set; }
        public static bool   LoggingEnableWarn      { get; private set; }
        public static bool   LoggingEnableError     { get; private set; }
        public static bool   LoggingEnableFatal     { get; private set; }
        public static bool   LoggingEnableIpc       { get; private set; }
        public static bool   LoggingEnableStub      { get; private set; }
        public static bool   LoggingEnableLogFile   { get; private set; }
        public static bool   LoggingEnableFilter    { get; private set; }
        public static bool[] LoggingFilteredClasses { get; private set; }

        public static JoyCon FakeJoyCon { get; private set; }

        public static void Read()
        {
            var iniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var iniPath = Path.Combine(iniFolder, "Ryujinx.conf");
            IniParser Parser = new IniParser(iniPath);

            EnableMemoryChecks     = Convert.ToBoolean(Parser.Value("Enable_Memory_Checks"));
            LoggingEnableInfo      = Convert.ToBoolean(Parser.Value("Logging_Enable_Info"));
            LoggingEnableTrace     = Convert.ToBoolean(Parser.Value("Logging_Enable_Trace"));
            LoggingEnableDebug     = Convert.ToBoolean(Parser.Value("Logging_Enable_Debug"));
            LoggingEnableWarn      = Convert.ToBoolean(Parser.Value("Logging_Enable_Warn"));
            LoggingEnableError     = Convert.ToBoolean(Parser.Value("Logging_Enable_Error"));
            LoggingEnableFatal     = Convert.ToBoolean(Parser.Value("Logging_Enable_Fatal"));
            LoggingEnableIpc       = Convert.ToBoolean(Parser.Value("Logging_Enable_Ipc"));
            LoggingEnableStub      = Convert.ToBoolean(Parser.Value("Logging_Enable_Stub"));
            LoggingEnableLogFile   = Convert.ToBoolean(Parser.Value("Logging_Enable_LogFile"));
            LoggingEnableFilter    = Convert.ToBoolean(Parser.Value("Logging_Enable_Filter"));
            LoggingFilteredClasses = new bool[(int)LogClass.Count];

            string[] FilteredLogClasses = Parser.Value("Logging_Filtered_Classes", string.Empty).Split(',');
            foreach (string LogClass in FilteredLogClasses)
            {
                if (!string.IsNullOrEmpty(LogClass.Trim()))
                {
                    foreach (LogClass EnumItemName in Enum.GetValues(typeof(LogClass)))
                    {
                        if (EnumItemName.ToString().ToLower().Contains(LogClass.Trim().ToLower()))
                        {
                            LoggingFilteredClasses[(int)EnumItemName] = true;
                        }
                    }
                }
            }

            FakeJoyCon = new JoyCon
            {
                Left = new JoyConLeft
                {
                    StickUp     = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Stick_Up")),
                    StickDown   = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Stick_Down")),
                    StickLeft   = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Stick_Left")),
                    StickRight  = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Stick_Right")),
                    StickButton = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Stick_Button")),
                    DPadUp      = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_DPad_Up")),
                    DPadDown    = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_DPad_Down")),
                    DPadLeft    = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_DPad_Left")),
                    DPadRight   = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_DPad_Right")),
                    ButtonMinus = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Button_Minus")),
                    ButtonL     = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Button_L")),
                    ButtonZL    = Convert.ToInt16(Parser.Value("Controls_Left_FakeJoycon_Button_ZL"))
                },

                Right = new JoyConRight
                {
                    StickUp     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Stick_Up")),
                    StickDown   = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Stick_Down")),
                    StickLeft   = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Stick_Left")),
                    StickRight  = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Stick_Right")),
                    StickButton = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Stick_Button")),
                    ButtonA     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_A")),
                    ButtonB     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_B")),
                    ButtonX     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_X")),
                    ButtonY     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_Y")),
                    ButtonPlus  = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_Plus")),
                    ButtonR     = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_R")),
                    ButtonZR    = Convert.ToInt16(Parser.Value("Controls_Right_FakeJoycon_Button_ZR"))
                }
            };
        }
    }

    // https://stackoverflow.com/a/37772571
    public class IniParser
    {
        private readonly Dictionary<string, string> Values;

        public IniParser(string Path)
        {
            Values = File.ReadLines(Path)
            .Where(Line => !string.IsNullOrWhiteSpace(Line) && !Line.StartsWith('#'))
            .Select(Line => Line.Split('=', 2))
            .ToDictionary(Parts => Parts[0].Trim(), Parts => Parts.Length > 1 ? Parts[1].Trim() : null);
        }

        /// <summary>
        /// Gets the setting value for the requested setting <see cref="Name"/>.
        /// </summary>
        /// <param name="Name">Setting Name</param>
        /// <param name="defaultValue">Default value of the setting</param>
        public string Value(string Name, string defaultValue = null)
        {
            return Values.TryGetValue(Name, out var value) ? value : defaultValue;
        }
    }
}
