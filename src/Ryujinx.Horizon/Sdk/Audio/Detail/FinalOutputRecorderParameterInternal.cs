using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 0x4)]
    readonly struct FinalOutputRecorderParameterInternal
    {
        public readonly uint SampleRate;
        public readonly uint ChannelCount;
        public readonly uint UseLargeFrameSize;
        public readonly uint Padding;

        public FinalOutputRecorderParameterInternal(uint sampleRate, uint channelCount, uint useLargeFrameSize)
        {
            SampleRate = sampleRate;
            ChannelCount = channelCount;
            UseLargeFrameSize = useLargeFrameSize;
            Padding = 0;
        }
    }
}
