using LibHac;
using Ryujinx.Common.Logging;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using Ryujinx.UI.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx
{
    public static class Config
    {
        public static NpadKeyboard   NpadKeyboard   { get; private set; }
        public static NpadController NpadController { get; private set; }

        public static void Read(Switch device)
        {
            string iniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string iniPath = Path.Combine(iniFolder, "Ryujinx.conf");

            IniParser parser = new IniParser(iniPath);

            GraphicsConfig.ShadersDumpPath = parser.Value("Graphics_Shaders_Dump_Path");

            Logger.SetEnable(LogLevel.Debug,   Convert.ToBoolean(parser.Value("Logging_Enable_Debug")));
            Logger.SetEnable(LogLevel.Stub,    Convert.ToBoolean(parser.Value("Logging_Enable_Stub")));
            Logger.SetEnable(LogLevel.Info,    Convert.ToBoolean(parser.Value("Logging_Enable_Info")));
            Logger.SetEnable(LogLevel.Warning, Convert.ToBoolean(parser.Value("Logging_Enable_Warn")));
            Logger.SetEnable(LogLevel.Error,   Convert.ToBoolean(parser.Value("Logging_Enable_Error")));

            string[] filteredLogClasses = parser.Value("Logging_Filtered_Classes").Split(',', StringSplitOptions.RemoveEmptyEntries);

            //When the classes are specified on the list, we only
            //enable the classes that are on the list.
            //So, first disable everything, then enable
            //the classes that the user added to the list.
            if (filteredLogClasses.Length > 0)
            {
                foreach (LogClass Class in Enum.GetValues(typeof(LogClass)))
                {
                    Logger.SetEnable(Class, false);
                }
            }

            foreach (string logClass in filteredLogClasses)
            {
                if (!string.IsNullOrEmpty(logClass.Trim()))
                {
                    foreach (LogClass Class in Enum.GetValues(typeof(LogClass)))
                    {
                        if (Class.ToString().ToLower().Contains(logClass.Trim().ToLower()))
                        {
                            Logger.SetEnable(Class, true);
                        }
                    }
                }
            }

            device.System.State.DockedMode = Convert.ToBoolean(parser.Value("Docked_Mode"));

            device.EnableDeviceVsync = Convert.ToBoolean(parser.Value("Enable_Vsync"));

            if (Convert.ToBoolean(parser.Value("Enable_MultiCore_Scheduling")))
            {
                device.System.EnableMultiCoreScheduling();
            }

            device.System.FsIntegrityCheckLevel = Convert.ToBoolean(parser.Value("Enable_FS_Integrity_Checks"))
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;

            HidControllerType ControllerType = Enum.Parse<HidControllerType>(parser.Value("Controller_Type"));

            device.Hid.InitilizePrimaryController(ControllerType);

            NpadKeyboard = new NpadKeyboard(

                new NpadKeyboardLeft
                {
                    StickUp     = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Stick_Up")),
                    StickDown   = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Stick_Down")),
                    StickLeft   = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Stick_Left")),
                    StickRight  = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Stick_Right")),
                    StickButton = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Stick_Button")),
                    DPadUp      = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_DPad_Up")),
                    DPadDown    = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_DPad_Down")),
                    DPadLeft    = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_DPad_Left")),
                    DPadRight   = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_DPad_Right")),
                    ButtonMinus = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Button_Minus")),
                    ButtonL     = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Button_L")),
                    ButtonZl    = Convert.ToInt16(parser.Value("Controls_Left_JoyConKeyboard_Button_ZL"))
                },

                new NpadKeyboardRight
                {
                    StickUp     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Stick_Up")),
                    StickDown   = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Stick_Down")),
                    StickLeft   = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Stick_Left")),
                    StickRight  = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Stick_Right")),
                    StickButton = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Stick_Button")),
                    ButtonA     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_A")),
                    ButtonB     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_B")),
                    ButtonX     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_X")),
                    ButtonY     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_Y")),
                    ButtonPlus  = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_Plus")),
                    ButtonR     = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_R")),
                    ButtonZr    = Convert.ToInt16(parser.Value("Controls_Right_JoyConKeyboard_Button_ZR"))
                });

            NpadController = new NpadController(

                       Convert.ToBoolean(parser.Value("GamePad_Enable")),
                       Convert.ToInt32  (parser.Value("GamePad_Index")),
                (float)Convert.ToDouble (parser.Value("GamePad_Deadzone"),          CultureInfo.InvariantCulture),
                (float)Convert.ToDouble (parser.Value("GamePad_Trigger_Threshold"), CultureInfo.InvariantCulture),

                new NpadControllerLeft
                {
                    Stick       = ToId(parser.Value("Controls_Left_JoyConController_Stick")),
                    StickButton = ToId(parser.Value("Controls_Left_JoyConController_Stick_Button")),
                    DPadUp      = ToId(parser.Value("Controls_Left_JoyConController_DPad_Up")),
                    DPadDown    = ToId(parser.Value("Controls_Left_JoyConController_DPad_Down")),
                    DPadLeft    = ToId(parser.Value("Controls_Left_JoyConController_DPad_Left")),
                    DPadRight   = ToId(parser.Value("Controls_Left_JoyConController_DPad_Right")),
                    ButtonMinus = ToId(parser.Value("Controls_Left_JoyConController_Button_Minus")),
                    ButtonL     = ToId(parser.Value("Controls_Left_JoyConController_Button_L")),
                    ButtonZl    = ToId(parser.Value("Controls_Left_JoyConController_Button_ZL"))
                },

                new NpadControllerRight
                {
                    Stick       = ToId(parser.Value("Controls_Right_JoyConController_Stick")),
                    StickButton = ToId(parser.Value("Controls_Right_JoyConController_Stick_Button")),
                    ButtonA     = ToId(parser.Value("Controls_Right_JoyConController_Button_A")),
                    ButtonB     = ToId(parser.Value("Controls_Right_JoyConController_Button_B")),
                    ButtonX     = ToId(parser.Value("Controls_Right_JoyConController_Button_X")),
                    ButtonY     = ToId(parser.Value("Controls_Right_JoyConController_Button_Y")),
                    ButtonPlus  = ToId(parser.Value("Controls_Right_JoyConController_Button_Plus")),
                    ButtonR     = ToId(parser.Value("Controls_Right_JoyConController_Button_R")),
                    ButtonZr    = ToId(parser.Value("Controls_Right_JoyConController_Button_ZR"))
                });
        }

        private static ControllerInputId ToId(string key)
        {
            switch (key.ToUpper())
            {
                case "LSTICK":    return ControllerInputId.LStick;
                case "DPADUP":    return ControllerInputId.DPadUp;
                case "DPADDOWN":  return ControllerInputId.DPadDown;
                case "DPADLEFT":  return ControllerInputId.DPadLeft;
                case "DPADRIGHT": return ControllerInputId.DPadRight;
                case "BACK":      return ControllerInputId.Back;
                case "LSHOULDER": return ControllerInputId.LShoulder;
                case "LTRIGGER":  return ControllerInputId.LTrigger;

                case "RSTICK":    return ControllerInputId.RStick;
                case "A":         return ControllerInputId.A;
                case "B":         return ControllerInputId.B;
                case "X":         return ControllerInputId.X;
                case "Y":         return ControllerInputId.Y;
                case "START":     return ControllerInputId.Start;
                case "RSHOULDER": return ControllerInputId.RShoulder;
                case "RTRIGGER":  return ControllerInputId.RTrigger;

                case "LJOYSTICK": return ControllerInputId.LJoystick;
                case "RJOYSTICK": return ControllerInputId.RJoystick;

                default: return ControllerInputId.Invalid;
            }
        }
    }

    //https://stackoverflow.com/a/37772571
    public class IniParser
    {
        private readonly Dictionary<string, string> _values;

        public IniParser(string path)
        {
            _values = File.ReadLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
                .Select(line => line.Split('=', 2))
                .ToDictionary(parts => parts[0].Trim(), parts => parts.Length > 1 ? parts[1].Trim() : null);
        }

        public string Value(string name)
        {
            return _values.TryGetValue(name, out string value) ? value : null;
        }
    }
}