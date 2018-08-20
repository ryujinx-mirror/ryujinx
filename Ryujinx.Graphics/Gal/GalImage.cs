namespace Ryujinx.Graphics.Gal
{
    public struct GalImage
    {
        public int Width;
        public int Height;

        public GalImageFormat Format;

        public GalTextureSource XSource;
        public GalTextureSource YSource;
        public GalTextureSource ZSource;
        public GalTextureSource WSource;

        public GalImage(
            int              Width,
            int              Height,
            GalImageFormat   Format,
            GalTextureSource XSource = GalTextureSource.Red,
            GalTextureSource YSource = GalTextureSource.Green,
            GalTextureSource ZSource = GalTextureSource.Blue,
            GalTextureSource WSource = GalTextureSource.Alpha)
        {
            this.Width   = Width;
            this.Height  = Height;
            this.Format  = Format;
            this.XSource = XSource;
            this.YSource = YSource;
            this.ZSource = ZSource;
            this.WSource = WSource;
        }
    }
}