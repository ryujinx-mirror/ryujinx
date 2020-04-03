using OpenTK;
using OpenTK.Input;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.HLE.HOS.Services.Hid;
using System;

using InnerNpadController = Ryujinx.Common.Configuration.Hid.NpadController;

namespace Ryujinx.Ui.Input
{
    public class NpadController
    {
        private InnerNpadController _inner;

        // NOTE: This should be initialized AFTER GTK for compat reasons with OpenTK SDL2 backend and GTK on Linux.
        // BODY: Usage of Joystick.GetState must be defer to after GTK full initialization. Otherwise, GTK will segfault because SDL2 was already init *sighs*
        public NpadController(InnerNpadController inner)
        {
            _inner = inner;
        }

        private bool IsEnabled()
        {
            return _inner.Enabled && Joystick.GetState(_inner.Index).IsConnected;
        }

        public ControllerKeys GetButtons()
        {
            if (!IsEnabled())
            {
                return 0;
            }

            JoystickState joystickState = Joystick.GetState(_inner.Index);

            ControllerKeys buttons = 0;

            if (IsActivated(joystickState, _inner.LeftJoycon.DPadUp))       buttons |= ControllerKeys.DpadUp;
            if (IsActivated(joystickState, _inner.LeftJoycon.DPadDown))     buttons |= ControllerKeys.DpadDown;
            if (IsActivated(joystickState, _inner.LeftJoycon.DPadLeft))     buttons |= ControllerKeys.DpadLeft;
            if (IsActivated(joystickState, _inner.LeftJoycon.DPadRight))    buttons |= ControllerKeys.DpadRight;
            if (IsActivated(joystickState, _inner.LeftJoycon.StickButton))  buttons |= ControllerKeys.LStick;
            if (IsActivated(joystickState, _inner.LeftJoycon.ButtonMinus))  buttons |= ControllerKeys.Minus;
            if (IsActivated(joystickState, _inner.LeftJoycon.ButtonL))      buttons |= ControllerKeys.L | ControllerKeys.Sl;
            if (IsActivated(joystickState, _inner.LeftJoycon.ButtonZl))     buttons |= ControllerKeys.Zl;

            if (IsActivated(joystickState, _inner.RightJoycon.ButtonA))     buttons |= ControllerKeys.A;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonB))     buttons |= ControllerKeys.B;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonX))     buttons |= ControllerKeys.X;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonY))     buttons |= ControllerKeys.Y;
            if (IsActivated(joystickState, _inner.RightJoycon.StickButton)) buttons |= ControllerKeys.RStick;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonPlus))  buttons |= ControllerKeys.Plus;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonR))     buttons |= ControllerKeys.R | ControllerKeys.Sr;
            if (IsActivated(joystickState, _inner.RightJoycon.ButtonZr))    buttons |= ControllerKeys.Zr;

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

                return joystickState.GetAxis(axis) > _inner.TriggerThreshold;
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

            return GetStick(_inner.LeftJoycon.Stick);
        }

        public (short, short) GetRightStick()
        {
            if (!IsEnabled())
            {
                return (0, 0);
            }

            return GetStick(_inner.RightJoycon.Stick);
        }

        private (short, short) GetStick(ControllerInputId stickInputId)
        {
            if (stickInputId < ControllerInputId.Axis0 || stickInputId > ControllerInputId.Axis5)
            {
                return (0, 0);
            }

            JoystickState jsState = Joystick.GetState(_inner.Index);

            int xAxis = stickInputId - ControllerInputId.Axis0;

            float xValue = jsState.GetAxis(xAxis);
            float yValue = 0 - jsState.GetAxis(xAxis + 1); // Invert Y-axis

            return ApplyDeadzone(new Vector2(xValue, yValue));
        }

        private (short, short) ApplyDeadzone(Vector2 axis)
        {
            return (ClampAxis(MathF.Abs(axis.X) > _inner.Deadzone ? axis.X : 0f),
                    ClampAxis(MathF.Abs(axis.Y) > _inner.Deadzone ? axis.Y : 0f));
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
