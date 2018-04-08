namespace Ryujinx.Graphics.Gal
{
    public struct GalTexture
    {
        public byte[] Data;

        public int Width;
        public int Height;

        public GalTextureFormat Format;

        public GalTexture(byte[] Data, int Width, int Height, GalTextureFormat Format)
        {
            this.Data   = Data;
            this.Width  = Width;
            this.Height = Height;
            this.Format = Format;
        }
    }
}