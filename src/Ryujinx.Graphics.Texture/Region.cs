namespace Ryujinx.Graphics.Texture
{
    public readonly struct Region
    {
        public int Offset { get; }
        public int Size { get; }

        public Region(int offset, int size)
        {
            Offset = offset;
            Size = size;
        }
    }
}
