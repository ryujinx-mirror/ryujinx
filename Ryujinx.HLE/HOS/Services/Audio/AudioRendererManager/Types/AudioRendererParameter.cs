using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    [StructLayout(LayoutKind.Sequential)]
    struct AudioRendererParameter
    {
        public int SampleRate;
        public int SampleCount;
        public int Unknown8;
        public int MixCount;
        public int VoiceCount;
        public int SinkCount;
        public int EffectCount;
        public int PerformanceManagerCount;
        public int VoiceDropEnable;
        public int SplitterCount;
        public int SplitterDestinationDataCount;
        public int Unknown2C;
        public int Revision;
    }
}