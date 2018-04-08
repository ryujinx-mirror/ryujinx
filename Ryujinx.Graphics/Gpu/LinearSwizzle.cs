namespace Ryujinx.Graphics.Gpu
{
    class LinearSwizzle : ISwizzle
    {
        private int Bpp;
        private int Stride;

        public LinearSwizzle(int Width, int Bpp)
        {
            this.Bpp = Bpp;

            Stride = Width * Bpp;
        }

        public int GetSwizzleOffset(int X, int Y)
        {
            return X * Bpp + Y * Stride;
        }
    }
}