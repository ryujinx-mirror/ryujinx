namespace Ryujinx.HLE.OsHle.Services.Aud
{
    struct UpdateDataHeader
    {
        public int Revision;
        public int BehaviorSize;
        public int MemoryPoolsSize;
        public int VoicesSize;
        public int VoiceResourceSize;
        public int EffectsSize;
        public int MixesSize;
        public int SinksSize;
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
