using OpenTK.Input;
using Ryujinx.HLE.Input;

namespace Ryujinx.UI.Input
{
    public struct NpadKeyboardLeft
    {
        public Key StickUp;
        public Key StickDown;
        public Key StickLeft;
        public Key StickRight;
        public Key StickButton;
        public Key DPadUp;
        public Key DPadDown;
        public Key DPadLeft;
        public Key DPadRight;
        public Key ButtonMinus;
        public Key ButtonL;
        public Key ButtonZl;
    }

    public struct NpadKeyboardRight
    {
        public Key StickUp;
        public Key StickDown;
        public Key StickLeft;
        public Key StickRight;
        public Key StickButton;
        public Key ButtonA;
        public Key ButtonB;
        public Key ButtonX;
        public Key ButtonY;
        public Key ButtonPlus;
        public Key ButtonR;
        public Key ButtonZr;
    }

    public class NpadKeyboard
    {
        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardLeft LeftJoycon { get; private set; }

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardRight RightJoycon { get; private set; }

        public HidControllerButtons GetButtons(KeyboardState keyboard)
        {
            HidControllerButtons buttons = 0;

            if (keyboard[(Key)LeftJoycon.StickButton]) buttons |= HidControllerButtons.StickLeft;
            if (keyboard[(Key)LeftJoycon.DPadUp])      buttons |= HidControllerButtons.DpadUp;
            if (keyboard[(Key)LeftJoycon.DPadDown])    buttons |= HidControllerButtons.DpadDown;
            if (keyboard[(Key)LeftJoycon.DPadLeft])    buttons |= HidControllerButtons.DpadLeft;
            if (keyboard[(Key)LeftJoycon.DPadRight])   buttons |= HidControllerButtons.DPadRight;
            if (keyboard[(Key)LeftJoycon.ButtonMinus]) buttons |= HidControllerButtons.Minus;
            if (keyboard[(Key)LeftJoycon.ButtonL])     buttons |= HidControllerButtons.L;
            if (keyboard[(Key)LeftJoycon.ButtonZl])    buttons |= HidControllerButtons.Zl;
            
            if (keyboard[(Key)RightJoycon.StickButton]) buttons |= HidControllerButtons.StickRight;
            if (keyboard[(Key)RightJoycon.ButtonA])     buttons |= HidControllerButtons.A;
            if (keyboard[(Key)RightJoycon.ButtonB])     buttons |= HidControllerButtons.B;
            if (keyboard[(Key)RightJoycon.ButtonX])     buttons |= HidControllerButtons.X;
            if (keyboard[(Key)RightJoycon.ButtonY])     buttons |= HidControllerButtons.Y;
            if (keyboard[(Key)RightJoycon.ButtonPlus])  buttons |= HidControllerButtons.Plus;
            if (keyboard[(Key)RightJoycon.ButtonR])     buttons |= HidControllerButtons.R;
            if (keyboard[(Key)RightJoycon.ButtonZr])    buttons |= HidControllerButtons.Zr;

            return buttons;
        }

        public (short, short) GetLeftStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;
            
            if (keyboard[(Key)LeftJoycon.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)LeftJoycon.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }

        public (short, short) GetRightStick(KeyboardState keyboard)
        {
            short dx = 0;
            short dy = 0;

            if (keyboard[(Key)RightJoycon.StickUp])    dy =  short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickDown])  dy = -short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickLeft])  dx = -short.MaxValue;
            if (keyboard[(Key)RightJoycon.StickRight]) dx =  short.MaxValue;

            return (dx, dy);
        }
    }
}
