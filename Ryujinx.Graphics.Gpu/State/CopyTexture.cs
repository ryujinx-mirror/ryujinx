namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture to texture (with optional resizing) copy parameters.
    /// </summary>
    struct CopyTexture
    {
#pragma warning disable CS0649
        public ColorFormat  Format;
        public Boolean32    LinearLayout;
        public MemoryLayout MemoryLayout;
        public int          Depth;
        public int          Layer;
        public int          Stride;
        public int          Width;
        public int          Height;
        public GpuVa        Address;
#pragma warning restore CS0649
    }
}