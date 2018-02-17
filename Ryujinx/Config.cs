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

        public static JoyCon FakeJoyCon { get; private set; }

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
