namespace Ryujinx.Graphics.Vic.Types
{
    struct OutputConfig
    {
#pragma warning disable CS0649
        private long _word0;
        private long _word1;
#pragma warning restore CS0649

        public int AlphaFillMode => _word0.Extract(0, 3);
        public int AlphaFillSlot => _word0.Extract(3, 3);
        public int BackgroundAlpha => _word0.Extract(6, 10);
        public int BackgroundR => _word0.Extract(16, 10);
        public int BackgroundG => _word0.Extract(26, 10);
        public int BackgroundB => _word0.Extract(36, 10);
        public int RegammaMode => _word0.Extract(46, 2);
        public bool OutputFlipX => _word0.Extract(48);
        public bool OutputFlipY => _word0.Extract(49);
        public bool OutputTranspose => _word0.Extract(50);
        public int TargetRectLeft => _word1.Extract(64, 14);
        public int TargetRectRight => _word1.Extract(80, 14);
        public int TargetRectTop => _word1.Extract(96, 14);
        public int TargetRectBottom => _word1.Extract(112, 14);
    }
}
