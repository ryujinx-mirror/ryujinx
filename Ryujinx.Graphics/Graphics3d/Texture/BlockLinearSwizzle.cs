using Ryujinx.Common;
using System;

namespace Ryujinx.Graphics.Texture
{
    class BlockLinearSwizzle : ISwizzle
    {
        private const int GobWidth  = 64;
        private const int GobHeight = 8;

        private const int GobSize = GobWidth * GobHeight;

        private int TexWidth;
        private int TexHeight;
        private int TexDepth;
        private int TexGobBlockHeight;
        private int TexGobBlockDepth;
        private int TexBpp;

        private int BhMask;
        private int BdMask;

        private int BhShift;
        private int BdShift;
        private int BppShift;

        private int XShift;

        private int RobSize;
        private int SliceSize;

        private int BaseOffset;

        public BlockLinearSwizzle(
            int Width,
            int Height,
            int Depth,
            int GobBlockHeight,
            int GobBlockDepth,
            int Bpp)
        {
            TexWidth          = Width;
            TexHeight         = Height;
            TexDepth          = Depth;
            TexGobBlockHeight = GobBlockHeight;
            TexGobBlockDepth  = GobBlockDepth;
            TexBpp            = Bpp;

            BppShift = BitUtils.CountTrailingZeros32(Bpp);

            SetMipLevel(0);
        }

        public void SetMipLevel(int Level)
        {
            BaseOffset = GetMipOffset(Level);

            int Width  = Math.Max(1, TexWidth  >> Level);
            int Height = Math.Max(1, TexHeight >> Level);
            int Depth  = Math.Max(1, TexDepth  >> Level);

            GobBlockSizes GbSizes = AdjustGobBlockSizes(Height, Depth);

            BhMask = GbSizes.Height - 1;
            BdMask = GbSizes.Depth  - 1;

            BhShift = BitUtils.CountTrailingZeros32(GbSizes.Height);
            BdShift = BitUtils.CountTrailingZeros32(GbSizes.Depth);

            XShift = BitUtils.CountTrailingZeros32(GobSize * GbSizes.Height * GbSizes.Depth);

            RobAndSliceSizes GsSizes = GetRobAndSliceSizes(Width, Height, GbSizes);

            RobSize   = GsSizes.RobSize;
            SliceSize = GsSizes.SliceSize;
        }

        public int GetImageSize(int MipsCount)
        {
            int Size = GetMipOffset(MipsCount);

            Size = (Size + 0x1fff) & ~0x1fff;

            return Size;
        }

        public int GetMipOffset(int Level)
        {
            int TotalSize = 0;

            for (int Index = 0; Index < Level; Index++)
            {
                int Width  = Math.Max(1, TexWidth  >> Index);
                int Height = Math.Max(1, TexHeight >> Index);
                int Depth  = Math.Max(1, TexDepth  >> Index);

                GobBlockSizes GbSizes = AdjustGobBlockSizes(Height, Depth);

                RobAndSliceSizes RsSizes = GetRobAndSliceSizes(Width, Height, GbSizes);

                TotalSize += BitUtils.DivRoundUp(Depth, GbSizes.Depth) * RsSizes.SliceSize;
            }

            return TotalSize;
        }

        private struct GobBlockSizes
        {
            public int Height;
            public int Depth;

            public GobBlockSizes(int GobBlockHeight, int GobBlockDepth)
            {
                this.Height = GobBlockHeight;
                this.Depth  = GobBlockDepth;
            }
        }

        private GobBlockSizes AdjustGobBlockSizes(int Height, int Depth)
        {
            int GobBlockHeight = TexGobBlockHeight;
            int GobBlockDepth  = TexGobBlockDepth;

            int Pow2Height = BitUtils.Pow2RoundUp(Height);
            int Pow2Depth  = BitUtils.Pow2RoundUp(Depth);

            while (GobBlockHeight * GobHeight > Pow2Height && GobBlockHeight > 1)
            {
                GobBlockHeight >>= 1;
            }

            while (GobBlockDepth > Pow2Depth && GobBlockDepth > 1)
            {
                GobBlockDepth >>= 1;
            }

            return new GobBlockSizes(GobBlockHeight, GobBlockDepth);
        }

        private struct RobAndSliceSizes
        {
            public int RobSize;
            public int SliceSize;

            public RobAndSliceSizes(int RobSize, int SliceSize)
            {
                this.RobSize   = RobSize;
                this.SliceSize = SliceSize;
            }
        }

        private RobAndSliceSizes GetRobAndSliceSizes(int Width, int Height, GobBlockSizes GbSizes)
        {
            int WidthInGobs = BitUtils.DivRoundUp(Width * TexBpp, GobWidth);

            int RobSize = GobSize * GbSizes.Height * GbSizes.Depth * WidthInGobs;

            int SliceSize = BitUtils.DivRoundUp(Height, GbSizes.Height * GobHeight) * RobSize;

            return new RobAndSliceSizes(RobSize, SliceSize);
        }

        public int GetSwizzleOffset(int X, int Y, int Z)
        {
            X <<= BppShift;

            int YH = Y / GobHeight;

            int Position = (Z >> BdShift) * SliceSize + (YH >> BhShift) * RobSize;

            Position += (X / GobWidth) << XShift;

            Position += (YH & BhMask) * GobSize;

            Position += ((Z & BdMask) * GobSize) << BhShift;

            Position += ((X & 0x3f) >> 5) << 8;
            Position += ((Y & 0x07) >> 1) << 6;
            Position += ((X & 0x1f) >> 4) << 5;
            Position += ((Y & 0x01) >> 0) << 4;
            Position += ((X & 0x0f) >> 0) << 0;

            return BaseOffset + Position;
        }
    }
}