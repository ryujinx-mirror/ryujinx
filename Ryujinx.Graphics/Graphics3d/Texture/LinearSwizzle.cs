using System;

namespace Ryujinx.Graphics.Texture
{
    class LinearSwizzle : ISwizzle
    {
        private int _pitch;
        private int _bpp;

        private int _sliceSize;

        public LinearSwizzle(int pitch, int bpp, int width, int height)
        {
            _pitch     = pitch;
            _bpp       = bpp;
            _sliceSize = width * height * bpp;
        }

        public void SetMipLevel(int level)
        {
            throw new NotImplementedException();
        }

        public int GetMipOffset(int level)
        {
            if (level == 1)
                return _sliceSize;
            throw new NotImplementedException();
        }

        public int GetImageSize(int mipsCount)
        {
            int size = GetMipOffset(mipsCount);

            size = (size + 0x1fff) & ~0x1fff;

            return size;
        }

        public int GetSwizzleOffset(int x, int y, int z)
        {
            return z * _sliceSize + x * _bpp + y * _pitch;
        }
    }
}