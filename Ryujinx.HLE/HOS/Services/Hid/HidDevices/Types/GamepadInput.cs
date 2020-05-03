namespace Ryujinx.HLE.HOS.Services.Hid
{
    public struct GamepadInput
    {
        public PlayerIndex      PlayerId;
        public ControllerKeys   Buttons;
        public JoystickPosition LStick;
        public JoystickPosition RStick;
    }
}