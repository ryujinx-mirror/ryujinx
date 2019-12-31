namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture to texture (with optional resizing) copy parameters.
    /// </summary>
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