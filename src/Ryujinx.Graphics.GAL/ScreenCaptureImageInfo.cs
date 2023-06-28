namespace Ryujinx.Graphics.GAL
{
    public readonly struct ScreenCaptureImageInfo
    {
        public ScreenCaptureImageInfo(int width, int height, bool isBgra, byte[] data, bool flipX, bool flipY)
        {
            Width = width;
            Height = height;
            IsBgra = isBgra;
            Data = data;
            FlipX = flipX;
            FlipY = flipY;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Data { get; }
        public bool IsBgra { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
    }
}
