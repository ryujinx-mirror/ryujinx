namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture copy region.
    /// </summary>
    struct CopyRegion
    {
#pragma warning disable CS0649
        public int  DstX;
        public int  DstY;
        public int  DstWidth;
        public int  DstHeight;
        public long SrcWidthRF;
        public long SrcHeightRF;
        public long SrcXF;
        public long SrcYF;
#pragma warning restore CS0649
    }
}