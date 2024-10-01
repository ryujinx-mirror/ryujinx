namespace Ryujinx.Common.Configuration.Hid.Keyboard
{
    public class JoyconConfigKeyboardStick<TKey> where TKey : unmanaged
    {
        public TKey StickUp { get; set; }
        public TKey StickDown { get; set; }
        public TKey StickLeft { get; set; }
        public TKey StickRight { get; set; }
        public TKey StickButton { get; set; }
    }
}
