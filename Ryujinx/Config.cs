using Ryujinx.UI.Input;
using Ryujinx.HLE.Logging;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx
{
    public static class Config
    {
        public static JoyConKeyboard   JoyConKeyboard   { get; private set; }
        public static JoyConController JoyConController { get; private set; }

        public static float GamePadDeadzone             { get; private set; }
        public static bool  GamePadEnable               { get; private set; }
        public static int   GamePadIndex                { get; private set; }
        public static float GamePadTriggerThreshold     { get; private set; }

        public static void Read(Logger Log)
        {
            string IniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string IniPath = Path.Combine(IniFolder, "Ryujinx.conf");

            IniParser Parser = new IniParser(IniPath);

            AOptimizations.DisableMemoryChecks = !Convert.ToBoolean(Parser.Value("Enable_Memory_Checks"));

            GraphicsConfig.ShadersDumpPath = Parser.Value("Graphics_Shaders_Dump_Path");

            Log.SetEnable(LogLevel.Debug,   Convert.ToBoolean(Parser.Value("Logging_Enable_Debug")));
            Log.SetEnable(LogLevel.Stub,    Convert.ToBoolean(Parser.Value("Logging_Enable_Stub")));
            Log.SetEnable(LogLevel.Info,    Convert.ToBoolean(Parser.Value("Logging_Enable_Info")));
            Log.SetEnable(LogLevel.Warning, Convert.ToBoolean(Parser.Value("Logging_Enable_Warn")));
            Log.SetEnable(LogLevel.Error,   Convert.ToBoolean(Parser.Value("Logging_Enable_Error")));

            GamePadEnable            =        Convert.ToBoolean(Parser.Value("GamePad_Enable"));
            GamePadIndex             =        Convert.ToInt32  (Parser.Value("GamePad_Index"));
            GamePadDeadzone          = (float)Convert.ToDouble (Parser.Value("GamePad_Deadzone"),          CultureInfo.InvariantCulture);
            GamePadTriggerThreshold  = (float)Convert.ToDouble (Parser.Value("GamePad_Trigger_Threshold"), CultureInfo.InvariantCulture);

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

            JoyConKeyboard = new JoyConKeyboard
            {
                Left = new JoyConKeyboardLeft
                {
                    StickUp     = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Stick_Up")),
                    StickDown   = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Stick_Down")),
                    StickLeft   = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Stick_Left")),
                    StickRight  = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Stick_Right")),
                    StickButton = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Stick_Button")),
                    DPadUp      = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_DPad_Up")),
                    DPadDown    = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_DPad_Down")),
                    DPadLeft    = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_DPad_Left")),
                    DPadRight   = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_DPad_Right")),
                    ButtonMinus = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Button_Minus")),
                    ButtonL     = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Button_L")),
                    ButtonZL    = Convert.ToInt16(Parser.Value("Controls_Left_JoyConKeyboard_Button_ZL"))
                },

                Right = new JoyConKeyboardRight
                {
                    StickUp     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Stick_Up")),
                    StickDown   = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Stick_Down")),
                    StickLeft   = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Stick_Left")),
                    StickRight  = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Stick_Right")),
                    StickButton = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Stick_Button")),
                    ButtonA     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_A")),
                    ButtonB     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_B")),
                    ButtonX     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_X")),
                    ButtonY     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_Y")),
                    ButtonPlus  = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_Plus")),
                    ButtonR     = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_R")),
                    ButtonZR    = Convert.ToInt16(Parser.Value("Controls_Right_JoyConKeyboard_Button_ZR"))
                }
            };

            JoyConController = new JoyConController
            {
                Left = new JoyConControllerLeft
                {
                    Stick       = Parser.Value("Controls_Left_JoyConController_Stick"),
                    StickButton = Parser.Value("Controls_Left_JoyConController_Stick_Button"),
                    DPadUp      = Parser.Value("Controls_Left_JoyConController_DPad_Up"),
                    DPadDown    = Parser.Value("Controls_Left_JoyConController_DPad_Down"),
                    DPadLeft    = Parser.Value("Controls_Left_JoyConController_DPad_Left"),
                    DPadRight   = Parser.Value("Controls_Left_JoyConController_DPad_Right"),
                    ButtonMinus = Parser.Value("Controls_Left_JoyConController_Button_Minus"),
                    ButtonL     = Parser.Value("Controls_Left_JoyConController_Button_L"),
                    ButtonZL    = Parser.Value("Controls_Left_JoyConController_Button_ZL")
                },

                Right = new JoyConControllerRight
                {
                    Stick       = Parser.Value("Controls_Right_JoyConController_Stick"),
                    StickButton = Parser.Value("Controls_Right_JoyConController_Stick_Button"),
                    ButtonA     = Parser.Value("Controls_Right_JoyConController_Button_A"),
                    ButtonB     = Parser.Value("Controls_Right_JoyConController_Button_B"),
                    ButtonX     = Parser.Value("Controls_Right_JoyConController_Button_X"),
                    ButtonY     = Parser.Value("Controls_Right_JoyConController_Button_Y"),
                    ButtonPlus  = Parser.Value("Controls_Right_JoyConController_Button_Plus"),
                    ButtonR     = Parser.Value("Controls_Right_JoyConController_Button_R"),
                    ButtonZR    = Parser.Value("Controls_Right_JoyConController_Button_ZR")
                }
            };
        }
    }

    //https://stackoverflow.com/a/37772571
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
