namespace Ryujinx.Graphics.Vic.Types
{
    struct PipeConfig
    {
        private long _word0;
        private long _word1;

        public int DownsampleHoriz => _word0.Extract(0, 11);
        public int DownsampleVert => _word0.Extract(16, 11);
    }
}
