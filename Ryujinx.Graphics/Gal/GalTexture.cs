namespace Ryujinx.Graphics.Gal
{
    public struct GalTexture
    {
        public int Width;
        public int Height;

        public GalTextureFormat Format;

        public GalTextureSource XSource;
        public GalTextureSource YSource;
        public GalTextureSource ZSource;
        public GalTextureSource WSource;

        public GalTexture(
            int              Width,
            int              Height,
            GalTextureFormat Format,
            GalTextureSource XSource,
            GalTextureSource YSource,
            GalTextureSource ZSource,
            GalTextureSource WSource)
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