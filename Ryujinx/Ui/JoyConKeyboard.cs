using OpenTK.Input;
using Ryujinx.HLE.Input;

namespace Ryujinx.UI.Input
{
    public struct JoyConKeyboardLeft
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

    public struct JoyConKeyboardRight
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

    public class JoyConKeyboard
    {
        public JoyConKeyboardLeft  Left;
        public JoyConKeyboardRight Right;

        public JoyConKeyboard(
            JoyConKeyboardLeft  left,
            JoyConKeyboardRight right)
        {
            Left  = left;
            Right = right;
        }

        public HidControllerButtons GetButtons(KeyboardState keyboard)
        {
            HidControllerButtons buttons = 0;

            if (keyboard[(Key)Left.StickButton]) buttons |= HidControllerButtons.KEY_LSTICK;
            if (keyboard[(Key)Left.DPadUp])      buttons |= HidControllerButtons.KEY_DUP;
            if (keyboard[(Key)Left.DPadDown])    buttons |= HidControllerButtons.KEY_DDOWN;
            if (keyboard[(Key)Left.DPadLeft])    buttons |= HidControllerButtons.KEY_DLEFT;
            if (keyboard[(Key)Left.DPadRight])   buttons |= HidControllerButtons.KEY_DRIGHT;
            if (keyboard[(Key)Left.ButtonMinus]) buttons |= HidControllerButtons.KEY_MINUS;
            if (keyboard[(Key)Left.ButtonL])     buttons |= HidControllerButtons.KEY_L;
            if (keyboard[(Key)Left.ButtonZl])    buttons |= HidControllerButtons.KEY_ZL;
            
            if (keyboard[(Key)Right.StickButton]) buttons |= HidControllerButtons.KEY_RSTICK;
            if (keyboard[(Key)Right.ButtonA])     buttons |= HidControllerButtons.KEY_A;
            if (keyboard[(Key)Right.ButtonB])     buttons |= HidControllerButtons.KEY_B;
            if (keyboard[(Key)Right.ButtonX])     buttons |= HidControllerButtons.KEY_X;
            if (keyboard[(Key)Right.ButtonY])     buttons |= HidControllerButtons.KEY_Y;
            if (keyboard[(Key)Right.ButtonPlus])  buttons |= HidControllerButtons.KEY_PLUS;
            if (keyboard[(Key)Right.ButtonR])     buttons |= HidControllerButtons.KEY_R;
            if (keyboard[(Key)Right.ButtonZr])    buttons |= HidControllerButtons.KEY_ZR;

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
