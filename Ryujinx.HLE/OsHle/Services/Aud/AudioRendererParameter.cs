using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Services.Aud
{
    [StructLayout(LayoutKind.Sequential)]
    struct AudioRendererParameter
    {
        public int SampleRate;
        public int SampleCount;
        public int Unknown8;
        public int UnknownC;
        public int VoiceCount;
        public int SinkCount;
        public int EffectCount;
        public int Unknown1C;
        public int Unknown20;
        public int SplitterCount;
        public int Unknown28;
        public int Unknown2C;
        public int Revision;
    }
}
