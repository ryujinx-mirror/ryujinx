namespace Ryujinx.Graphics.Vic.Types
{
    struct OutputSurfaceConfig
    {
#pragma warning disable CS0649
        private long _word0;
        private long _word1;
#pragma warning restore CS0649

        public PixelFormat OutPixelFormat => (PixelFormat)_word0.Extract(0, 7);
        public int OutChromaLocHoriz => _word0.Extract(7, 2);
        public int OutChromaLocVert => _word0.Extract(9, 2);
        public int OutBlkKind => _word0.Extract(11, 4);
        public int OutBlkHeight => _word0.Extract(15, 4);
        public int OutSurfaceWidth => _word0.Extract(32, 14);
        public int OutSurfaceHeight => _word0.Extract(46, 14);
        public int OutLumaWidth => _word1.Extract(64, 14);
        public int OutLumaHeight => _word1.Extract(78, 14);
        public int OutChromaWidth => _word1.Extract(96, 14);
        public int OutChromaHeight => _word1.Extract(110, 14);
    }
}
