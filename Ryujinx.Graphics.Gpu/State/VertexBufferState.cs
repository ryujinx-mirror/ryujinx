namespace Ryujinx.Graphics.Gpu.State
{
    struct VertexBufferState
    {
        public uint  Control;
        public GpuVa Address;
        public int   Divisor;

        public int UnpackStride()
        {
            return (int)(Control & 0xfff);
        }

        public bool UnpackEnable()
        {
            return (Control & (1 << 12)) != 0;
        }
    }
}
