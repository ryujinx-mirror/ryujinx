namespace Ryujinx.Graphics.GAL
{
    public struct BufferRange
    {
        private static BufferRange _empty = new BufferRange(null, 0, 0);

        public BufferRange Empty => _empty;

        public IBuffer Buffer { get; }

        public int Offset { get; }
        public int Size   { get; }

        public BufferRange(IBuffer buffer, int offset, int size)
        {
            Buffer = buffer;
            Offset = offset;
            Size   = size;
        }
    }
}