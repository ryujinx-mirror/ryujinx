namespace Ryujinx.Graphics.GAL
{
    public struct ImageCrop
    {
        public int   Left         { get; }
        public int   Right        { get; }
        public int   Top          { get; }
        public int   Bottom       { get; }
        public bool  FlipX        { get; }
        public bool  FlipY        { get; }
        public bool  IsStretched  { get; }
        public float AspectRatioX { get; }
        public float AspectRatioY { get; }

        public ImageCrop(
            int   left,
            int   right,
            int   top,
            int   bottom,
            bool  flipX,
            bool  flipY,
            bool  isStretched,
            float aspectRatioX,
            float aspectRatioY
            )
        {
            Left         = left;
            Right        = right;
            Top          = top;
            Bottom       = bottom;
            FlipX        = flipX;
            FlipY        = flipY;
            IsStretched  = isStretched;
            AspectRatioX = aspectRatioX;
            AspectRatioY = aspectRatioY;
        }
    }
}