using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.OpenGL
{
    struct VertexBuffer
    {
        public BufferRange Range { get; }

        public int Divisor { get; }
        public int Stride  { get; }

        public VertexBuffer(BufferRange range, int divisor, int stride)
        {
            Range   = range;
            Divisor = divisor;
            Stride  = stride;
        }
    }
}
