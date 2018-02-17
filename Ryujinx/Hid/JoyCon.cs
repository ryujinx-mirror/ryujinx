namespace Ryujinx
{
    public enum JoyConColor //Thanks to CTCaer
    {
        Body_Grey = 0x828282,
        Body_Neon_Blue = 0x0AB9E6,
        Body_Neon_Red = 0xFF3C28,
        Body_Neon_Yellow = 0xE6FF00,
        Body_Neon_Pink = 0xFF3278,
        Body_Neon_Green = 0x1EDC00,
        Body_Red = 0xE10F00,

        Buttons_Grey = 0x0F0F0F,
        Buttons_Neon_Blue = 0x001E1E,
        Buttons_Neon_Red = 0x1E0A0A,
        Buttons_Neon_Yellow = 0x142800,
        Buttons_Neon_Pink = 0x28001E,
        Buttons_Neon_Green = 0x002800,
        Buttons_Red = 0x280A0A
    }

    public struct JoyConLeft
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
        public int ButtonSL;
        public int ButtonSR;
    }

    public struct JoyConRight
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
        public int ButtonSL;
        public int ButtonSR;
    }

    public struct JoyCon
    {
        public JoyConLeft Left;
        public JoyConRight Right;
    }
}
