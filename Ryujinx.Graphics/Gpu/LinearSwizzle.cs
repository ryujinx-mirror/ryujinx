namespace Ryujinx.Graphics.Gpu
{
    class LinearSwizzle : ISwizzle
    {
        private int Pitch;
        private int Bpp;

        public LinearSwizzle(int Pitch, int Bpp)
        {
            this.Pitch = Pitch;
            this.Bpp   = Bpp;
        }

        public int GetSwizzleOffset(int X, int Y)
        {
            return X * Bpp + Y * Pitch;
        }
    }
}