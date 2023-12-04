using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct OutputSurfaceConfig
    {
#pragma warning disable CS0649 // Field is never assigned to
        private readonly long _word0;
        private readonly long _word1;
#pragma warning restore CS0649

        public PixelFormat OutPixelFormat => (PixelFormat)_word0.Extract(0, 7);
        public int OutChromaLocHoriz => (int)_word0.Extract(7, 2);
        public int OutChromaLocVert => (int)_word0.Extract(9, 2);
        public int OutBlkKind => (int)_word0.Extract(11, 4);
        public int OutBlkHeight => (int)_word0.Extract(15, 4);
        public int OutSurfaceWidth => (int)_word0.Extract(32, 14);
        public int OutSurfaceHeight => (int)_word0.Extract(46, 14);
        public int OutLumaWidth => (int)_word1.Extract(64, 14);
        public int OutLumaHeight => (int)_word1.Extract(78, 14);
        public int OutChromaWidth => (int)_word1.Extract(96, 14);
        public int OutChromaHeight => (int)_word1.Extract(110, 14);
    }
}
