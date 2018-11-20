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
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Back,
        LShoulder,

        RStick,
        A,
        B,
        X,
        Y,
        Start,
        RShoulder,

        LTrigger,
        RTrigger,

        LJoystick,
        RJoystick
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
        public bool  Enabled          { private set; get; }
        public int   Index            { private set; get; }
        public float Deadzone         { private set; get; }
        public float TriggerThreshold { private set; get; }

        public NpadControllerLeft  Left  { private set; get; }
        public NpadControllerRight Right { private set; get; }

        public NpadController(
            bool                  enabled,
            int                   index,
            float                 deadzone,
            float                 triggerThreshold,
            NpadControllerLeft    left,
            NpadControllerRight   right)
        {
            Enabled          = enabled;
            Index            = index;
            Deadzone         = deadzone;
            TriggerThreshold = triggerThreshold;
            Left             = left;
            Right            = right;

            //Unmapped controllers are problematic, skip them
            if (GamePad.GetName(index) == "Unmapped Controller")
            {
                Enabled = false;
            }
        }

        public HidControllerButtons GetButtons()
        {
            if (!Enabled)
            {
                return 0;
            }

            GamePadState gpState = GamePad.GetState(Index);

            HidControllerButtons buttons = 0;

            if (IsPressed(gpState, Left.DPadUp))       buttons |= HidControllerButtons.DpadUp;
            if (IsPressed(gpState, Left.DPadDown))     buttons |= HidControllerButtons.DpadDown;
            if (IsPressed(gpState, Left.DPadLeft))     buttons |= HidControllerButtons.DpadLeft;
            if (IsPressed(gpState, Left.DPadRight))    buttons |= HidControllerButtons.DPadRight;
            if (IsPressed(gpState, Left.StickButton))  buttons |= HidControllerButtons.StickLeft;
            if (IsPressed(gpState, Left.ButtonMinus))  buttons |= HidControllerButtons.Minus;
            if (IsPressed(gpState, Left.ButtonL))      buttons |= HidControllerButtons.L;
            if (IsPressed(gpState, Left.ButtonZl))     buttons |= HidControllerButtons.Zl;

            if (IsPressed(gpState, Right.ButtonA))     buttons |= HidControllerButtons.A;
            if (IsPressed(gpState, Right.ButtonB))     buttons |= HidControllerButtons.B;
            if (IsPressed(gpState, Right.ButtonX))     buttons |= HidControllerButtons.X;
            if (IsPressed(gpState, Right.ButtonY))     buttons |= HidControllerButtons.Y;
            if (IsPressed(gpState, Right.StickButton)) buttons |= HidControllerButtons.StickRight;
            if (IsPressed(gpState, Right.ButtonPlus))  buttons |= HidControllerButtons.Plus;
            if (IsPressed(gpState, Right.ButtonR))     buttons |= HidControllerButtons.R;
            if (IsPressed(gpState, Right.ButtonZr))    buttons |= HidControllerButtons.Zr;

            return buttons;
        }

        public (short, short) GetLeftStick()
        {
            if (!Enabled)
            {
                return (0, 0);
            }

            return GetStick(Left.Stick);
        }

        public (short, short) GetRightStick()
        {
            if (!Enabled)
            {
                return (0, 0);
            }

            return GetStick(Right.Stick);
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
