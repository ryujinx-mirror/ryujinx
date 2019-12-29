namespace Ryujinx.Graphics.GAL
{
    public struct VertexBufferDescriptor
    {
        public BufferRange Buffer { get; }

        public int Stride  { get; }
        public int Divisor { get; }

        public VertexBufferDescriptor(BufferRange buffer, int stride, int divisor)
        {
            Buffer  = buffer;
            Stride  = stride;
            Divisor = divisor;
        }
    }
}
