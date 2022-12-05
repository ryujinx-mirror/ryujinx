namespace Ryujinx.Graphics.GAL
{
    public readonly struct Rectangle<T> where T : unmanaged
    {
        public T X { get; }
        public T Y { get; }
        public T Width { get; }
        public T Height { get; }

        public Rectangle(T x, T y, T width, T height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
