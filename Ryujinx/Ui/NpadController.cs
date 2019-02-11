using OpenTK;
using OpenTK.Input;
using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.UI.Input
{
    public enum ControllerInputId
    {
        Invalid,
        LStick,
        RStick,
        LShoulder,
        RShoulder,
        LTrigger,
        RTrigger,
        LJoystick,
        RJoystick,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Start,
        Back,
        A,
        B,
        X,
        Y
    }

    public struct NpadControllerLeft
    {
        public ControllerInputId Stick;
        public ControllerInputId StickButton;
        public ControllerInputId DPadUp;
        public ControllerInputId DPadDown;
        public ControllerInputId DPadLeft;
        public ControllerInputId DPadRight;
        public ControllerInputId ButtonMinus;
        public ControllerInputId ButtonL;
        public ControllerInputId ButtonZl;
    }

    public struct NpadControllerRight
    {
        public ControllerInputId Stick;
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

        public HidControllerButtons GetButtons()
        {
            if (!Enabled)
            {
                return 0;
            }

            GamePadState gpState = GamePad.GetState(Index);

            HidControllerButtons buttons = 0;

            if (IsPressed(gpState, LeftJoycon.DPadUp))       buttons |= HidControllerButtons.DpadUp;
            if (IsPressed(gpState, LeftJoycon.DPadDown))     buttons |= HidControllerButtons.DpadDown;
            if (IsPressed(gpState, LeftJoycon.DPadLeft))     buttons |= HidControllerButtons.DpadLeft;
            if (IsPressed(gpState, LeftJoycon.DPadRight))    buttons |= HidControllerButtons.DPadRight;
            if (IsPressed(gpState, LeftJoycon.StickButton))  buttons |= HidControllerButtons.StickLeft;
            if (IsPressed(gpState, LeftJoycon.ButtonMinus))  buttons |= HidControllerButtons.Minus;
            if (IsPressed(gpState, LeftJoycon.ButtonL))      buttons |= HidControllerButtons.L;
            if (IsPressed(gpState, LeftJoycon.ButtonZl))     buttons |= HidControllerButtons.Zl;

            if (IsPressed(gpState, RightJoycon.ButtonA))     buttons |= HidControllerButtons.A;
            if (IsPressed(gpState, RightJoycon.ButtonB))     buttons |= HidControllerButtons.B;
            if (IsPressed(gpState, RightJoycon.ButtonX))     buttons |= HidControllerButtons.X;
            if (IsPressed(gpState, RightJoycon.ButtonY))     buttons |= HidControllerButtons.Y;
            if (IsPressed(gpState, RightJoycon.StickButton)) buttons |= HidControllerButtons.StickRight;
            if (IsPressed(gpState, RightJoycon.ButtonPlus))  buttons |= HidControllerButtons.Plus;
            if (IsPressed(gpState, RightJoycon.ButtonR))     buttons |= HidControllerButtons.R;
            if (IsPressed(gpState, RightJoycon.ButtonZr))    buttons |= HidControllerButtons.Zr;

            return buttons;
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

        private (short, short) GetStick(ControllerInputId joystick)
        {
            GamePadState gpState = GamePad.GetState(Index);

            switch (joystick)
            {
                case ControllerInputId.LJoystick:
                    return ApplyDeadzone(gpState.ThumbSticks.Left);

                case ControllerInputId.RJoystick:
                    return ApplyDeadzone(gpState.ThumbSticks.Right);

                default:
                    return (0, 0);
            }
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

        private bool IsPressed(GamePadState gpState, ControllerInputId button)
        {
            switch (button)
            {
                case ControllerInputId.A:         return gpState.Buttons.A             == ButtonState.Pressed;
                case ControllerInputId.B:         return gpState.Buttons.B             == ButtonState.Pressed;
                case ControllerInputId.X:         return gpState.Buttons.X             == ButtonState.Pressed;
                case ControllerInputId.Y:         return gpState.Buttons.Y             == ButtonState.Pressed;
                case ControllerInputId.LStick:    return gpState.Buttons.LeftStick     == ButtonState.Pressed;
                case ControllerInputId.RStick:    return gpState.Buttons.RightStick    == ButtonState.Pressed;
                case ControllerInputId.LShoulder: return gpState.Buttons.LeftShoulder  == ButtonState.Pressed;
                case ControllerInputId.RShoulder: return gpState.Buttons.RightShoulder == ButtonState.Pressed;
                case ControllerInputId.DPadUp:    return gpState.DPad.Up               == ButtonState.Pressed;
                case ControllerInputId.DPadDown:  return gpState.DPad.Down             == ButtonState.Pressed;
                case ControllerInputId.DPadLeft:  return gpState.DPad.Left             == ButtonState.Pressed;
                case ControllerInputId.DPadRight: return gpState.DPad.Right            == ButtonState.Pressed;
                case ControllerInputId.Start:     return gpState.Buttons.Start         == ButtonState.Pressed;
                case ControllerInputId.Back:      return gpState.Buttons.Back          == ButtonState.Pressed;

                case ControllerInputId.LTrigger: return gpState.Triggers.Left  >= TriggerThreshold;
                case ControllerInputId.RTrigger: return gpState.Triggers.Right >= TriggerThreshold;

                //Using thumbsticks as buttons is not common, but it would be nice not to ignore them
                case ControllerInputId.LJoystick:
                    return gpState.ThumbSticks.Left.X >= Deadzone ||
                           gpState.ThumbSticks.Left.Y >= Deadzone;

                case ControllerInputId.RJoystick:
                    return gpState.ThumbSticks.Right.X >= Deadzone ||
                           gpState.ThumbSticks.Right.Y >= Deadzone;

                default:
                    return false;
            }
        }
    }
}
