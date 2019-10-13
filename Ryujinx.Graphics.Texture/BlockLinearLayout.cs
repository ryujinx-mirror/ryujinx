using Ryujinx.Common;
using System;

using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    class BlockLinearLayout
    {
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

        private int _texWidth;
        private int _texHeight;
        private int _texDepth;
        private int _texGobBlocksInY;
        private int _texGobBlocksInZ;
        private int _texBpp;

        private int _bhMask;
        private int _bdMask;

        private int _bhShift;
        private int _bdShift;
        private int _bppShift;

        private int _xShift;

        private int _robSize;
        private int _sliceSize;

        public BlockLinearLayout(
            int width,
            int height,
            int depth,
            int gobBlocksInY,
            int gobBlocksInZ,
            int bpp)
        {
            _texWidth        = width;
            _texHeight       = height;
            _texDepth        = depth;
            _texGobBlocksInY = gobBlocksInY;
            _texGobBlocksInZ = gobBlocksInZ;
            _texBpp          = bpp;

            _bppShift = BitUtils.CountTrailingZeros32(bpp);

            _bhMask = gobBlocksInY - 1;
            _bdMask = gobBlocksInZ - 1;

            _bhShift = BitUtils.CountTrailingZeros32(gobBlocksInY);
            _bdShift = BitUtils.CountTrailingZeros32(gobBlocksInZ);

            _xShift = BitUtils.CountTrailingZeros32(GobSize * gobBlocksInY * gobBlocksInZ);

            RobAndSliceSizes rsSizes = GetRobAndSliceSizes(width, height, gobBlocksInY, gobBlocksInZ);

            _robSize   = rsSizes.RobSize;
            _sliceSize = rsSizes.SliceSize;
        }

        private RobAndSliceSizes GetRobAndSliceSizes(int width, int height, int gobBlocksInY, int gobBlocksInZ)
        {
            int widthInGobs = BitUtils.DivRoundUp(width * _texBpp, GobStride);

            int robSize = GobSize * gobBlocksInY * gobBlocksInZ * widthInGobs;

            int sliceSize = BitUtils.DivRoundUp(height, gobBlocksInY * GobHeight) * robSize;

            return new RobAndSliceSizes(robSize, sliceSize);
        }

        public int GetOffset(int x, int y, int z)
        {
            x <<= _bppShift;

            int yh = y / GobHeight;

            int position = (z >> _bdShift) * _sliceSize + (yh >> _bhShift) * _robSize;

            position += (x / GobStride) << _xShift;

            position += (yh & _bhMask) * GobSize;

            position += ((z & _bdMask) * GobSize) << _bhShift;

            position += ((x & 0x3f) >> 5) << 8;
            position += ((y & 0x07) >> 1) << 6;
            position += ((x & 0x1f) >> 4) << 5;
            position += ((y & 0x01) >> 0) << 4;
            position += ((x & 0x0f) >> 0) << 0;

            return position;
        }
    }
}