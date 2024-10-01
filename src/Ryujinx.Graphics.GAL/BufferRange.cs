namespace Ryujinx.Graphics.GAL
{
    public readonly struct BufferRange
    {
        private static readonly BufferRange _empty = new(BufferHandle.Null, 0, 0);

        public static BufferRange Empty => _empty;

        public BufferHandle Handle { get; }

        public int Offset { get; }
        public int Size { get; }
        public bool Write { get; }

        public BufferRange(BufferHandle handle, int offset, int size, bool write = false)
        {
            Handle = handle;
            Offset = offset;
            Size = size;
            Write = write;
        }
    }
}
