namespace Ryujinx.Graphics.Gpu.Memory
{
    struct VertexBuffer
    {
        public ulong Address;
        public ulong Size;
        public int   Stride;
        public int   Divisor;
    }
}