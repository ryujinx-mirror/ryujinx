namespace Ryujinx.HLE.HOS.Services.Hid
{
    public enum HidVibrationDeviceType
    {
        None,
        LinearResonantActuator
    }

    public enum HidVibrationDevicePosition
    {
        None,
        Left,
        Right
    }

    public struct HidVibrationDeviceValue
    {
        public HidVibrationDeviceType     DeviceType;
        public HidVibrationDevicePosition Position;
    }

    public struct HidVibrationValue
    {
        public float AmplitudeLow;
        public float FrequencyLow;
        public float AmplitudeHigh;
        public float FrequencyHigh;
    }
}
