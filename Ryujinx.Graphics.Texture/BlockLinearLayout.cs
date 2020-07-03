using Ryujinx.Common;
using System.Runtime.CompilerServices;

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

        private int _texBpp;

        private int _bhMask;
        private int _bdMask;

        private int _bhShift;
        private int _bdShift;
        private int _bppShift;

        private int _xShift;

        private int _robSize;
        private int _sliceSize;

        // Variables for built in iteration.
        private int _yPart;
        private int _yzPart;
        private int _zPart;

        public BlockLinearLayout(
            int width,
            int height,
            int gobBlocksInY,
            int gobBlocksInZ,
            int bpp)
        {
            _texBpp = bpp;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int x, int y, int z)
        {
            return GetOffsetWithLineOffset(x << _bppShift, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffsetWithLineOffset(int x, int y, int z)
        {
            int yh = y / GobHeight;

            int offset = (z >> _bdShift) * _sliceSize + (yh >> _bhShift) * _robSize;

            offset += (x / GobStride) << _xShift;

            offset += (yh & _bhMask) * GobSize;

            offset += ((z & _bdMask) * GobSize) << _bhShift;

            offset += ((x & 0x3f) >> 5) << 8;
            offset += ((y & 0x07) >> 1) << 6;
            offset += ((x & 0x1f) >> 4) << 5;
            offset += ((y & 0x01) >> 0) << 4;
            offset += ((x & 0x0f) >> 0) << 0;

            return offset;
        }

        public (int offset, int size) GetRectangleRange(int x, int y, int width, int height)
        {
            // Justification:
            // The 2D offset is a combination of separate x and y parts.
            // Both components increase with input and never overlap bits.
            // Therefore for each component, the minimum input value is the lowest that component can go.
            // Minimum total value is minimum X component + minimum Y component. Similar goes for maximum.

            int start = GetOffset(x, y, 0);
            int end = GetOffset(x + width - 1, y + height - 1, 0) + _texBpp; // Cover the last pixel.
            return (start, end - start);
        }

        public bool LayoutMatches(BlockLinearLayout other)
        {
            return _robSize == other._robSize &&
                   _sliceSize == other._sliceSize &&
                   _texBpp == other._texBpp &&
                   _bhMask == other._bhMask &&
                   _bdMask == other._bdMask;
        }

        // Functions for built in iteration.
        // Components of the offset can be updated separately, and combined to save some time.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetY(int y)
        {
            int yh = y / GobHeight;
            int offset = (yh >> _bhShift) * _robSize;

            offset += (yh & _bhMask) * GobSize;

            offset += ((y & 0x07) >> 1) << 6;
            offset += ((y & 0x01) >> 0) << 4;

            _yPart = offset;
            _yzPart = offset + _zPart;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetZ(int z)
        {
            int offset = (z >> _bdShift) * _sliceSize;

            offset += ((z & _bdMask) * GobSize) << _bhShift;

            _zPart = offset;
            _yzPart = offset + _yPart;
        }

        /// <summary>
        /// Optimized conversion for line offset in bytes to an absolute offset. Input x must be divisible by 16.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffsetWithLineOffset16(int x)
        {
            int offset = (x / GobStride) << _xShift;

            offset += ((x & 0x3f) >> 5) << 8;
            offset += ((x & 0x1f) >> 4) << 5;

            return offset + _yzPart;
        }

        /// <summary>
        /// Optimized conversion for line offset in bytes to an absolute offset. Input x must be divisible by 64.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffsetWithLineOffset64(int x)
        {
            int offset = (x / GobStride) << _xShift;

            return offset + _yzPart;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int x)
        {
            x <<= _bppShift;
            int offset = (x / GobStride) << _xShift;

            offset += ((x & 0x3f) >> 5) << 8;
            offset += ((x & 0x1f) >> 4) << 5;
            offset += (x & 0x0f);

            return offset + _yzPart;
        }
    }
}