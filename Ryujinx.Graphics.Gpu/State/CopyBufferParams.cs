namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Buffer to buffer copy parameters.
    /// </summary>
    struct CopyBufferParams
    {
#pragma warning disable CS0649
        public GpuVa SrcAddress;
        public GpuVa DstAddress;
        public int   SrcStride;
        public int   DstStride;
        public int   XCount;
        public int   YCount;
#pragma warning restore CS0649
    }
}