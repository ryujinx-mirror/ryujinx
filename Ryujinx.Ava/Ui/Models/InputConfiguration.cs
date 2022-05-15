using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Controller.Motion;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using System;

namespace Ryujinx.Ava.Ui.Models
{
    public class InputConfiguration<Key, Stick> : BaseModel
    {
        private float _deadzoneRight;
        private float _triggerThreshold;
        private float _deadzoneLeft;
        private double _gyroDeadzone;
        private int _sensitivity;
        private bool enableMotion;
        private float weakRumble;
        private float strongRumble;
        private float _rangeLeft;
        private float _rangeRight;

        public InputBackendType Backend { get; set; }

        /// <summary>
        /// Controller id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  Controller's Type
        /// </summary>
        public ControllerType ControllerType { get; set; }

        /// <summary>
        ///  Player's Index for the controller
        /// </summary>
        public PlayerIndex PlayerIndex { get; set; }

        public Stick LeftJoystick { get; set; }
        public bool LeftInvertStickX { get; set; }
        public bool LeftInvertStickY { get; set; }
        public bool RightRotate90 { get; set; }
        public Key LeftControllerStickButton { get; set; }

        public Stick RightJoystick { get; set; }
        public bool RightInvertStickX { get; set; }
        public bool RightInvertStickY { get; set; }
        public bool LeftRotate90 { get; set; }
        public Key RightControllerStickButton { get; set; }

        public float DeadzoneLeft
        {
            get => _deadzoneLeft;
            set
            {
                _deadzoneLeft = MathF.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public float RangeLeft
        {
            get => _rangeLeft;
            set
            {
                _rangeLeft = MathF.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public float DeadzoneRight
        {
            get => _deadzoneRight;
            set
            {
                _deadzoneRight = MathF.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public float RangeRight
        {
            get => _rangeRight;
            set
            {
                _rangeRight = MathF.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public float TriggerThreshold
        {
            get => _triggerThreshold;
            set
            {
                _triggerThreshold = MathF.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public MotionInputBackendType MotionBackend { get; set; }

        public Key ButtonMinus { get; set; }
        public Key ButtonL { get; set; }
        public Key ButtonZl { get; set; }
        public Key LeftButtonSl { get; set; }
        public Key LeftButtonSr { get; set; }
        public Key DpadUp { get; set; }
        public Key DpadDown { get; set; }
        public Key DpadLeft { get; set; }
        public Key DpadRight { get; set; }

        public Key ButtonPlus { get; set; }
        public Key ButtonR { get; set; }
        public Key ButtonZr { get; set; }
        public Key RightButtonSl { get; set; }
        public Key RightButtonSr { get; set; }
        public Key ButtonX { get; set; }
        public Key ButtonB { get; set; }
        public Key ButtonY { get; set; }
        public Key ButtonA { get; set; }


        public Key LeftStickUp { get; set; }
        public Key LeftStickDown { get; set; }
        public Key LeftStickLeft { get; set; }
        public Key LeftStickRight { get; set; }
        public Key LeftKeyboardStickButton { get; set; }

        public Key RightStickUp { get; set; }
        public Key RightStickDown { get; set; }
        public Key RightStickLeft { get; set; }
        public Key RightStickRight { get; set; }
        public Key RightKeyboardStickButton { get; set; }

        public int Sensitivity
        {
            get => _sensitivity;
            set
            {
                _sensitivity = value;

                OnPropertyChanged();
            }
        }

        public double GyroDeadzone
        {
            get => _gyroDeadzone;
            set
            {
                _gyroDeadzone = Math.Round(value, 3);

                OnPropertyChanged();
            }
        }

        public bool EnableMotion
        {
            get => enableMotion; set
            {
                enableMotion = value;

                OnPropertyChanged();
            }
        }

        public bool EnableCemuHookMotion { get; set; }
        public int Slot { get; set; }
        public int AltSlot { get; set; }
        public bool MirrorInput { get; set; }
        public string DsuServerHost { get; set; }
        public int DsuServerPort { get; set; }

        public bool EnableRumble { get; set; }
        public float WeakRumble
        {
            get => weakRumble; set
            {
                weakRumble = value;

                OnPropertyChanged();
            }
        }
        public float StrongRumble
        {
            get => strongRumble; set
            {
                strongRumble = value;

                OnPropertyChanged();
            }
        }

        public InputConfiguration(InputConfig config)
        {
            if (config != null)
            {
                Backend = config.Backend;
                Id = config.Id;
                ControllerType = config.ControllerType;
                PlayerIndex = config.PlayerIndex;

                if (config is StandardKeyboardInputConfig keyboardConfig)
                {
                    LeftStickUp = (Key)(object)keyboardConfig.LeftJoyconStick.StickUp;
                    LeftStickDown = (Key)(object)keyboardConfig.LeftJoyconStick.StickDown;
                    LeftStickLeft = (Key)(object)keyboardConfig.LeftJoyconStick.StickLeft;
                    LeftStickRight = (Key)(object)keyboardConfig.LeftJoyconStick.StickRight;
                    LeftKeyboardStickButton = (Key)(object)keyboardConfig.LeftJoyconStick.StickButton;

                    RightStickUp = (Key)(object)keyboardConfig.RightJoyconStick.StickUp;
                    RightStickDown = (Key)(object)keyboardConfig.RightJoyconStick.StickDown;
                    RightStickLeft = (Key)(object)keyboardConfig.RightJoyconStick.StickLeft;
                    RightStickRight = (Key)(object)keyboardConfig.RightJoyconStick.StickRight;
                    RightKeyboardStickButton = (Key)(object)keyboardConfig.RightJoyconStick.StickButton;

                    ButtonA = (Key)(object)keyboardConfig.RightJoycon.ButtonA;
                    ButtonB = (Key)(object)keyboardConfig.RightJoycon.ButtonB;
                    ButtonX = (Key)(object)keyboardConfig.RightJoycon.ButtonX;
                    ButtonY = (Key)(object)keyboardConfig.RightJoycon.ButtonY;
                    ButtonR = (Key)(object)keyboardConfig.RightJoycon.ButtonR;
                    RightButtonSl = (Key)(object)keyboardConfig.RightJoycon.ButtonSl;
                    RightButtonSr = (Key)(object)keyboardConfig.RightJoycon.ButtonSr;
                    ButtonZr = (Key)(object)keyboardConfig.RightJoycon.ButtonZr;
                    ButtonPlus = (Key)(object)keyboardConfig.RightJoycon.ButtonPlus;

                    DpadUp = (Key)(object)keyboardConfig.LeftJoycon.DpadUp;
                    DpadDown = (Key)(object)keyboardConfig.LeftJoycon.DpadDown;
                    DpadLeft = (Key)(object)keyboardConfig.LeftJoycon.DpadLeft;
                    DpadRight = (Key)(object)keyboardConfig.LeftJoycon.DpadRight;
                    ButtonMinus = (Key)(object)keyboardConfig.LeftJoycon.ButtonMinus;
                    LeftButtonSl = (Key)(object)keyboardConfig.LeftJoycon.ButtonSl;
                    LeftButtonSr = (Key)(object)keyboardConfig.LeftJoycon.ButtonSr;
                    ButtonZl = (Key)(object)keyboardConfig.LeftJoycon.ButtonZl;
                    ButtonL = (Key)(object)keyboardConfig.LeftJoycon.ButtonL;
                }
                else if (config is StandardControllerInputConfig controllerConfig)
                {
                    LeftJoystick = (Stick)(object)controllerConfig.LeftJoyconStick.Joystick;
                    LeftInvertStickX = controllerConfig.LeftJoyconStick.InvertStickX;
                    LeftInvertStickY = controllerConfig.LeftJoyconStick.InvertStickY;
                    LeftRotate90 = controllerConfig.LeftJoyconStick.Rotate90CW;
                    LeftControllerStickButton = (Key)(object)controllerConfig.LeftJoyconStick.StickButton;

                    RightJoystick = (Stick)(object)controllerConfig.RightJoyconStick.Joystick;
                    RightInvertStickX = controllerConfig.RightJoyconStick.InvertStickX;
                    RightInvertStickY = controllerConfig.RightJoyconStick.InvertStickY;
                    RightRotate90 = controllerConfig.RightJoyconStick.Rotate90CW;
                    RightControllerStickButton = (Key)(object)controllerConfig.RightJoyconStick.StickButton;

                    ButtonA = (Key)(object)controllerConfig.RightJoycon.ButtonA;
                    ButtonB = (Key)(object)controllerConfig.RightJoycon.ButtonB;
                    ButtonX = (Key)(object)controllerConfig.RightJoycon.ButtonX;
                    ButtonY = (Key)(object)controllerConfig.RightJoycon.ButtonY;
                    ButtonR = (Key)(object)controllerConfig.RightJoycon.ButtonR;
                    RightButtonSl = (Key)(object)controllerConfig.RightJoycon.ButtonSl;
                    RightButtonSr = (Key)(object)controllerConfig.RightJoycon.ButtonSr;
                    ButtonZr = (Key)(object)controllerConfig.RightJoycon.ButtonZr;
                    ButtonPlus = (Key)(object)controllerConfig.RightJoycon.ButtonPlus;

                    DpadUp = (Key)(object)controllerConfig.LeftJoycon.DpadUp;
                    DpadDown = (Key)(object)controllerConfig.LeftJoycon.DpadDown;
                    DpadLeft = (Key)(object)controllerConfig.LeftJoycon.DpadLeft;
                    DpadRight = (Key)(object)controllerConfig.LeftJoycon.DpadRight;
                    ButtonMinus = (Key)(object)controllerConfig.LeftJoycon.ButtonMinus;
                    LeftButtonSl = (Key)(object)controllerConfig.LeftJoycon.ButtonSl;
                    LeftButtonSr = (Key)(object)controllerConfig.LeftJoycon.ButtonSr;
                    ButtonZl = (Key)(object)controllerConfig.LeftJoycon.ButtonZl;
                    ButtonL = (Key)(object)controllerConfig.LeftJoycon.ButtonL;

                    DeadzoneLeft = controllerConfig.DeadzoneLeft;
                    DeadzoneRight = controllerConfig.DeadzoneRight;
                    RangeLeft = controllerConfig.RangeLeft;
                    RangeRight = controllerConfig.RangeRight;
                    TriggerThreshold = controllerConfig.TriggerThreshold;

                    if (controllerConfig.Motion != null)
                    {
                        EnableMotion = controllerConfig.Motion.EnableMotion;
                        MotionBackend = controllerConfig.Motion.MotionBackend;
                        GyroDeadzone = controllerConfig.Motion.GyroDeadzone;
                        Sensitivity = controllerConfig.Motion.Sensitivity;

                        if (controllerConfig.Motion is CemuHookMotionConfigController cemuHook)
                        {
                            EnableCemuHookMotion = true;
                            DsuServerHost = cemuHook.DsuServerHost;
                            DsuServerPort = cemuHook.DsuServerPort;
                            Slot = cemuHook.Slot;
                            AltSlot = cemuHook.AltSlot;
                            MirrorInput = cemuHook.MirrorInput;
                        }

                        if (controllerConfig.Rumble != null)
                        {
                            EnableRumble = controllerConfig.Rumble.EnableRumble;
                            WeakRumble = controllerConfig.Rumble.WeakRumble;
                            StrongRumble = controllerConfig.Rumble.StrongRumble;
                        }
                    }
                }
            }
        }

        public InputConfiguration()
        {
        }

        public InputConfig GetConfig()
        {
            if (Backend == InputBackendType.WindowKeyboard)
            {
                return new StandardKeyboardInputConfig()
                {
                    Id = Id,
                    Backend = Backend,
                    PlayerIndex = PlayerIndex,
                    ControllerType = ControllerType,
                    LeftJoycon = new LeftJoyconCommonConfig<Ryujinx.Common.Configuration.Hid.Key>()
                    {
                        DpadUp = (Ryujinx.Common.Configuration.Hid.Key)(object)DpadUp,
                        DpadDown = (Ryujinx.Common.Configuration.Hid.Key)(object)DpadDown,
                        DpadLeft = (Ryujinx.Common.Configuration.Hid.Key)(object)DpadLeft,
                        DpadRight = (Ryujinx.Common.Configuration.Hid.Key)(object)DpadRight,
                        ButtonL = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonL,
                        ButtonZl = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonZl,
                        ButtonSl = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftButtonSl,
                        ButtonSr = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftButtonSr,
                        ButtonMinus = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonMinus
                    },
                    RightJoycon = new RightJoyconCommonConfig<Ryujinx.Common.Configuration.Hid.Key>()
                    {
                        ButtonA = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonA,
                        ButtonB = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonB,
                        ButtonX = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonX,
                        ButtonY = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonY,
                        ButtonPlus = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonPlus,
                        ButtonSl = (Ryujinx.Common.Configuration.Hid.Key)(object)RightButtonSl,
                        ButtonSr = (Ryujinx.Common.Configuration.Hid.Key)(object)RightButtonSr,
                        ButtonR = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonR,
                        ButtonZr = (Ryujinx.Common.Configuration.Hid.Key)(object)ButtonZr
                    },
                    LeftJoyconStick = new JoyconConfigKeyboardStick<Ryujinx.Common.Configuration.Hid.Key>()
                    {
                        StickUp = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftStickUp,
                        StickDown = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftStickDown,
                        StickRight = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftStickRight,
                        StickLeft = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftStickLeft,
                        StickButton = (Ryujinx.Common.Configuration.Hid.Key)(object)LeftKeyboardStickButton
                    },
                    RightJoyconStick = new JoyconConfigKeyboardStick<Ryujinx.Common.Configuration.Hid.Key>()
                    {
                        StickUp = (Ryujinx.Common.Configuration.Hid.Key)(object)RightStickUp,
                        StickDown = (Ryujinx.Common.Configuration.Hid.Key)(object)RightStickDown,
                        StickLeft = (Ryujinx.Common.Configuration.Hid.Key)(object)RightStickLeft,
                        StickRight = (Ryujinx.Common.Configuration.Hid.Key)(object)RightStickRight,
                        StickButton = (Ryujinx.Common.Configuration.Hid.Key)(object)RightKeyboardStickButton
                    },
                    Version = InputConfig.CurrentVersion
                };

            }
            else if (Backend == InputBackendType.GamepadSDL2)
            {
                var config = new StandardControllerInputConfig()
                {
                    Id = Id,
                    Backend = Backend,
                    PlayerIndex = PlayerIndex,
                    ControllerType = ControllerType,
                    LeftJoycon = new LeftJoyconCommonConfig<GamepadInputId>()
                    {
                        DpadUp = (GamepadInputId)(object)DpadUp,
                        DpadDown = (GamepadInputId)(object)DpadDown,
                        DpadLeft = (GamepadInputId)(object)DpadLeft,
                        DpadRight = (GamepadInputId)(object)DpadRight,
                        ButtonL = (GamepadInputId)(object)ButtonL,
                        ButtonZl = (GamepadInputId)(object)ButtonZl,
                        ButtonSl = (GamepadInputId)(object)LeftButtonSl,
                        ButtonSr = (GamepadInputId)(object)LeftButtonSr,
                        ButtonMinus = (GamepadInputId)(object)ButtonMinus,
                    },
                    RightJoycon = new RightJoyconCommonConfig<GamepadInputId>()
                    {
                        ButtonA = (GamepadInputId)(object)ButtonA,
                        ButtonB = (GamepadInputId)(object)ButtonB,
                        ButtonX = (GamepadInputId)(object)ButtonX,
                        ButtonY = (GamepadInputId)(object)ButtonY,
                        ButtonPlus = (GamepadInputId)(object)ButtonPlus,
                        ButtonSl = (GamepadInputId)(object)RightButtonSl,
                        ButtonSr = (GamepadInputId)(object)RightButtonSr,
                        ButtonR = (GamepadInputId)(object)ButtonR,
                        ButtonZr = (GamepadInputId)(object)ButtonZr,
                    },
                    LeftJoyconStick = new JoyconConfigControllerStick<GamepadInputId, StickInputId>()
                    {
                        Joystick = (StickInputId)(object)LeftJoystick,
                        InvertStickX = LeftInvertStickX,
                        InvertStickY = LeftInvertStickY,
                        Rotate90CW = LeftRotate90,
                        StickButton = (GamepadInputId)(object)LeftControllerStickButton,
                    },
                    RightJoyconStick = new JoyconConfigControllerStick<GamepadInputId, StickInputId>()
                    {
                        Joystick = (StickInputId)(object)RightJoystick,
                        InvertStickX = RightInvertStickX,
                        InvertStickY = RightInvertStickY,
                        Rotate90CW = RightRotate90,
                        StickButton = (GamepadInputId)(object)RightControllerStickButton,
                    },
                    Rumble = new RumbleConfigController()
                    {
                        EnableRumble = EnableRumble,
                        WeakRumble = WeakRumble,
                        StrongRumble = StrongRumble
                    },
                    Version = InputConfig.CurrentVersion,
                    DeadzoneLeft = DeadzoneLeft,
                    DeadzoneRight = DeadzoneRight,
                    RangeLeft = RangeLeft,
                    RangeRight = RangeRight,
                    TriggerThreshold = TriggerThreshold,
                    Motion = EnableCemuHookMotion
                           ? new CemuHookMotionConfigController()
                           {
                               DsuServerHost = DsuServerHost,
                               DsuServerPort = DsuServerPort,
                               Slot = Slot,
                               AltSlot = AltSlot,
                               MirrorInput = MirrorInput,
                               MotionBackend = MotionInputBackendType.CemuHook
                           }
                           : new StandardMotionConfigController()
                           {
                               MotionBackend = MotionInputBackendType.GamepadDriver
                           }
                };

                config.Motion.Sensitivity = Sensitivity;
                config.Motion.EnableMotion = EnableMotion;
                config.Motion.GyroDeadzone = GyroDeadzone;

                return config;
            }

            return null;
        }
    }
}