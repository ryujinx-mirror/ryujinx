namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    struct UpdateDataHeader
    {
#pragma warning disable CS0649
        public int Revision;
        public int BehaviorSize;
        public int MemoryPoolSize;
        public int VoiceSize;
        public int VoiceResourceSize;
        public int EffectSize;
        public int MixSize;
        public int SinkSize;
        public int PerformanceManagerSize;
        public int Unknown24;
        public int ElapsedFrameCountInfoSize;
        public int Unknown2C;
        public int Unknown30;
        public int Unknown34;
        public int Unknown38;
        public int TotalSize;
#pragma warning restore CS0649
    }
}