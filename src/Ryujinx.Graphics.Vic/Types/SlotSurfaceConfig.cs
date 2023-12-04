using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct SlotSurfaceConfig
    {
        private readonly long _word0;
        private readonly long _word1;

        public PixelFormat SlotPixelFormat => (PixelFormat)_word0.Extract(0, 7);
        public int SlotChromaLocHoriz => (int)_word0.Extract(7, 2);
        public int SlotChromaLocVert => (int)_word0.Extract(9, 2);
        public int SlotBlkKind => (int)_word0.Extract(11, 4);
        public int SlotBlkHeight => (int)_word0.Extract(15, 4);
        public int SlotCacheWidth => (int)_word0.Extract(19, 3);
        public int SlotSurfaceWidth => (int)_word0.Extract(32, 14);
        public int SlotSurfaceHeight => (int)_word0.Extract(46, 14);
        public int SlotLumaWidth => (int)_word1.Extract(64, 14);
        public int SlotLumaHeight => (int)_word1.Extract(78, 14);
        public int SlotChromaWidth => (int)_word1.Extract(96, 14);
        public int SlotChromaHeight => (int)_word1.Extract(110, 14);
    }
}
