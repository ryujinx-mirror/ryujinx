namespace Ryujinx.Audio.Backends.SoundIo
{
    class SoundIoAudioBuffer
    {
        public readonly ulong DriverIdentifier;
        public readonly ulong SampleCount;
        public ulong SamplePlayed;

        public SoundIoAudioBuffer(ulong driverIdentifier, ulong sampleCount)
        {
            DriverIdentifier = driverIdentifier;
            SampleCount = sampleCount;
            SamplePlayed = 0;
        }
    }
}
