using OpenTK.Input;
using Ryujinx.HLE.Input;

namespace Ryujinx.UI.Input
{
    public struct NpadKeyboardLeft
    {
        public int StickUp;
        public int StickDown;
        public int StickLeft;
        public int StickRight;
        public int StickButton;
        public int DPadUp;
        public int DPadDown;
        public int DPadLeft;
        public int DPadRight;
        public int ButtonMinus;
        public int ButtonL;
        public int ButtonZl;
    }

    public struct NpadKeyboardRight
    {
        public int StickUp;
        public int StickDown;
        public int StickLeft;
        public int StickRight;
        public int StickButton;
        public int ButtonA;
        public int ButtonB;
        public int ButtonX;
        public int ButtonY;
        public int ButtonPlus;
        public int ButtonR;
        public int ButtonZr;
    }

    public class NpadKeyboard
    {
        public NpadKeyboardLeft  Left;
        public NpadKeyboardRight Right;

        public NpadKeyboard(
            NpadKeyboardLeft  left,
            NpadKeyboardRight right)
        {
            Left  = left;
            Right = right;
        }

        public HidControllerButtons GetButtons(KeyboardState keyboard)
        {
            HidControllerButtons buttons = 0;

            if (keyboard[(Key)Left.StickButton]) buttons |= HidControllerButtons.StickLeft;
            if (keyboard[(Key)Left.DPadUp])      buttons |= HidControllerButtons.DpadUp;
            if (keyboard[(Key)Left.DPadDown])    buttons |= HidControllerButtons.DpadDown;
            if (keyboard[(Key)Left.DPadLeft])    buttons |= HidControllerButtons.DpadLeft;
            if (keyboard[(Key)Left.DPadRight])   buttons |= HidControllerButtons.DPadRight;
            if (keyboard[(Key)Left.ButtonMinus]) buttons |= HidControllerButtons.Minus;
            if (keyboard[(Key)Left.ButtonL])     buttons |= HidControllerButtons.L;
            if (keyboard[(Key)Left.ButtonZl])    buttons |= HidControllerButtons.Zl;
            
            if (keyboard[(Key)Right.StickButton]) buttons |= HidControllerButtons.StickRight;
            if (keyboard[(Key)Right.ButtonA])     buttons |= HidControllerButtons.A;
            if (keyboard[(Key)Right.ButtonB])     buttons |= HidControllerButtons.B;
            if (keyboard[(Key)Right.ButtonX])     buttons |= HidControllerButtons.X;
            if (keyboard[(Key)Right.ButtonY])     buttons |= HidControllerButtons.Y;
            if (keyboard[(Key)Right.ButtonPlus])  buttons |= HidControllerButtons.Plus;
            if (keyboard[(Key)Right.ButtonR])     buttons |= HidControllerButtons.R;
            if (keyboard[(Key)Right.ButtonZr])    buttons |= HidControllerButtons.Zr;

            return buttons;
        }

        public (short, short) GetLeftStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;
            
            if (keyboard[(Key)Left.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)Left.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)Left.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)Left.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }

        public (short, short) GetRightStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;

            if (keyboard[(Key)Right.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)Right.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)Right.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)Right.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }
    }
}
