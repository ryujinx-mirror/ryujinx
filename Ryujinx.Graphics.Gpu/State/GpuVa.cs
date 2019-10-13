namespace Ryujinx.Graphics.Gpu.State
{
    struct GpuVa
    {
        public uint High;
        public uint Low;

        public ulong Pack()
        {
            return Low | ((ulong)High << 32);
        }

        public bool IsNullPtr()
        {
            return (Low | High) == 0;
        }
    }
}
