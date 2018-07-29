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
        public int ButtonZL;
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
        public int ButtonZR;
    }

    public class JoyConKeyboard
    {
        public JoyConKeyboardLeft  Left;
        public JoyConKeyboardRight Right;

        public JoyConKeyboard(
            JoyConKeyboardLeft  Left,
            JoyConKeyboardRight Right)
        {
            this.Left  = Left;
            this.Right = Right;
        }

        public HidControllerButtons GetButtons(KeyboardState Keyboard)
        {
            HidControllerButtons Buttons = 0;

            if (Keyboard[(Key)Left.StickButton]) Buttons |= HidControllerButtons.KEY_LSTICK;
            if (Keyboard[(Key)Left.DPadUp])      Buttons |= HidControllerButtons.KEY_DUP;
            if (Keyboard[(Key)Left.DPadDown])    Buttons |= HidControllerButtons.KEY_DDOWN;
            if (Keyboard[(Key)Left.DPadLeft])    Buttons |= HidControllerButtons.KEY_DLEFT;
            if (Keyboard[(Key)Left.DPadRight])   Buttons |= HidControllerButtons.KEY_DRIGHT;
            if (Keyboard[(Key)Left.ButtonMinus]) Buttons |= HidControllerButtons.KEY_MINUS;
            if (Keyboard[(Key)Left.ButtonL])     Buttons |= HidControllerButtons.KEY_L;
            if (Keyboard[(Key)Left.ButtonZL])    Buttons |= HidControllerButtons.KEY_ZL;
            
            if (Keyboard[(Key)Right.StickButton]) Buttons |= HidControllerButtons.KEY_RSTICK;
            if (Keyboard[(Key)Right.ButtonA])     Buttons |= HidControllerButtons.KEY_A;
            if (Keyboard[(Key)Right.ButtonB])     Buttons |= HidControllerButtons.KEY_B;
            if (Keyboard[(Key)Right.ButtonX])     Buttons |= HidControllerButtons.KEY_X;
            if (Keyboard[(Key)Right.ButtonY])     Buttons |= HidControllerButtons.KEY_Y;
            if (Keyboard[(Key)Right.ButtonPlus])  Buttons |= HidControllerButtons.KEY_PLUS;
            if (Keyboard[(Key)Right.ButtonR])     Buttons |= HidControllerButtons.KEY_R;
            if (Keyboard[(Key)Right.ButtonZR])    Buttons |= HidControllerButtons.KEY_ZR;

            return Buttons;
        }

        public (short, short) GetLeftStick(KeyboardState Keyboard)
        {
            short DX = 0;
            short DY = 0;
            
            if (Keyboard[(Key)Left.StickUp])    DY =  short.MaxValue;
            if (Keyboard[(Key)Left.StickDown])  DY = -short.MaxValue;
            if (Keyboard[(Key)Left.StickLeft])  DX = -short.MaxValue;
            if (Keyboard[(Key)Left.StickRight]) DX =  short.MaxValue;

            return (DX, DY);
        }

        public (short, short) GetRightStick(KeyboardState Keyboard)
        {
            short DX = 0;
            short DY = 0;

            if (Keyboard[(Key)Right.StickUp])    DY =  short.MaxValue;
            if (Keyboard[(Key)Right.StickDown])  DY = -short.MaxValue;
            if (Keyboard[(Key)Right.StickLeft])  DX = -short.MaxValue;
            if (Keyboard[(Key)Right.StickRight]) DX =  short.MaxValue;

            return (DX, DY);
        }
    }
}
