using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioRenderer
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 4)]
    struct VoiceOut
    {
        public long PlayedSamplesCount;
        public int  PlayedWaveBuffersCount;
        public int  VoiceDropsCount; //?
    }
}