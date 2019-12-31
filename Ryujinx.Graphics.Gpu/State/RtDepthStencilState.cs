namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Render target depth-stencil buffer state.
    /// </summary>
    struct RtDepthStencilState
    {
        public GpuVa        Address;
        public RtFormat     Format;
        public MemoryLayout MemoryLayout;
        public int          LayerSize;
    }
}
