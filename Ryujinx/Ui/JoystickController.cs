using OpenTK;
using OpenTK.Input;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.HLE.HOS.Services.Hid;
using System;

using ControllerConfig = Ryujinx.Common.Configuration.Hid.ControllerConfig;

namespace Ryujinx.Ui
{
    public class JoystickController
    {
        private readonly ControllerConfig _config;

        public JoystickController(ControllerConfig config)
        {
            _config = config;
        }

        private bool IsEnabled()
        {
            return Joystick.GetState(_config.Index).IsConnected;
        }

        public ControllerKeys GetButtons()
        {
            // NOTE: This should be initialized AFTER GTK for compat reasons with OpenTK SDL2 backend and GTK on Linux.
            // BODY: Usage of Joystick.GetState must be defer to after GTK full initialization. Otherwise, GTK will segfault because SDL2 was already init *sighs*
            if (!IsEnabled())
            {
                return 0;
            }

            JoystickState joystickState = Joystick.GetState(_config.Index);

            ControllerKeys buttons = 0;

            if (IsActivated(joystickState, _config.LeftJoycon.DPadUp))       buttons |= ControllerKeys.DpadUp;
            if (IsActivated(joystickState, _config.LeftJoycon.DPadDown))     buttons |= ControllerKeys.DpadDown;
            if (IsActivated(joystickState, _config.LeftJoycon.DPadLeft))     buttons |= ControllerKeys.DpadLeft;
            if (IsActivated(joystickState, _config.LeftJoycon.DPadRight))    buttons |= ControllerKeys.DpadRight;
            if (IsActivated(joystickState, _config.LeftJoycon.StickButton))  buttons |= ControllerKeys.LStick;
            if (IsActivated(joystickState, _config.LeftJoycon.ButtonMinus))  buttons |= ControllerKeys.Minus;
            if (IsActivated(joystickState, _config.LeftJoycon.ButtonL))      buttons |= ControllerKeys.L;
            if (IsActivated(joystickState, _config.LeftJoycon.ButtonZl))     buttons |= ControllerKeys.Zl;
            if (IsActivated(joystickState, _config.LeftJoycon.ButtonSl))     buttons |= ControllerKeys.SlLeft;
            if (IsActivated(joystickState, _config.LeftJoycon.ButtonSr))     buttons |= ControllerKeys.SrLeft;

            if (IsActivated(joystickState, _config.RightJoycon.ButtonA))     buttons |= ControllerKeys.A;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonB))     buttons |= ControllerKeys.B;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonX))     buttons |= ControllerKeys.X;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonY))     buttons |= ControllerKeys.Y;
            if (IsActivated(joystickState, _config.RightJoycon.StickButton)) buttons |= ControllerKeys.RStick;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonPlus))  buttons |= ControllerKeys.Plus;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonR))     buttons |= ControllerKeys.R;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonZr))    buttons |= ControllerKeys.Zr;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonSl))    buttons |= ControllerKeys.SlRight;
            if (IsActivated(joystickState, _config.RightJoycon.ButtonSr))    buttons |= ControllerKeys.SrRight;

            return buttons;
        }

        private bool IsActivated(JoystickState joystickState, ControllerInputId controllerInputId)
        {
            if (controllerInputId <= ControllerInputId.Button20)
            {
                return joystickState.IsButtonDown((int)controllerInputId);
            }
            else if (controllerInputId <= ControllerInputId.Axis5)
            {
                int axis = controllerInputId - ControllerInputId.Axis0;

                return joystickState.GetAxis(axis) > _config.TriggerThreshold;
            }
            else if (controllerInputId <= ControllerInputId.Hat2Right)
            {
                int hat = (controllerInputId - ControllerInputId.Hat0Up) / 4;

                int baseHatId = (int)ControllerInputId.Hat0Up + (hat * 4);

                JoystickHatState hatState = joystickState.GetHat((JoystickHat)hat);

                if (hatState.IsUp    && ((int)controllerInputId % baseHatId == 0)) return true;
                if (hatState.IsDown  && ((int)controllerInputId % baseHatId == 1)) return true;
                if (hatState.IsLeft  && ((int)controllerInputId % baseHatId == 2)) return true;
                if (hatState.IsRight && ((int)controllerInputId % baseHatId == 3)) return true;
            }

            return false;
        }

        public (short, short) GetLeftStick()
        {
            if (!IsEnabled())
            {
                return (0, 0);
            }

            return GetStick(_config.LeftJoycon.StickX, _config.LeftJoycon.StickY, _config.DeadzoneLeft);
        }

        public (short, short) GetRightStick()
        {
            if (!IsEnabled())
            {
                return (0, 0);
            }

            return GetStick(_config.RightJoycon.StickX, _config.RightJoycon.StickY, _config.DeadzoneRight);
        }

        private (short, short) GetStick(ControllerInputId stickXInputId, ControllerInputId stickYInputId, float deadzone)
        {
            if (stickXInputId < ControllerInputId.Axis0 || stickXInputId > ControllerInputId.Axis5 || 
                stickYInputId < ControllerInputId.Axis0 || stickYInputId > ControllerInputId.Axis5)
            {
                return (0, 0);
            }

            JoystickState jsState = Joystick.GetState(_config.Index);

            int xAxis = stickXInputId - ControllerInputId.Axis0;
            int yAxis = stickYInputId - ControllerInputId.Axis0;

            float xValue =  jsState.GetAxis(xAxis);
            float yValue = -jsState.GetAxis(yAxis); // Invert Y-axis

            return ApplyDeadzone(new Vector2(xValue, yValue), deadzone);
        }

        private (short, short) ApplyDeadzone(Vector2 axis, float deadzone)
        {
            return (ClampAxis(MathF.Abs(axis.X) > deadzone ? axis.X : 0f),
                    ClampAxis(MathF.Abs(axis.Y) > deadzone ? axis.Y : 0f));
        }

        private static short ClampAxis(float value)
        {
            if (value <= -short.MaxValue)
            {
                return -short.MaxValue;
            }
            else
            {
                return (short)(value * short.MaxValue);
            }
        }
    }
}
