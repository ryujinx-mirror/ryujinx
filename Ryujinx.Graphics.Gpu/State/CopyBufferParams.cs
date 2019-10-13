namespace Ryujinx.Graphics.Gpu.State
{
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