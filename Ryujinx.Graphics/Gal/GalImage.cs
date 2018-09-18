using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Gal
{
    public struct GalImage
    {
        public int Width;
        public int Height;
        public int TileWidth;
        public int GobBlockHeight;
        public int Pitch;

        public GalImageFormat   Format;
        public GalMemoryLayout  Layout;
        public GalTextureSource XSource;
        public GalTextureSource YSource;
        public GalTextureSource ZSource;
        public GalTextureSource WSource;

        public GalImage(
            int              Width,
            int              Height,
            int              TileWidth,
            int              GobBlockHeight,
            GalMemoryLayout  Layout,
            GalImageFormat   Format,
            GalTextureSource XSource = GalTextureSource.Red,
            GalTextureSource YSource = GalTextureSource.Green,
            GalTextureSource ZSource = GalTextureSource.Blue,
            GalTextureSource WSource = GalTextureSource.Alpha)
        {
            this.Width          = Width;
            this.Height         = Height;
            this.TileWidth      = TileWidth;
            this.GobBlockHeight = GobBlockHeight;
            this.Layout         = Layout;
            this.Format         = Format;
            this.XSource        = XSource;
            this.YSource        = YSource;
            this.ZSource        = ZSource;
            this.WSource        = WSource;

            Pitch = ImageUtils.GetPitch(Format, Width);
        }

        public bool SizeMatches(GalImage Image)
        {
            if (ImageUtils.GetBytesPerPixel(Format) !=
                ImageUtils.GetBytesPerPixel(Image.Format))
            {
                return false;
            }

            if (ImageUtils.GetAlignedWidth(this) !=
                ImageUtils.GetAlignedWidth(Image))
            {
                return false;
            }

            return Height == Image.Height;
        }
    }
}