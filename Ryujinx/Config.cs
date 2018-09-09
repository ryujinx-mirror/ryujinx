using Ryujinx.HLE;
using Ryujinx.HLE.Logging;
using Ryujinx.UI.Input;
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

        public static void Read(Switch Device)
        {
            string IniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string IniPath = Path.Combine(IniFolder, "Ryujinx.conf");

            IniParser Parser = new IniParser(IniPath);

            GraphicsConfig.ShadersDumpPath = Parser.Value("Graphics_Shaders_Dump_Path");

            Device.Log.SetEnable(LogLevel.Debug,   Convert.ToBoolean(Parser.Value("Logging_Enable_Debug")));
            Device.Log.SetEnable(LogLevel.Stub,    Convert.ToBoolean(Parser.Value("Logging_Enable_Stub")));
            Device.Log.SetEnable(LogLevel.Info,    Convert.ToBoolean(Parser.Value("Logging_Enable_Info")));
            Device.Log.SetEnable(LogLevel.Warning, Convert.ToBoolean(Parser.Value("Logging_Enable_Warn")));
            Device.Log.SetEnable(LogLevel.Error,   Convert.ToBoolean(Parser.Value("Logging_Enable_Error")));

            Device.System.State.DockedMode = Convert.ToBoolean(Parser.Value("Docked_Mode"));

            Device.EnableDeviceVsync = Convert.ToBoolean(Parser.Value("Enable_Vsync"));

            string[] FilteredLogClasses = Parser.Value("Logging_Filtered_Classes").Split(',', StringSplitOptions.RemoveEmptyEntries);

            //When the classes are specified on the list, we only
            //enable the classes that are on the list.
            //So, first disable everything, then enable
            //the classes that the user added to the list.
            if (FilteredLogClasses.Length > 0)
            {
                foreach (LogClass Class in Enum.GetValues(typeof(LogClass)))
                {
                    Device.Log.SetEnable(Class, false);
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
                            Device.Log.SetEnable(Class, true);
                        }
                    }
                }
            }

            JoyConKeyboard = new JoyConKeyboard(

                new JoyConKeyboardLeft
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

                new JoyConKeyboardRight
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
                });

            JoyConController = new JoyConController(

                       Convert.ToBoolean(Parser.Value("GamePad_Enable")),
                       Convert.ToInt32  (Parser.Value("GamePad_Index")),
                (float)Convert.ToDouble (Parser.Value("GamePad_Deadzone"),          CultureInfo.InvariantCulture),
                (float)Convert.ToDouble (Parser.Value("GamePad_Trigger_Threshold"), CultureInfo.InvariantCulture),

                new JoyConControllerLeft
                {
                    Stick       = ToID(Parser.Value("Controls_Left_JoyConController_Stick")),
                    StickButton = ToID(Parser.Value("Controls_Left_JoyConController_Stick_Button")),
                    DPadUp      = ToID(Parser.Value("Controls_Left_JoyConController_DPad_Up")),
                    DPadDown    = ToID(Parser.Value("Controls_Left_JoyConController_DPad_Down")),
                    DPadLeft    = ToID(Parser.Value("Controls_Left_JoyConController_DPad_Left")),
                    DPadRight   = ToID(Parser.Value("Controls_Left_JoyConController_DPad_Right")),
                    ButtonMinus = ToID(Parser.Value("Controls_Left_JoyConController_Button_Minus")),
                    ButtonL     = ToID(Parser.Value("Controls_Left_JoyConController_Button_L")),
                    ButtonZL    = ToID(Parser.Value("Controls_Left_JoyConController_Button_ZL"))
                },

                new JoyConControllerRight
                {
                    Stick       = ToID(Parser.Value("Controls_Right_JoyConController_Stick")),
                    StickButton = ToID(Parser.Value("Controls_Right_JoyConController_Stick_Button")),
                    ButtonA     = ToID(Parser.Value("Controls_Right_JoyConController_Button_A")),
                    ButtonB     = ToID(Parser.Value("Controls_Right_JoyConController_Button_B")),
                    ButtonX     = ToID(Parser.Value("Controls_Right_JoyConController_Button_X")),
                    ButtonY     = ToID(Parser.Value("Controls_Right_JoyConController_Button_Y")),
                    ButtonPlus  = ToID(Parser.Value("Controls_Right_JoyConController_Button_Plus")),
                    ButtonR     = ToID(Parser.Value("Controls_Right_JoyConController_Button_R")),
                    ButtonZR    = ToID(Parser.Value("Controls_Right_JoyConController_Button_ZR"))
                });
        }

        private static ControllerInputID ToID(string Key)
        {
            switch (Key.ToUpper())
            {
                case "LSTICK":    return ControllerInputID.LStick;
                case "DPADUP":    return ControllerInputID.DPadUp;
                case "DPADDOWN":  return ControllerInputID.DPadDown;
                case "DPADLEFT":  return ControllerInputID.DPadLeft;
                case "DPADRIGHT": return ControllerInputID.DPadRight;
                case "BACK":      return ControllerInputID.Back;
                case "LSHOULDER": return ControllerInputID.LShoulder;
                case "LTRIGGER":  return ControllerInputID.LTrigger;

                case "RSTICK":    return ControllerInputID.RStick;
                case "A":         return ControllerInputID.A;
                case "B":         return ControllerInputID.B;
                case "X":         return ControllerInputID.X;
                case "Y":         return ControllerInputID.Y;
                case "START":     return ControllerInputID.Start;
                case "RSHOULDER": return ControllerInputID.RShoulder;
                case "RTRIGGER":  return ControllerInputID.RTrigger;

                case "LJOYSTICK": return ControllerInputID.LJoystick;
                case "RJOYSTICK": return ControllerInputID.RJoystick;

                default: return ControllerInputID.Invalid;
            }
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