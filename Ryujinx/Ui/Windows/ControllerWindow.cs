using Gtk;
using OpenTK.Input;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Utilities;
using Ryujinx.Configuration;
using Ryujinx.Ui.Input;
using Ryujinx.Ui.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

using GUI = Gtk.Builder.ObjectAttribute;
using Key = Ryujinx.Configuration.Hid.Key;

namespace Ryujinx.Ui.Windows
{
    public class ControllerWindow : Window
    {
        private readonly PlayerIndex _playerIndex;
        private readonly InputConfig _inputConfig;

        private bool _isWaitingForInput;

#pragma warning disable CS0649, IDE0044
        [GUI] Adjustment   _controllerDeadzoneLeft;
        [GUI] Adjustment   _controllerDeadzoneRight;
        [GUI] Adjustment   _controllerTriggerThreshold;
        [GUI] Adjustment   _slotNumber;
        [GUI] Adjustment   _altSlotNumber;
        [GUI] Adjustment   _sensitivity;
        [GUI] Adjustment   _gyroDeadzone;
        [GUI] CheckButton  _enableMotion;
        [GUI] CheckButton  _mirrorInput;
        [GUI] Entry        _dsuServerHost;
        [GUI] Entry        _dsuServerPort;
        [GUI] ComboBoxText _inputDevice;
        [GUI] ComboBoxText _profile;
        [GUI] ToggleButton _refreshInputDevicesButton;
        [GUI] Box          _settingsBox;
        [GUI] Box          _altBox;
        [GUI] Grid         _leftStickKeyboard;
        [GUI] Grid         _leftStickController;
        [GUI] Box          _deadZoneLeftBox;
        [GUI] Grid         _rightStickKeyboard;
        [GUI] Grid         _rightStickController;
        [GUI] Box          _deadZoneRightBox;
        [GUI] Grid         _leftSideTriggerBox;
        [GUI] Grid         _rightSideTriggerBox;
        [GUI] Box          _triggerThresholdBox;
        [GUI] ComboBoxText _controllerType;
        [GUI] ToggleButton _lStickX;
        [GUI] CheckButton  _invertLStickX;
        [GUI] ToggleButton _lStickY;
        [GUI] CheckButton  _invertLStickY;
        [GUI] ToggleButton _lStickUp;
        [GUI] ToggleButton _lStickDown;
        [GUI] ToggleButton _lStickLeft;
        [GUI] ToggleButton _lStickRight;
        [GUI] ToggleButton _lStickButton;
        [GUI] ToggleButton _dpadUp;
        [GUI] ToggleButton _dpadDown;
        [GUI] ToggleButton _dpadLeft;
        [GUI] ToggleButton _dpadRight;
        [GUI] ToggleButton _minus;
        [GUI] ToggleButton _l;
        [GUI] ToggleButton _zL;
        [GUI] ToggleButton _rStickX;
        [GUI] CheckButton  _invertRStickX;
        [GUI] ToggleButton _rStickY;
        [GUI] CheckButton  _invertRStickY;
        [GUI] ToggleButton _rStickUp;
        [GUI] ToggleButton _rStickDown;
        [GUI] ToggleButton _rStickLeft;
        [GUI] ToggleButton _rStickRight;
        [GUI] ToggleButton _rStickButton;
        [GUI] ToggleButton _a;
        [GUI] ToggleButton _b;
        [GUI] ToggleButton _x;
        [GUI] ToggleButton _y;
        [GUI] ToggleButton _plus;
        [GUI] ToggleButton _r;
        [GUI] ToggleButton _zR;
        [GUI] ToggleButton _lSl;
        [GUI] ToggleButton _lSr;
        [GUI] ToggleButton _rSl;
        [GUI] ToggleButton _rSr;
        [GUI] Image        _controllerImage;
#pragma warning restore CS0649, IDE0044

        public ControllerWindow(PlayerIndex controllerId) : this(new Builder("Ryujinx.Ui.Windows.ControllerWindow.glade"), controllerId) { }

        private ControllerWindow(Builder builder, PlayerIndex controllerId) : base(builder.GetObject("_controllerWin").Handle)
        {
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Logo_Ryujinx.png");

            builder.Autoconnect(this);

            _playerIndex = controllerId;
            _inputConfig = ConfigurationState.Instance.Hid.InputConfig.Value.Find(inputConfig => inputConfig.PlayerIndex == _playerIndex);

            Title = $"Ryujinx - Controller Settings - {_playerIndex}";

            if (_playerIndex == PlayerIndex.Handheld)
            {
                _controllerType.Append(ControllerType.Handheld.ToString(), "Handheld");
                _controllerType.Sensitive = false;
            }
            else
            {
                _controllerType.Append(ControllerType.ProController.ToString(), "Pro Controller");
                _controllerType.Append(ControllerType.JoyconPair.ToString(), "Joycon Pair");
                _controllerType.Append(ControllerType.JoyconLeft.ToString(), "Joycon Left");
                _controllerType.Append(ControllerType.JoyconRight.ToString(), "Joycon Right");
            }

            _controllerType.Active = 0; // Set initial value to first in list.

            // Bind Events.
            _lStickX.Clicked        += Button_Pressed;
            _lStickY.Clicked        += Button_Pressed;
            _lStickUp.Clicked       += Button_Pressed;
            _lStickDown.Clicked     += Button_Pressed;
            _lStickLeft.Clicked     += Button_Pressed;
            _lStickRight.Clicked    += Button_Pressed;
            _lStickButton.Clicked   += Button_Pressed;
            _dpadUp.Clicked         += Button_Pressed;
            _dpadDown.Clicked       += Button_Pressed;
            _dpadLeft.Clicked       += Button_Pressed;
            _dpadRight.Clicked      += Button_Pressed;
            _minus.Clicked          += Button_Pressed;
            _l.Clicked              += Button_Pressed;
            _zL.Clicked             += Button_Pressed;
            _lSl.Clicked            += Button_Pressed;
            _lSr.Clicked            += Button_Pressed;
            _rStickX.Clicked        += Button_Pressed;
            _rStickY.Clicked        += Button_Pressed;
            _rStickUp.Clicked       += Button_Pressed;
            _rStickDown.Clicked     += Button_Pressed;
            _rStickLeft.Clicked     += Button_Pressed;
            _rStickRight.Clicked    += Button_Pressed;
            _rStickButton.Clicked   += Button_Pressed;
            _a.Clicked              += Button_Pressed;
            _b.Clicked              += Button_Pressed;
            _x.Clicked              += Button_Pressed;
            _y.Clicked              += Button_Pressed;
            _plus.Clicked           += Button_Pressed;
            _r.Clicked              += Button_Pressed;
            _zR.Clicked             += Button_Pressed;
            _rSl.Clicked            += Button_Pressed;
            _rSr.Clicked            += Button_Pressed;

            // Setup current values.
            UpdateInputDeviceList();
            SetAvailableOptions();

            ClearValues();
            if (_inputDevice.ActiveId != null)
            {
                SetCurrentValues();
            }
        }

        private void UpdateInputDeviceList()
        {
            _inputDevice.RemoveAll();
            _inputDevice.Append("disabled", "Disabled");
            _inputDevice.SetActiveId("disabled");

            _inputDevice.Append($"keyboard/{KeyboardConfig.AllKeyboardsIndex}", "All keyboards");

            for (int i = 0; i < 20; i++)
            {
                if (KeyboardController.GetKeyboardState(i + 1).IsConnected)
                    _inputDevice.Append($"keyboard/{i + 1}", $"Keyboard/{i + 1}");

                if (GamePad.GetState(i).IsConnected)
                    _inputDevice.Append($"controller/{i}", $"Controller/{i} ({GamePad.GetName(i)})");
            }

            switch (_inputConfig)
            {
                case KeyboardConfig keyboard:
                    _inputDevice.SetActiveId($"keyboard/{keyboard.Index}");
                    break;
                case ControllerConfig controller:
                    _inputDevice.SetActiveId($"controller/{controller.Index}");
                    break;
            }
        }

        private void SetAvailableOptions()
        {
            if (_inputDevice.ActiveId != null && _inputDevice.ActiveId.StartsWith("keyboard"))
            {
                ShowAll();
                _leftStickController.Hide();
                _rightStickController.Hide();
                _deadZoneLeftBox.Hide();
                _deadZoneRightBox.Hide();
                _triggerThresholdBox.Hide();
            }
            else if (_inputDevice.ActiveId != null && _inputDevice.ActiveId.StartsWith("controller"))
            {
                ShowAll();
                _leftStickKeyboard.Hide();
                _rightStickKeyboard.Hide();
            }
            else
            {
                _settingsBox.Hide();
            }

            ClearValues();
        }

        private void SetCurrentValues()
        {
            SetControllerSpecificFields();

            SetProfiles();

            if (_inputDevice.ActiveId.StartsWith("keyboard") && _inputConfig is KeyboardConfig)
            {
                SetValues(_inputConfig);
            }
            else if (_inputDevice.ActiveId.StartsWith("controller") && _inputConfig is ControllerConfig)
            {
                SetValues(_inputConfig);
            }
        }

        private void SetControllerSpecificFields()
        {
            _leftSideTriggerBox.Hide();
            _rightSideTriggerBox.Hide();
            _altBox.Hide();

            switch (_controllerType.ActiveId)
            {
                case "JoyconLeft":
                    _leftSideTriggerBox.Show();
                    break;
                case "JoyconRight":
                    _rightSideTriggerBox.Show();
                    break;
                case "JoyconPair":
                    _altBox.Show();
                    break;
            }

            _controllerImage.Pixbuf = _controllerType.ActiveId switch
            {
                "ProController" => new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Controller_ProCon.svg", 400, 400),
                "JoyconLeft"    => new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Controller_JoyConLeft.svg", 400, 500),
                "JoyconRight"   => new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Controller_JoyConRight.svg", 400, 500),
                _               => new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Controller_JoyConPair.svg", 400, 500),
            };
        }

        private void ClearValues()
        {
            _lStickX.Label                    = "Unbound";
            _lStickY.Label                    = "Unbound";
            _lStickUp.Label                   = "Unbound";
            _lStickDown.Label                 = "Unbound";
            _lStickLeft.Label                 = "Unbound";
            _lStickRight.Label                = "Unbound";
            _lStickButton.Label               = "Unbound";
            _dpadUp.Label                     = "Unbound";
            _dpadDown.Label                   = "Unbound";
            _dpadLeft.Label                   = "Unbound";
            _dpadRight.Label                  = "Unbound";
            _minus.Label                      = "Unbound";
            _l.Label                          = "Unbound";
            _zL.Label                         = "Unbound";
            _lSl.Label                        = "Unbound";
            _lSr.Label                        = "Unbound";
            _rStickX.Label                    = "Unbound";
            _rStickY.Label                    = "Unbound";
            _rStickUp.Label                   = "Unbound";
            _rStickDown.Label                 = "Unbound";
            _rStickLeft.Label                 = "Unbound";
            _rStickRight.Label                = "Unbound";
            _rStickButton.Label               = "Unbound";
            _a.Label                          = "Unbound";
            _b.Label                          = "Unbound";
            _x.Label                          = "Unbound";
            _y.Label                          = "Unbound";
            _plus.Label                       = "Unbound";
            _r.Label                          = "Unbound";
            _zR.Label                         = "Unbound";
            _rSl.Label                        = "Unbound";
            _rSr.Label                        = "Unbound";
            _controllerDeadzoneLeft.Value     = 0;
            _controllerDeadzoneRight.Value    = 0;
            _controllerTriggerThreshold.Value = 0;
            _mirrorInput.Active               = false;
            _enableMotion.Active              = false;
            _slotNumber.Value                 = 0;
            _altSlotNumber.Value              = 0;
            _sensitivity.Value                = 100;
            _gyroDeadzone.Value               = 1;
            _dsuServerHost.Buffer.Text        = "";
            _dsuServerPort.Buffer.Text        = "";
        }

        private void SetValues(InputConfig config)
        {
            switch (config)
            {
                case KeyboardConfig keyboardConfig:
                    if (!_controllerType.SetActiveId(keyboardConfig.ControllerType.ToString()))
                    {
                        _controllerType.SetActiveId(_playerIndex == PlayerIndex.Handheld 
                            ? ControllerType.Handheld.ToString() 
                            : ControllerType.ProController.ToString());
                    }

                    _lStickUp.Label            = keyboardConfig.LeftJoycon.StickUp.ToString();
                    _lStickDown.Label          = keyboardConfig.LeftJoycon.StickDown.ToString();
                    _lStickLeft.Label          = keyboardConfig.LeftJoycon.StickLeft.ToString();
                    _lStickRight.Label         = keyboardConfig.LeftJoycon.StickRight.ToString();
                    _lStickButton.Label        = keyboardConfig.LeftJoycon.StickButton.ToString();
                    _dpadUp.Label              = keyboardConfig.LeftJoycon.DPadUp.ToString();
                    _dpadDown.Label            = keyboardConfig.LeftJoycon.DPadDown.ToString();
                    _dpadLeft.Label            = keyboardConfig.LeftJoycon.DPadLeft.ToString();
                    _dpadRight.Label           = keyboardConfig.LeftJoycon.DPadRight.ToString();
                    _minus.Label               = keyboardConfig.LeftJoycon.ButtonMinus.ToString();
                    _l.Label                   = keyboardConfig.LeftJoycon.ButtonL.ToString();
                    _zL.Label                  = keyboardConfig.LeftJoycon.ButtonZl.ToString();
                    _lSl.Label                 = keyboardConfig.LeftJoycon.ButtonSl.ToString();
                    _lSr.Label                 = keyboardConfig.LeftJoycon.ButtonSr.ToString();
                    _rStickUp.Label            = keyboardConfig.RightJoycon.StickUp.ToString();
                    _rStickDown.Label          = keyboardConfig.RightJoycon.StickDown.ToString();
                    _rStickLeft.Label          = keyboardConfig.RightJoycon.StickLeft.ToString();
                    _rStickRight.Label         = keyboardConfig.RightJoycon.StickRight.ToString();
                    _rStickButton.Label        = keyboardConfig.RightJoycon.StickButton.ToString();
                    _a.Label                   = keyboardConfig.RightJoycon.ButtonA.ToString();
                    _b.Label                   = keyboardConfig.RightJoycon.ButtonB.ToString();
                    _x.Label                   = keyboardConfig.RightJoycon.ButtonX.ToString();
                    _y.Label                   = keyboardConfig.RightJoycon.ButtonY.ToString();
                    _plus.Label                = keyboardConfig.RightJoycon.ButtonPlus.ToString();
                    _r.Label                   = keyboardConfig.RightJoycon.ButtonR.ToString();
                    _zR.Label                  = keyboardConfig.RightJoycon.ButtonZr.ToString();
                    _rSl.Label                 = keyboardConfig.RightJoycon.ButtonSl.ToString();
                    _rSr.Label                 = keyboardConfig.RightJoycon.ButtonSr.ToString();
                    _slotNumber.Value          = keyboardConfig.Slot;
                    _altSlotNumber.Value       = keyboardConfig.AltSlot;
                    _sensitivity.Value         = keyboardConfig.Sensitivity;
                    _gyroDeadzone.Value        = keyboardConfig.GyroDeadzone;
                    _enableMotion.Active       = keyboardConfig.EnableMotion;
                    _mirrorInput.Active        = keyboardConfig.MirrorInput;
                    _dsuServerHost.Buffer.Text = keyboardConfig.DsuServerHost;
                    _dsuServerPort.Buffer.Text = keyboardConfig.DsuServerPort.ToString();
                    break;
                case ControllerConfig controllerConfig:
                    if (!_controllerType.SetActiveId(controllerConfig.ControllerType.ToString()))
                    {
                        _controllerType.SetActiveId(_playerIndex == PlayerIndex.Handheld 
                            ? ControllerType.Handheld.ToString() 
                            : ControllerType.ProController.ToString());
                    }

                    _lStickX.Label                    = controllerConfig.LeftJoycon.StickX.ToString();
                    _invertLStickX.Active             = controllerConfig.LeftJoycon.InvertStickX;
                    _lStickY.Label                    = controllerConfig.LeftJoycon.StickY.ToString();
                    _invertLStickY.Active             = controllerConfig.LeftJoycon.InvertStickY;
                    _lStickButton.Label               = controllerConfig.LeftJoycon.StickButton.ToString();
                    _dpadUp.Label                     = controllerConfig.LeftJoycon.DPadUp.ToString();
                    _dpadDown.Label                   = controllerConfig.LeftJoycon.DPadDown.ToString();
                    _dpadLeft.Label                   = controllerConfig.LeftJoycon.DPadLeft.ToString();
                    _dpadRight.Label                  = controllerConfig.LeftJoycon.DPadRight.ToString();
                    _minus.Label                      = controllerConfig.LeftJoycon.ButtonMinus.ToString();
                    _l.Label                          = controllerConfig.LeftJoycon.ButtonL.ToString();
                    _zL.Label                         = controllerConfig.LeftJoycon.ButtonZl.ToString();
                    _lSl.Label                        = controllerConfig.LeftJoycon.ButtonSl.ToString();
                    _lSr.Label                        = controllerConfig.LeftJoycon.ButtonSr.ToString();
                    _rStickX.Label                    = controllerConfig.RightJoycon.StickX.ToString();
                    _invertRStickX.Active             = controllerConfig.RightJoycon.InvertStickX;
                    _rStickY.Label                    = controllerConfig.RightJoycon.StickY.ToString();
                    _invertRStickY.Active             = controllerConfig.RightJoycon.InvertStickY;
                    _rStickButton.Label               = controllerConfig.RightJoycon.StickButton.ToString();
                    _a.Label                          = controllerConfig.RightJoycon.ButtonA.ToString();
                    _b.Label                          = controllerConfig.RightJoycon.ButtonB.ToString();
                    _x.Label                          = controllerConfig.RightJoycon.ButtonX.ToString();
                    _y.Label                          = controllerConfig.RightJoycon.ButtonY.ToString();
                    _plus.Label                       = controllerConfig.RightJoycon.ButtonPlus.ToString();
                    _r.Label                          = controllerConfig.RightJoycon.ButtonR.ToString();
                    _zR.Label                         = controllerConfig.RightJoycon.ButtonZr.ToString();
                    _rSl.Label                        = controllerConfig.RightJoycon.ButtonSl.ToString();
                    _rSr.Label                        = controllerConfig.RightJoycon.ButtonSr.ToString();
                    _controllerDeadzoneLeft.Value     = controllerConfig.DeadzoneLeft;
                    _controllerDeadzoneRight.Value    = controllerConfig.DeadzoneRight;
                    _controllerTriggerThreshold.Value = controllerConfig.TriggerThreshold;
                    _slotNumber.Value                 = controllerConfig.Slot;
                    _altSlotNumber.Value              = controllerConfig.AltSlot;
                    _sensitivity.Value                = controllerConfig.Sensitivity;
                    _gyroDeadzone.Value               = controllerConfig.GyroDeadzone;
                    _enableMotion.Active              = controllerConfig.EnableMotion;
                    _mirrorInput.Active               = controllerConfig.MirrorInput;
                    _dsuServerHost.Buffer.Text        = controllerConfig.DsuServerHost;
                    _dsuServerPort.Buffer.Text        = controllerConfig.DsuServerPort.ToString();
                    break;
            }
        }

        private InputConfig GetValues()
        {
            if (_inputDevice.ActiveId.StartsWith("keyboard"))
            {
                Enum.TryParse(_lStickUp.Label,     out Key lStickUp);
                Enum.TryParse(_lStickDown.Label,   out Key lStickDown);
                Enum.TryParse(_lStickLeft.Label,   out Key lStickLeft);
                Enum.TryParse(_lStickRight.Label,  out Key lStickRight);
                Enum.TryParse(_lStickButton.Label, out Key lStickButton);
                Enum.TryParse(_dpadUp.Label,       out Key lDPadUp);
                Enum.TryParse(_dpadDown.Label,     out Key lDPadDown);
                Enum.TryParse(_dpadLeft.Label,     out Key lDPadLeft);
                Enum.TryParse(_dpadRight.Label,    out Key lDPadRight);
                Enum.TryParse(_minus.Label,        out Key lButtonMinus);
                Enum.TryParse(_l.Label,            out Key lButtonL);
                Enum.TryParse(_zL.Label,           out Key lButtonZl);
                Enum.TryParse(_lSl.Label,          out Key lButtonSl);
                Enum.TryParse(_lSr.Label,          out Key lButtonSr);

                Enum.TryParse(_rStickUp.Label,     out Key rStickUp);
                Enum.TryParse(_rStickDown.Label,   out Key rStickDown);
                Enum.TryParse(_rStickLeft.Label,   out Key rStickLeft);
                Enum.TryParse(_rStickRight.Label,  out Key rStickRight);
                Enum.TryParse(_rStickButton.Label, out Key rStickButton);
                Enum.TryParse(_a.Label,            out Key rButtonA);
                Enum.TryParse(_b.Label,            out Key rButtonB);
                Enum.TryParse(_x.Label,            out Key rButtonX);
                Enum.TryParse(_y.Label,            out Key rButtonY);
                Enum.TryParse(_plus.Label,         out Key rButtonPlus);
                Enum.TryParse(_r.Label,            out Key rButtonR);
                Enum.TryParse(_zR.Label,           out Key rButtonZr);
                Enum.TryParse(_rSl.Label,          out Key rButtonSl);
                Enum.TryParse(_rSr.Label,          out Key rButtonSr);

                int.TryParse(_dsuServerPort.Buffer.Text, out int port);

                return new KeyboardConfig
                {
                    Index          = int.Parse(_inputDevice.ActiveId.Split("/")[1]),
                    ControllerType = Enum.Parse<ControllerType>(_controllerType.ActiveId),
                    PlayerIndex    = _playerIndex,
                    LeftJoycon     = new NpadKeyboardLeft
                    {
                        StickUp     = lStickUp,
                        StickDown   = lStickDown,
                        StickLeft   = lStickLeft,
                        StickRight  = lStickRight,
                        StickButton = lStickButton,
                        DPadUp      = lDPadUp,
                        DPadDown    = lDPadDown,
                        DPadLeft    = lDPadLeft,
                        DPadRight   = lDPadRight,
                        ButtonMinus = lButtonMinus,
                        ButtonL     = lButtonL,
                        ButtonZl    = lButtonZl,
                        ButtonSl    = lButtonSl,
                        ButtonSr    = lButtonSr
                    },
                    RightJoycon    = new NpadKeyboardRight
                    {
                        StickUp     = rStickUp,
                        StickDown   = rStickDown,
                        StickLeft   = rStickLeft,
                        StickRight  = rStickRight,
                        StickButton = rStickButton,
                        ButtonA     = rButtonA,
                        ButtonB     = rButtonB,
                        ButtonX     = rButtonX,
                        ButtonY     = rButtonY,
                        ButtonPlus  = rButtonPlus,
                        ButtonR     = rButtonR,
                        ButtonZr    = rButtonZr,
                        ButtonSl    = rButtonSl,
                        ButtonSr    = rButtonSr
                    },
                    EnableMotion  = _enableMotion.Active,
                    MirrorInput   = _mirrorInput.Active,
                    Slot          = (int)_slotNumber.Value,
                    AltSlot       = (int)_altSlotNumber.Value,
                    Sensitivity   = (int)_sensitivity.Value,
                    GyroDeadzone  = _gyroDeadzone.Value,
                    DsuServerHost = _dsuServerHost.Buffer.Text,
                    DsuServerPort = port
                };
            }
            
            if (_inputDevice.ActiveId.StartsWith("controller"))
            {
                Enum.TryParse(_lStickX.Label,      out ControllerInputId lStickX);
                Enum.TryParse(_lStickY.Label,      out ControllerInputId lStickY);
                Enum.TryParse(_lStickButton.Label, out ControllerInputId lStickButton);
                Enum.TryParse(_minus.Label,        out ControllerInputId lButtonMinus);
                Enum.TryParse(_l.Label,            out ControllerInputId lButtonL);
                Enum.TryParse(_zL.Label,           out ControllerInputId lButtonZl);
                Enum.TryParse(_lSl.Label,          out ControllerInputId lButtonSl);
                Enum.TryParse(_lSr.Label,          out ControllerInputId lButtonSr);
                Enum.TryParse(_dpadUp.Label,       out ControllerInputId lDPadUp);
                Enum.TryParse(_dpadDown.Label,     out ControllerInputId lDPadDown);
                Enum.TryParse(_dpadLeft.Label,     out ControllerInputId lDPadLeft);
                Enum.TryParse(_dpadRight.Label,    out ControllerInputId lDPadRight);

                Enum.TryParse(_rStickX.Label,      out ControllerInputId rStickX);
                Enum.TryParse(_rStickY.Label,      out ControllerInputId rStickY);
                Enum.TryParse(_rStickButton.Label, out ControllerInputId rStickButton);
                Enum.TryParse(_a.Label,            out ControllerInputId rButtonA);
                Enum.TryParse(_b.Label,            out ControllerInputId rButtonB);
                Enum.TryParse(_x.Label,            out ControllerInputId rButtonX);
                Enum.TryParse(_y.Label,            out ControllerInputId rButtonY);
                Enum.TryParse(_plus.Label,         out ControllerInputId rButtonPlus);
                Enum.TryParse(_r.Label,            out ControllerInputId rButtonR);
                Enum.TryParse(_zR.Label,           out ControllerInputId rButtonZr);
                Enum.TryParse(_rSl.Label,          out ControllerInputId rButtonSl);
                Enum.TryParse(_rSr.Label,          out ControllerInputId rButtonSr);

                int.TryParse(_dsuServerPort.Buffer.Text, out int port);

                return new ControllerConfig
                {
                    Index            = int.Parse(_inputDevice.ActiveId.Split("/")[1]),
                    ControllerType   = Enum.Parse<ControllerType>(_controllerType.ActiveId),
                    PlayerIndex      = _playerIndex,
                    DeadzoneLeft     = (float)_controllerDeadzoneLeft.Value,
                    DeadzoneRight    = (float)_controllerDeadzoneRight.Value,
                    TriggerThreshold = (float)_controllerTriggerThreshold.Value,
                    LeftJoycon       = new NpadControllerLeft
                    {
                        InvertStickX = _invertLStickX.Active,
                        StickX       = lStickX,
                        InvertStickY = _invertLStickY.Active,
                        StickY       = lStickY,
                        StickButton  = lStickButton,
                        ButtonMinus  = lButtonMinus,
                        ButtonL      = lButtonL,
                        ButtonZl     = lButtonZl,
                        ButtonSl     = lButtonSl,
                        ButtonSr     = lButtonSr,
                        DPadUp       = lDPadUp,
                        DPadDown     = lDPadDown,
                        DPadLeft     = lDPadLeft,
                        DPadRight    = lDPadRight
                    },
                    RightJoycon      = new NpadControllerRight
                    {
                        InvertStickX = _invertRStickX.Active,
                        StickX       = rStickX,
                        InvertStickY = _invertRStickY.Active,
                        StickY       = rStickY,
                        StickButton  = rStickButton,
                        ButtonA      = rButtonA,
                        ButtonB      = rButtonB,
                        ButtonX      = rButtonX,
                        ButtonY      = rButtonY,
                        ButtonPlus   = rButtonPlus,
                        ButtonR      = rButtonR,
                        ButtonZr     = rButtonZr,
                        ButtonSl     = rButtonSl,
                        ButtonSr     = rButtonSr
                    },
                    EnableMotion  = _enableMotion.Active,
                    MirrorInput   = _mirrorInput.Active,
                    Slot          = (int)_slotNumber.Value,
                    AltSlot       = (int)_altSlotNumber.Value,
                    Sensitivity   = (int)_sensitivity.Value,
                    GyroDeadzone  = _gyroDeadzone.Value,
                    DsuServerHost = _dsuServerHost.Buffer.Text,
                    DsuServerPort = port
                };
            }

            if (!_inputDevice.ActiveId.StartsWith("disabled"))
            {
                GtkDialog.CreateErrorDialog("Some fields entered where invalid and therefore your config was not saved.");
            }

            return null;
        }

        private string GetProfileBasePath()
        {
            string path = AppDataManager.ProfilesDirPath;

            if (_inputDevice.ActiveId.StartsWith("keyboard"))
            {
                path = System.IO.Path.Combine(path, "keyboard");
            }
            else if (_inputDevice.ActiveId.StartsWith("controller"))
            {
                path = System.IO.Path.Combine(path, "controller");
            }

            return path;
        }

        //
        // Events
        //
        private void InputDevice_Changed(object sender, EventArgs args)
        {
            SetAvailableOptions();
            SetControllerSpecificFields();

            if (_inputDevice.ActiveId != null) SetProfiles();
        }

        private void Controller_Changed(object sender, EventArgs args)
        {
            SetControllerSpecificFields();
        }

        private void RefreshInputDevicesButton_Pressed(object sender, EventArgs args)
        {
            UpdateInputDeviceList();

            _refreshInputDevicesButton.SetStateFlags(StateFlags.Normal, true);
        }

        private ButtonAssigner CreateButtonAssigner()
        {
            int index = int.Parse(_inputDevice.ActiveId.Split("/")[1]);

            ButtonAssigner assigner;

            if (_inputDevice.ActiveId.StartsWith("keyboard"))
            {
                assigner = new KeyboardKeyAssigner(index);
            }
            else if (_inputDevice.ActiveId.StartsWith("controller"))
            {
                // TODO: triggerThresold is passed but not used by JoystickButtonAssigner. Should it be used for key binding?.
                // Note that, like left and right sticks, ZL and ZR triggers are treated as axis.
                // The problem is then how to decide which axis should use triggerThresold.
                assigner = new JoystickButtonAssigner(index, _controllerTriggerThreshold.Value);
            }
            else
            {
                throw new Exception("Controller not supported");
            }
            
            return assigner;
        }

        private void Button_Pressed(object sender, EventArgs args)
        {
            if (_isWaitingForInput)
            {
                return;
            }

            ButtonAssigner assigner = CreateButtonAssigner();

            _isWaitingForInput = true;

            Thread inputThread = new Thread(() =>
            {
                assigner.Init();

                while (true)
                {
                    Thread.Sleep(10);
                    assigner.ReadInput();

                    if (assigner.HasAnyButtonPressed() || assigner.ShouldCancel())
                    {
                        break;
                    }
                }

                string pressedButton = assigner.GetPressedButton();

                ToggleButton button = (ToggleButton) sender;

                Application.Invoke(delegate
                {
                    if (pressedButton != "")
                    {
                        button.Label = pressedButton;
                    }
                    
                    button.Active = false;
                    _isWaitingForInput = false;   
                });
            });

            inputThread.Name = "GUI.InputThread";
            inputThread.IsBackground = true;
            inputThread.Start();
        }

        private void SetProfiles()
        {
            string basePath = GetProfileBasePath();
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            _profile.RemoveAll();
            _profile.Append("default", "Default");

            foreach (string profile in Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories))
            {
                _profile.Append(System.IO.Path.GetFileName(profile), System.IO.Path.GetFileNameWithoutExtension(profile));
            }
        }

        private void ProfileLoad_Activated(object sender, EventArgs args)
        {
            ((ToggleButton)sender).SetStateFlags(StateFlags.Normal, true);

            if (_inputDevice.ActiveId == "disabled" || _profile.ActiveId == null) return;

            InputConfig config = null;
            int         pos    = _profile.Active;

            if (_profile.ActiveId == "default")
            {
                if (_inputDevice.ActiveId.StartsWith("keyboard"))
                {
                    config = new KeyboardConfig
                    {
                        Index          = 0,
                        ControllerType = ControllerType.JoyconPair,
                        LeftJoycon     = new NpadKeyboardLeft
                        {
                            StickUp     = Key.W,
                            StickDown   = Key.S,
                            StickLeft   = Key.A,
                            StickRight  = Key.D,
                            StickButton = Key.F,
                            DPadUp      = Key.Up,
                            DPadDown    = Key.Down,
                            DPadLeft    = Key.Left,
                            DPadRight   = Key.Right,
                            ButtonMinus = Key.Minus,
                            ButtonL     = Key.E,
                            ButtonZl    = Key.Q,
                            ButtonSl    = Key.Unbound,
                            ButtonSr    = Key.Unbound
                        },
                        RightJoycon    = new NpadKeyboardRight
                        {
                            StickUp     = Key.I,
                            StickDown   = Key.K,
                            StickLeft   = Key.J,
                            StickRight  = Key.L,
                            StickButton = Key.H,
                            ButtonA     = Key.Z,
                            ButtonB     = Key.X,
                            ButtonX     = Key.C,
                            ButtonY     = Key.V,
                            ButtonPlus  = Key.Plus,
                            ButtonR     = Key.U,
                            ButtonZr    = Key.O,
                            ButtonSl    = Key.Unbound,
                            ButtonSr    = Key.Unbound
                        },
                        EnableMotion  = false,
                        MirrorInput   = false,
                        Slot          = 0,
                        AltSlot       = 0,
                        Sensitivity   = 100,
                        GyroDeadzone  = 1,
                        DsuServerHost = "127.0.0.1",
                        DsuServerPort = 26760
                    };
                }
                else if (_inputDevice.ActiveId.StartsWith("controller"))
                {
                    config = new ControllerConfig
                    {
                        Index            = 0,
                        ControllerType   = ControllerType.ProController,
                        DeadzoneLeft     = 0.1f,
                        DeadzoneRight    = 0.1f,
                        TriggerThreshold = 0.5f,
                        LeftJoycon       = new NpadControllerLeft
                        {
                            StickX       = ControllerInputId.Axis0,
                            StickY       = ControllerInputId.Axis1,
                            StickButton  = ControllerInputId.Button8,
                            DPadUp       = ControllerInputId.Hat0Up,
                            DPadDown     = ControllerInputId.Hat0Down,
                            DPadLeft     = ControllerInputId.Hat0Left,
                            DPadRight    = ControllerInputId.Hat0Right,
                            ButtonMinus  = ControllerInputId.Button6,
                            ButtonL      = ControllerInputId.Button4,
                            ButtonZl     = ControllerInputId.Axis2,
                            ButtonSl     = ControllerInputId.Unbound,
                            ButtonSr     = ControllerInputId.Unbound,
                            InvertStickX = false,
                            InvertStickY = false
                        },
                        RightJoycon      = new NpadControllerRight
                        {
                            StickX       = ControllerInputId.Axis3,
                            StickY       = ControllerInputId.Axis4,
                            StickButton  = ControllerInputId.Button9,
                            ButtonA      = ControllerInputId.Button1,
                            ButtonB      = ControllerInputId.Button0,
                            ButtonX      = ControllerInputId.Button3,
                            ButtonY      = ControllerInputId.Button2,
                            ButtonPlus   = ControllerInputId.Button7,
                            ButtonR      = ControllerInputId.Button5,
                            ButtonZr     = ControllerInputId.Axis5,
                            ButtonSl     = ControllerInputId.Unbound,
                            ButtonSr     = ControllerInputId.Unbound,
                            InvertStickX = false,
                            InvertStickY = false
                        },
                        EnableMotion  = false,
                        MirrorInput   = false,
                        Slot          = 0,
                        AltSlot       = 0,
                        Sensitivity   = 100,
                        GyroDeadzone  = 1,
                        DsuServerHost = "127.0.0.1",
                        DsuServerPort = 26760
                    };
                }
            }
            else
            {
                string path = System.IO.Path.Combine(GetProfileBasePath(), _profile.ActiveId);

                if (!File.Exists(path))
                {
                    if (pos >= 0)
                    {
                        _profile.Remove(pos);
                    }

                    return;
                }

                try
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        config = JsonHelper.Deserialize<ControllerConfig>(stream);
                    }
                }
                catch (JsonException)
                {
                    try
                    {
                        using (Stream stream = File.OpenRead(path))
                        {
                            config = JsonHelper.Deserialize<KeyboardConfig>(stream);
                        }
                    }
                    catch { }
                }
            }

            SetValues(config);
        }

        private void ProfileAdd_Activated(object sender, EventArgs args)
        {
            ((ToggleButton)sender).SetStateFlags(StateFlags.Normal, true);

            if (_inputDevice.ActiveId == "disabled") return;

            InputConfig   inputConfig   = GetValues();
            ProfileDialog profileDialog = new ProfileDialog();

            if (inputConfig == null) return;

            if (profileDialog.Run() == (int)ResponseType.Ok)
            {
                string path = System.IO.Path.Combine(GetProfileBasePath(), profileDialog.FileName);
                string jsonString;

                if (inputConfig is KeyboardConfig keyboardConfig)
                {
                    jsonString = JsonHelper.Serialize(keyboardConfig, true);
                }
                else
                {
                    jsonString = JsonHelper.Serialize(inputConfig as ControllerConfig, true);
                }

                File.WriteAllText(path, jsonString);
            }

            profileDialog.Dispose();

            SetProfiles();
        }

        private void ProfileRemove_Activated(object sender, EventArgs args)
        {
            ((ToggleButton) sender).SetStateFlags(StateFlags.Normal, true);

            if (_inputDevice.ActiveId == "disabled" || _profile.ActiveId == "default" || _profile.ActiveId == null) return;

            MessageDialog confirmDialog = GtkDialog.CreateConfirmationDialog("Deleting Profile", "This action is irreversible, are your sure you want to continue?");

            if (confirmDialog.Run() == (int)ResponseType.Yes)
            {
                string path = System.IO.Path.Combine(GetProfileBasePath(), _profile.ActiveId);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                SetProfiles();
            }
        }

        private void SaveToggle_Activated(object sender, EventArgs args)
        {
            InputConfig inputConfig = GetValues();

            var newConfig = new List<InputConfig>();
            newConfig.AddRange(ConfigurationState.Instance.Hid.InputConfig.Value);

            if (_inputConfig == null && inputConfig != null)
            {
                newConfig.Add(inputConfig);
            }
            else
            {
                if (_inputDevice.ActiveId == "disabled")
                {
                    newConfig.Remove(_inputConfig);
                }
                else if (inputConfig != null)
                {
                    int index = newConfig.IndexOf(_inputConfig);

                    newConfig[index] = inputConfig;
                }
            }

            // Atomically replace and signal input change.
            // NOTE: Do not modify InputConfig.Value directly as other code depends on the on-change event.
            ConfigurationState.Instance.Hid.InputConfig.Value = newConfig;

            ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

            Dispose();
        }

        private void CloseToggle_Activated(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}