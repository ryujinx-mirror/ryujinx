namespace Ryujinx.Graphics.Gpu.State
{
    struct RtColorState
    {
        public GpuVa        Address;
        public int          WidthOrStride;
        public int          Height;
        public RtFormat     Format;
        public MemoryLayout MemoryLayout;
        public int          Depth;
        public int          LayerSize;
    }
}
