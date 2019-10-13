namespace Ryujinx.Graphics.Gpu.State
{
    struct RtDepthStencilState
    {
        public GpuVa        Address;
        public RtFormat     Format;
        public MemoryLayout MemoryLayout;
        public int          LayerSize;
    }
}
