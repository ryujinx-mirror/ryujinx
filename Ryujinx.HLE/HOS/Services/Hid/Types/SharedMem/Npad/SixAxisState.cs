namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct SixAxisState
    {
        public ulong SampleTimestamp;
        ulong _unknown1;
        public ulong SampleTimestamp2;
        public HidVector Accelerometer;
        public HidVector Gyroscope;
        HidVector unknownSensor;
        public fixed float Orientation[9];
        ulong _unknown2;
    }
}