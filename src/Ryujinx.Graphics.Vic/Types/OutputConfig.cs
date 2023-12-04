using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct OutputConfig
    {
#pragma warning disable CS0649 // Field is never assigned to
        private readonly long _word0;
        private readonly long _word1;
#pragma warning restore CS0649

        public int AlphaFillMode => (int)_word0.Extract(0, 3);
        public int AlphaFillSlot => (int)_word0.Extract(3, 3);
        public int BackgroundAlpha => (int)_word0.Extract(6, 10);
        public int BackgroundR => (int)_word0.Extract(16, 10);
        public int BackgroundG => (int)_word0.Extract(26, 10);
        public int BackgroundB => (int)_word0.Extract(36, 10);
        public int RegammaMode => (int)_word0.Extract(46, 2);
        public bool OutputFlipX => _word0.Extract(48);
        public bool OutputFlipY => _word0.Extract(49);
        public bool OutputTranspose => _word0.Extract(50);
        public int TargetRectLeft => (int)_word1.Extract(64, 14);
        public int TargetRectRight => (int)_word1.Extract(80, 14);
        public int TargetRectTop => (int)_word1.Extract(96, 14);
        public int TargetRectBottom => (int)_word1.Extract(112, 14);
    }
}
