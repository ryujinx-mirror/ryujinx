namespace Ryujinx.Graphics.Vic.Types
{
    struct SlotSurfaceConfig
    {
        private long _word0;
        private long _word1;

        public PixelFormat SlotPixelFormat => (PixelFormat)_word0.Extract(0, 7);
        public int SlotChromaLocHoriz => _word0.Extract(7, 2);
        public int SlotChromaLocVert => _word0.Extract(9, 2);
        public int SlotBlkKind => _word0.Extract(11, 4);
        public int SlotBlkHeight => _word0.Extract(15, 4);
        public int SlotCacheWidth => _word0.Extract(19, 3);
        public int SlotSurfaceWidth => _word0.Extract(32, 14);
        public int SlotSurfaceHeight => _word0.Extract(46, 14);
        public int SlotLumaWidth => _word1.Extract(64, 14);
        public int SlotLumaHeight => _word1.Extract(78, 14);
        public int SlotChromaWidth => _word1.Extract(96, 14);
        public int SlotChromaHeight => _word1.Extract(110, 14);
    }
}
