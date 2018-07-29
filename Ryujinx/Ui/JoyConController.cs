using OpenTK;
using OpenTK.Input;
using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.UI.Input
{
    public enum ControllerInputID
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

    public struct JoyConControllerLeft
    {
        public ControllerInputID Stick;
        public ControllerInputID StickButton;
        public ControllerInputID DPadUp;
        public ControllerInputID DPadDown;
        public ControllerInputID DPadLeft;
        public ControllerInputID DPadRight;
        public ControllerInputID ButtonMinus;
        public ControllerInputID ButtonL;
        public ControllerInputID ButtonZL;
    }

    public struct JoyConControllerRight
    {
        public ControllerInputID Stick;
        public ControllerInputID StickButton;
        public ControllerInputID ButtonA;
        public ControllerInputID ButtonB;
        public ControllerInputID ButtonX;
        public ControllerInputID ButtonY;
        public ControllerInputID ButtonPlus;
        public ControllerInputID ButtonR;
        public ControllerInputID ButtonZR;
    }

    public class JoyConController
    {
        public bool  Enabled          { private set; get; }
        public int   Index            { private set; get; }
        public float Deadzone         { private set; get; }
        public float TriggerThreshold { private set; get; }

        public JoyConControllerLeft  Left  { private set; get; }
        public JoyConControllerRight Right { private set; get; }

        public JoyConController(
            bool                  Enabled,
            int                   Index,
            float                 Deadzone,
            float                 TriggerThreshold,
            JoyConControllerLeft  Left,
            JoyConControllerRight Right)
        {
            this.Enabled          = Enabled;
            this.Index            = Index;
            this.Deadzone         = Deadzone;
            this.TriggerThreshold = TriggerThreshold;
            this.Left             = Left;
            this.Right            = Right;

            //Unmapped controllers are problematic, skip them
            if (GamePad.GetName(Index) == "Unmapped Controller")
            {
                this.Enabled = false;
            }
        }

        public HidControllerButtons GetButtons()
        {
            if (!Enabled)
            {
                return 0;
            }

            GamePadState GpState = GamePad.GetState(Index);

            HidControllerButtons Buttons = 0;

            if (IsPressed(GpState, Left.DPadUp))       Buttons |= HidControllerButtons.KEY_DUP;
            if (IsPressed(GpState, Left.DPadDown))     Buttons |= HidControllerButtons.KEY_DDOWN;
            if (IsPressed(GpState, Left.DPadLeft))     Buttons |= HidControllerButtons.KEY_DLEFT;
            if (IsPressed(GpState, Left.DPadRight))    Buttons |= HidControllerButtons.KEY_DRIGHT;
            if (IsPressed(GpState, Left.StickButton))  Buttons |= HidControllerButtons.KEY_LSTICK;
            if (IsPressed(GpState, Left.ButtonMinus))  Buttons |= HidControllerButtons.KEY_MINUS;
            if (IsPressed(GpState, Left.ButtonL))      Buttons |= HidControllerButtons.KEY_L;
            if (IsPressed(GpState, Left.ButtonZL))     Buttons |= HidControllerButtons.KEY_ZL;

            if (IsPressed(GpState, Right.ButtonA))     Buttons |= HidControllerButtons.KEY_A;
            if (IsPressed(GpState, Right.ButtonB))     Buttons |= HidControllerButtons.KEY_B;
            if (IsPressed(GpState, Right.ButtonX))     Buttons |= HidControllerButtons.KEY_X;
            if (IsPressed(GpState, Right.ButtonY))     Buttons |= HidControllerButtons.KEY_Y;
            if (IsPressed(GpState, Right.StickButton)) Buttons |= HidControllerButtons.KEY_RSTICK;
            if (IsPressed(GpState, Right.ButtonPlus))  Buttons |= HidControllerButtons.KEY_PLUS;
            if (IsPressed(GpState, Right.ButtonR))     Buttons |= HidControllerButtons.KEY_R;
            if (IsPressed(GpState, Right.ButtonZR))    Buttons |= HidControllerButtons.KEY_ZR;

            return Buttons;
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

        private (short, short) GetStick(ControllerInputID Joystick)
        {
            GamePadState GpState = GamePad.GetState(Index);

            switch (Joystick)
            {
                case ControllerInputID.LJoystick:
                    return ApplyDeadzone(GpState.ThumbSticks.Left);

                case ControllerInputID.RJoystick:
                    return ApplyDeadzone(GpState.ThumbSticks.Right);

                default:
                    return (0, 0);
            }
        }

        private (short, short) ApplyDeadzone(Vector2 Axis)
        {
            return (ClampAxis(MathF.Abs(Axis.X) > Deadzone ? Axis.X : 0f),
                    ClampAxis(MathF.Abs(Axis.Y) > Deadzone ? Axis.Y : 0f));
        }

        private static short ClampAxis(float Value)
        {
            if (Value <= -short.MaxValue)
            {
                return -short.MaxValue;
            }
            else
            {
                return (short)(Value * short.MaxValue);
            }
        }

        private bool IsPressed(GamePadState GpState, ControllerInputID Button)
        {
            switch (Button)
            {
                case ControllerInputID.A:         return GpState.Buttons.A             == ButtonState.Pressed;
                case ControllerInputID.B:         return GpState.Buttons.B             == ButtonState.Pressed;
                case ControllerInputID.X:         return GpState.Buttons.X             == ButtonState.Pressed;
                case ControllerInputID.Y:         return GpState.Buttons.Y             == ButtonState.Pressed;
                case ControllerInputID.LStick:    return GpState.Buttons.LeftStick     == ButtonState.Pressed;
                case ControllerInputID.RStick:    return GpState.Buttons.RightStick    == ButtonState.Pressed;
                case ControllerInputID.LShoulder: return GpState.Buttons.LeftShoulder  == ButtonState.Pressed;
                case ControllerInputID.RShoulder: return GpState.Buttons.RightShoulder == ButtonState.Pressed;
                case ControllerInputID.DPadUp:    return GpState.DPad.Up               == ButtonState.Pressed;
                case ControllerInputID.DPadDown:  return GpState.DPad.Down             == ButtonState.Pressed;
                case ControllerInputID.DPadLeft:  return GpState.DPad.Left             == ButtonState.Pressed;
                case ControllerInputID.DPadRight: return GpState.DPad.Right            == ButtonState.Pressed;
                case ControllerInputID.Start:     return GpState.Buttons.Start         == ButtonState.Pressed;
                case ControllerInputID.Back:      return GpState.Buttons.Back          == ButtonState.Pressed;

                case ControllerInputID.LTrigger: return GpState.Triggers.Left  >= TriggerThreshold;
                case ControllerInputID.RTrigger: return GpState.Triggers.Right >= TriggerThreshold;

                //Using thumbsticks as buttons is not common, but it would be nice not to ignore them
                case ControllerInputID.LJoystick:
                    return GpState.ThumbSticks.Left.X >= Deadzone ||
                           GpState.ThumbSticks.Left.Y >= Deadzone;

                case ControllerInputID.RJoystick:
                    return GpState.ThumbSticks.Right.X >= Deadzone ||
                           GpState.ThumbSticks.Right.Y >= Deadzone;

                default:
                    return false;
            }
        }
    }
}
