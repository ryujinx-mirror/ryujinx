namespace Ryujinx.Audio.Backends.SDL2
{
    class SDL2AudioBuffer
    {
        public readonly ulong DriverIdentifier;
        public readonly ulong SampleCount;
        public ulong SamplePlayed;

        public SDL2AudioBuffer(ulong driverIdentifier, ulong sampleCount)
        {
            DriverIdentifier = driverIdentifier;
            SampleCount = sampleCount;
            SamplePlayed = 0;
        }
    }
}
