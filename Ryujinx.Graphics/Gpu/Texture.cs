using Ryujinx.Graphics.Gal;

namespace Ryujinx.Graphics.Gpu
{
    struct Texture
    {
        public long Position { get; private set; }

        public int Width  { get; private set; }
        public int Height { get; private set; }

        public int BlockHeight { get; private set; }

        public TextureSwizzle Swizzle { get; private set; }

        public GalTextureFormat Format { get; private set; }

        public Texture(
            long             Position,
            int              Width,
            int              Height,
            int              BlockHeight,
            TextureSwizzle   Swizzle,
            GalTextureFormat Format)
        {
            this.Position    = Position;
            this.Width       = Width;
            this.Height      = Height;
            this.BlockHeight = BlockHeight;
            this.Swizzle     = Swizzle;
            this.Format      = Format;
        }
    }
}