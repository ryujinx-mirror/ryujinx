namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    struct UpdateDataHeader
    {
        public int Revision;
        public int BehaviorSize;
        public int MemoryPoolSize;
        public int VoiceSize;
        public int VoiceResourceSize;
        public int EffectSize;
        public int MixeSize;
        public int SinkSize;
        public int PerformanceManagerSize;
        public int Unknown24;
        public int Unknown28;
        public int Unknown2C;
        public int Unknown30;
        public int Unknown34;
        public int Unknown38;
        public int TotalSize;
    }
}