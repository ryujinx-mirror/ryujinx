using Ryujinx.Graphics.Gal;

namespace Ryujinx.HLE.Gpu.Texture
{
    struct TextureInfo
    {
        public long Position { get; private set; }

        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Pitch  { get; private set; }

        public int BlockHeight { get; private set; }

        public TextureSwizzle Swizzle { get; private set; }

        public GalTextureFormat Format { get; private set; }

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

            Swizzle = TextureSwizzle.BlockLinear;

            Format = GalTextureFormat.A8B8G8R8;
        }

        public TextureInfo(
            long             Position,
            int              Width,
            int              Height,
            int              Pitch,
            int              BlockHeight,
            TextureSwizzle   Swizzle,
            GalTextureFormat Format)
        {
            this.Position    = Position;
            this.Width       = Width;
            this.Height      = Height;
            this.Pitch       = Pitch;
            this.BlockHeight = BlockHeight;
            this.Swizzle     = Swizzle;
            this.Format      = Format;
        }
    }
}