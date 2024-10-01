using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x4)]
    readonly struct FinalOutputRecorderParameter
    {
        public readonly uint SampleRate;
        public readonly uint Padding;

        public FinalOutputRecorderParameter(uint sampleRate)
        {
            SampleRate = sampleRate;
            Padding = 0;
        }
    }
}
