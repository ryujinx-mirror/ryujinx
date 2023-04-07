namespace Ryujinx.Common.Configuration.Hid.Keyboard
{
    public class JoyconConfigKeyboardStick<Key> where Key: unmanaged
    {
        public Key StickUp { get; set; }
        public Key StickDown { get; set; }
        public Key StickLeft { get; set; }
        public Key StickRight { get; set; }
        public Key StickButton { get; set; }
    }
}
