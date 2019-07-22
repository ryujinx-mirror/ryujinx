using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerState
    {
        public long                      SamplesTimestamp;
        public long                      SamplesTimestamp2;
        public ControllerButtons         ButtonState;
        public JoystickPosition          LeftStick;
        public JoystickPosition          RightStick;
        public ControllerConnectionState ConnectionState;
    }
}
