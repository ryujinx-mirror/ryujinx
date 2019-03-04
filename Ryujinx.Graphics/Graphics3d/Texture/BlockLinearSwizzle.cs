using Ryujinx.Common;
using System;

namespace Ryujinx.Graphics.Texture
{
    class BlockLinearSwizzle : ISwizzle
    {
        private const int GobWidth  = 64;
        private const int GobHeight = 8;

        private const int GobSize = GobWidth * GobHeight;

        private int _texWidth;
        private int _texHeight;
        private int _texDepth;
        private int _texGobBlockHeight;
        private int _texGobBlockDepth;
        private int _texBpp;

        private int _bhMask;
        private int _bdMask;

        private int _bhShift;
        private int _bdShift;
        private int _bppShift;

        private int _xShift;

        private int _robSize;
        private int _sliceSize;

        private int _baseOffset;

        public BlockLinearSwizzle(
            int width,
            int height,
            int depth,
            int gobBlockHeight,
            int gobBlockDepth,
            int bpp)
        {
            _texWidth          = width;
            _texHeight         = height;
            _texDepth          = depth;
            _texGobBlockHeight = gobBlockHeight;
            _texGobBlockDepth  = gobBlockDepth;
            _texBpp            = bpp;

            _bppShift = BitUtils.CountTrailingZeros32(bpp);

            SetMipLevel(0);
        }

        public void SetMipLevel(int level)
        {
            _baseOffset = GetMipOffset(level);

            int width  = Math.Max(1, _texWidth  >> level);
            int height = Math.Max(1, _texHeight >> level);
            int depth  = Math.Max(1, _texDepth  >> level);

            GobBlockSizes gbSizes = AdjustGobBlockSizes(height, depth);

            _bhMask = gbSizes.Height - 1;
            _bdMask = gbSizes.Depth  - 1;

            _bhShift = BitUtils.CountTrailingZeros32(gbSizes.Height);
            _bdShift = BitUtils.CountTrailingZeros32(gbSizes.Depth);

            _xShift = BitUtils.CountTrailingZeros32(GobSize * gbSizes.Height * gbSizes.Depth);

            RobAndSliceSizes gsSizes = GetRobAndSliceSizes(width, height, gbSizes);

            _robSize   = gsSizes.RobSize;
            _sliceSize = gsSizes.SliceSize;
        }

        public int GetImageSize(int mipsCount)
        {
            int size = GetMipOffset(mipsCount);

            size = (size + 0x1fff) & ~0x1fff;

            return size;
        }

        public int GetMipOffset(int level)
        {
            int totalSize = 0;

            for (int index = 0; index < level; index++)
            {
                int width  = Math.Max(1, _texWidth  >> index);
                int height = Math.Max(1, _texHeight >> index);
                int depth  = Math.Max(1, _texDepth  >> index);

                GobBlockSizes gbSizes = AdjustGobBlockSizes(height, depth);

                RobAndSliceSizes rsSizes = GetRobAndSliceSizes(width, height, gbSizes);

                totalSize += BitUtils.DivRoundUp(depth, gbSizes.Depth) * rsSizes.SliceSize;
            }

            return totalSize;
        }

        private struct GobBlockSizes
        {
            public int Height;
            public int Depth;

            public GobBlockSizes(int gobBlockHeight, int gobBlockDepth)
            {
                Height = gobBlockHeight;
                Depth  = gobBlockDepth;
            }
        }

        private GobBlockSizes AdjustGobBlockSizes(int height, int depth)
        {
            int gobBlockHeight = _texGobBlockHeight;
            int gobBlockDepth  = _texGobBlockDepth;

            int pow2Height = BitUtils.Pow2RoundUp(height);
            int pow2Depth  = BitUtils.Pow2RoundUp(depth);

            while (gobBlockHeight * GobHeight > pow2Height && gobBlockHeight > 1)
            {
                gobBlockHeight >>= 1;
            }

            while (gobBlockDepth > pow2Depth && gobBlockDepth > 1)
            {
                gobBlockDepth >>= 1;
            }

            return new GobBlockSizes(gobBlockHeight, gobBlockDepth);
        }

        private struct RobAndSliceSizes
        {
            public int RobSize;
            public int SliceSize;

            public RobAndSliceSizes(int robSize, int sliceSize)
            {
                RobSize   = robSize;
                SliceSize = sliceSize;
            }
        }

        private RobAndSliceSizes GetRobAndSliceSizes(int width, int height, GobBlockSizes gbSizes)
        {
            int widthInGobs = BitUtils.DivRoundUp(width * _texBpp, GobWidth);

            int robSize = GobSize * gbSizes.Height * gbSizes.Depth * widthInGobs;

            int sliceSize = BitUtils.DivRoundUp(height, gbSizes.Height * GobHeight) * robSize;

            return new RobAndSliceSizes(robSize, sliceSize);
        }

        public int GetSwizzleOffset(int x, int y, int z)
        {
            x <<= _bppShift;

            int yh = y / GobHeight;

            int position = (z >> _bdShift) * _sliceSize + (yh >> _bhShift) * _robSize;

            position += (x / GobWidth) << _xShift;

            position += (yh & _bhMask) * GobSize;

            position += ((z & _bdMask) * GobSize) << _bhShift;

            position += ((x & 0x3f) >> 5) << 8;
            position += ((y & 0x07) >> 1) << 6;
            position += ((x & 0x1f) >> 4) << 5;
            position += ((y & 0x01) >> 0) << 4;
            position += ((x & 0x0f) >> 0) << 0;

            return _baseOffset + position;
        }
    }
}