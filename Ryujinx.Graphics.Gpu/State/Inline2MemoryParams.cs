namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Inline-to-memory copy parameters.
    /// </summary>
    struct Inline2MemoryParams
    {
        public int          LineLengthIn;
        public int          LineCount;
        public GpuVa        DstAddress;
        public int          DstStride;
        public MemoryLayout DstMemoryLayout;
        public int          DstWidth;
        public int          DstHeight;
        public int          DstDepth;
        public int          DstZ;
        public int          DstX;
        public int          DstY;
    }
}