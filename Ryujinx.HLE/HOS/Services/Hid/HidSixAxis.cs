namespace Ryujinx.HLE.HOS.Services.Hid
{
    public struct HidSensorFusionParameters
    {
        public float RevisePower;
        public float ReviseRange;
    }

    public struct HidAccelerometerParameters
    {
        public float X;
        public float Y;
    }

    public enum HidGyroscopeZeroDriftMode
    {
        Loose,
        Standard,
        Tight
    }
}
