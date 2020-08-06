namespace Ryujinx.Graphics.Vic.Types
{
    struct ClearRectStruct
    {
#pragma warning disable CS0649
        private long _word0;
        private long _word1;
#pragma warning restore CS0649

        public int ClearRect0Left => _word0.Extract(0, 14);
        public int ClearRect0Right => _word0.Extract(16, 14);
        public int ClearRect0Top => _word0.Extract(32, 14);
        public int ClearRect0Bottom => _word0.Extract(48, 14);
        public int ClearRect1Left => _word1.Extract(64, 14);
        public int ClearRect1Right => _word1.Extract(80, 14);
        public int ClearRect1Top => _word1.Extract(96, 14);
        public int ClearRect1Bottom => _word1.Extract(112, 14);
    }
}
