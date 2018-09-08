using Ryujinx.Graphics.Gal;

namespace Ryujinx.Graphics.Texture
{
    public struct TextureInfo
    {
        public long Position { get; private set; }

        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Pitch  { get; private set; }

        public int BlockHeight { get; private set; }
        public int TileWidth   { get; private set; }

        public TextureSwizzle Swizzle { get; private set; }

        public GalImageFormat Format { get; private set; }

        public TextureInfo(
            long Position,
            int  Width,
            int  Height)
        {
            this.Position = Position;
            this.Width    = Width;
            this.Height   = Height;

            Pitch = 0;

            BlockHeight = 16;

            TileWidth = 1;

            Swizzle = TextureSwizzle.BlockLinear;

            Format = GalImageFormat.A8B8G8R8 | GalImageFormat.Unorm;
        }

        public TextureInfo(
            long             Position,
            int              Width,
            int              Height,
            int              Pitch,
            int              BlockHeight,
            int              TileWidth,
            TextureSwizzle   Swizzle,
            GalImageFormat   Format)
        {
            this.Position     = Position;
            this.Width        = Width;
            this.Height       = Height;
            this.Pitch        = Pitch;
            this.BlockHeight  = BlockHeight;
            this.TileWidth    = TileWidth;
            this.Swizzle      = Swizzle;
            this.Format       = Format;
        }
    }
}