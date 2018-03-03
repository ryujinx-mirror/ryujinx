//TODO: This is only used by Config, it doesn't belong to Core.
namespace Ryujinx.Core.Input
{
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
