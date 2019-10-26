namespace Ryujinx.Graphics.Gpu.State
{
    struct CopyTexture
    {
        public RtFormat     Format;
        public Boolean32    LinearLayout;
        public MemoryLayout MemoryLayout;
        public int          Depth;
        public int          Layer;
        public int          Stride;
        public int          Width;
        public int          Height;
        public GpuVa        Address;
    }
}