namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Render target color buffer state.
    /// </summary>
    struct RtColorState
    {
#pragma warning disable CS0649
        public GpuVa        Address;
        public int          WidthOrStride;
        public int          Height;
        public ColorFormat  Format;
        public MemoryLayout MemoryLayout;
        public int          Depth;
        public int          LayerSize;
        public int          BaseLayer;
        public int          Unknown0x24;
        public int          Padding0;
        public int          Padding1;
        public int          Padding2;
        public int          Padding3;
        public int          Padding4;
        public int          Padding5;
#pragma warning restore CS0649
    }
}
