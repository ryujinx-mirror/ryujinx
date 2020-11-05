namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Render target depth-stencil buffer state.
    /// </summary>
    struct RtDepthStencilState
    {
#pragma warning disable CS0649
        public GpuVa        Address;
        public ZetaFormat   Format;
        public MemoryLayout MemoryLayout;
        public int          LayerSize;
#pragma warning restore CS0649
    }
}
