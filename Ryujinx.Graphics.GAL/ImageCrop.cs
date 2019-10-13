namespace Ryujinx.Graphics.GAL
{
    public struct ImageCrop
    {
        public int  Left   { get; }
        public int  Right  { get; }
        public int  Top    { get; }
        public int  Bottom { get; }
        public bool FlipX  { get; }
        public bool FlipY  { get; }

        public ImageCrop(
            int  left,
            int  right,
            int  top,
            int  bottom,
            bool flipX,
            bool flipY)
        {
            Left   = left;
            Right  = right;
            Top    = top;
            Bottom = bottom;
            FlipX  = flipX;
            FlipY  = flipY;
        }
    }
}