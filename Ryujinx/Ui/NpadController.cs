using OpenTK;
using OpenTK.Input;
using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.UI.Input
{
    public enum ControllerInputId
    {
        Button0,
        Button1,
        Button2,
        Button3,
        Button4,
        Button5,
        Button6,
        Button7,
        Button8,
        Button9,
        Button10,
        Button11,
        Button12,
        Button13,
        Button14,
        Button15,
        Button16,
        Button17,
        Button18,
        Button19,
        Button20,
        Axis0,
        Axis1,
        Axis2,
        Axis3,
        Axis4,
        Axis5,
        Hat0Up,
        Hat0Down,
        Hat0Left,
        Hat0Right,
        Hat1Up,
        Hat1Down,
        Hat1Left,
        Hat1Right,
        Hat2Up,
        Hat2Down,
        Hat2Left,
        Hat2Right,
    }

    public struct NpadControllerLeft
    {
        public ControllerInputId Stick;
        public ControllerInputId StickButton;
        public ControllerInputId ButtonMinus;
        public ControllerInputId ButtonL;
        public ControllerInputId ButtonZl;
        public ControllerInputId DPadUp;
        public ControllerInputId DPadDown;
        public ControllerInputId DPadLeft;
        public ControllerInputId DPadRight;
    }

    public struct NpadControllerRight
    {
        public ControllerInputId Stick;
        public ControllerInputId StickY;
        public ControllerInputId StickButton;
        public ControllerInputId ButtonA;
        public ControllerInputId ButtonB;
        public ControllerInputId ButtonX;
        public ControllerInputId ButtonY;
        public ControllerInputId ButtonPlus;
        public ControllerInputId ButtonR;
        public ControllerInputId ButtonZr;
    }

    public class NpadController
    {
        /// <summary>
        /// Enables or disables controller support
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Controller Device Index
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Controller Analog Stick Deadzone
        /// </summary>
        public float Deadzone { get; private set; }

        /// <summary>
        /// Controller Trigger Threshold
        /// </summary>
        public float TriggerThreshold { get; private set; }

        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public NpadControllerLeft LeftJoycon { get; private set; }

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public NpadControllerRight RightJoycon { get; private set; }

        public NpadController(
            bool                enabled,
            int                 index,
            float               deadzone,
            float               triggerThreshold,
            NpadControllerLeft  leftJoycon,
            NpadControllerRight rightJoycon)
        {
            Enabled          = enabled;
            Index            = index;
            Deadzone         = deadzone;
            TriggerThreshold = triggerThreshold;
            LeftJoycon       = leftJoycon;
            RightJoycon      = rightJoycon;
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        public ControllerButtons GetButtons()
        {
            if (!Enabled)
            {
                return 0;
            }

            JoystickState joystickState = Joystick.GetState(Index);

            ControllerButtons buttons = 0;

            if (IsActivated(joystickState, LeftJoycon.DPadUp))       buttons |= ControllerButtons.DpadUp;
            if (IsActivated(joystickState, LeftJoycon.DPadDown))     buttons |= ControllerButtons.DpadDown;
            if (IsActivated(joystickState, LeftJoycon.DPadLeft))     buttons |= ControllerButtons.DpadLeft;
            if (IsActivated(joystickState, LeftJoycon.DPadRight))    buttons |= ControllerButtons.DPadRight;
            if (IsActivated(joystickState, LeftJoycon.StickButton))  buttons |= ControllerButtons.StickLeft;
            if (IsActivated(joystickState, LeftJoycon.ButtonMinus))  buttons |= ControllerButtons.Minus;
            if (IsActivated(joystickState, LeftJoycon.ButtonL))      buttons |= ControllerButtons.L;
            if (IsActivated(joystickState, LeftJoycon.ButtonZl))     buttons |= ControllerButtons.Zl;

            if (IsActivated(joystickState, RightJoycon.ButtonA))     buttons |= ControllerButtons.A;
            if (IsActivated(joystickState, RightJoycon.ButtonB))     buttons |= ControllerButtons.B;
            if (IsActivated(joystickState, RightJoycon.ButtonX))     buttons |= ControllerButtons.X;
            if (IsActivated(joystickState, RightJoycon.ButtonY))     buttons |= ControllerButtons.Y;
            if (IsActivated(joystickState, RightJoycon.StickButton)) buttons |= ControllerButtons.StickRight;
            if (IsActivated(joystickState, RightJoycon.ButtonPlus))  buttons |= ControllerButtons.Plus;
            if (IsActivated(joystickState, RightJoycon.ButtonR))     buttons |= ControllerButtons.R;
            if (IsActivated(joystickState, RightJoycon.ButtonZr))    buttons |= ControllerButtons.Zr;

            return buttons;
        }

        private bool IsActivated(JoystickState joystickState,ControllerInputId controllerInputId)
        {
            if (controllerInputId <= ControllerInputId.Button20)
            {
                return joystickState.IsButtonDown((int)controllerInputId);
            }
            else if (controllerInputId <= ControllerInputId.Axis5)
            {
                int axis = controllerInputId - ControllerInputId.Axis0;

                return Math.Abs(joystickState.GetAxis(axis)) > Deadzone;
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
            if (!Enabled)
            {
                return (0, 0);
            }

            return GetStick(LeftJoycon.Stick);
        }

        public (short, short) GetRightStick()
        {
            if (!Enabled)
            {
                return (0, 0);
            }

            return GetStick(RightJoycon.Stick);
        }

        private (short, short) GetStick(ControllerInputId stickInputId)
        {
            if (stickInputId < ControllerInputId.Axis0 || stickInputId > ControllerInputId.Axis5)
            {
                return (0, 0);
            }

            JoystickState jsState = Joystick.GetState(Index);

            int xAxis = stickInputId - ControllerInputId.Axis0;

            float xValue = jsState.GetAxis(xAxis);
            float yValue = 0 - jsState.GetAxis(xAxis + 1); // Invert Y-axis

            return ApplyDeadzone(new Vector2(xValue, yValue));
        }

        private (short, short) ApplyDeadzone(Vector2 axis)
        {
            return (ClampAxis(MathF.Abs(axis.X) > Deadzone ? axis.X : 0f),
                    ClampAxis(MathF.Abs(axis.Y) > Deadzone ? axis.Y : 0f));
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
