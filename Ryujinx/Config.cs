using Ryujinx.Core.Input;
using Ryujinx.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx
{
    public static class Config
    {
        public static JoyCon FakeJoyCon { get; private set; }

        public static void Read(Logger Log)
        {
            string IniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string IniPath = Path.Combine(IniFolder, "Ryujinx.conf");

            IniParser Parser = new IniParser(IniPath);

            AOptimizations.DisableMemoryChecks = !Convert.ToBoolean(Parser.Value("Enable_Memory_Checks"));

            Console.WriteLine(Parser.Value("Logging_Enable_Warn"));

            bool LoggingEnableDebug = Convert.ToBoolean(Parser.Value("Logging_Enable_Debug"));
            bool LoggingEnableStub  = Convert.ToBoolean(Parser.Value("Logging_Enable_Stub"));
            bool LoggingEnableInfo  = Convert.ToBoolean(Parser.Value("Logging_Enable_Info"));
            bool LoggingEnableTrace = Convert.ToBoolean(Parser.Value("Logging_Enable_Trace"));
            bool LoggingEnableWarn  = Convert.ToBoolean(Parser.Value("Logging_Enable_Warn"));
            bool LoggingEnableError = Convert.ToBoolean(Parser.Value("Logging_Enable_Error"));

            string[] FilteredLogClasses = Parser.Value("Logging_Filtered_Classes").Split(',', StringSplitOptions.RemoveEmptyEntries);

            //When the classes are specified on the list, we only
            //enable the classes that are on the list.
            //So, first disable everything, then enable
            //the classes that the user added to the list.
            if (FilteredLogClasses.Length > 0)
            {
                foreach (LogClass Class in Enum.GetValues(typeof(LogClass)))
                {
                    Log.SetEnable(Class, false);
                }
            }

            foreach (string LogClass in FilteredLogClasses)
            {
                if (!string.IsNullOrEmpty(LogClass.Trim()))
                {
                    foreach (LogClass Class in Enum.GetValues(typeof(LogClass)))
                    {
                        if (Class.ToString().ToLower().Contains(LogClass.Trim().ToLower()))
                        {
                            Log.SetEnable(Class, true);
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

        public string Value(string Name)
        {
            return Values.TryGetValue(Name, out string Value) ? Value : null;
        }
    }
}
