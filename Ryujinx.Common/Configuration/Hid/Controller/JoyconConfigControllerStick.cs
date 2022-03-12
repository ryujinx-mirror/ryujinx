namespace Ryujinx.Common.Configuration.Hid.Controller
{
    public class JoyconConfigControllerStick<Button, Stick> where Button: unmanaged where Stick: unmanaged
    {
        public Stick Joystick { get; set; }
        public bool InvertStickX { get; set; }
        public bool InvertStickY { get; set; }
        public bool Rotate90CW { get; set; }
        public Button StickButton { get; set; }
    }
}
