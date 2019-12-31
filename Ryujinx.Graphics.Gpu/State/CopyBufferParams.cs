namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Buffer to buffer copy parameters.
    /// </summary>
    struct CopyBufferParams
    {
        public GpuVa SrcAddress;
        public GpuVa DstAddress;
        public int   SrcStride;
        public int   DstStride;
        public int   XCount;
        public int   YCount;
    }
}