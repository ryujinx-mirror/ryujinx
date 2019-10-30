using Ryujinx.Common;
using System;

using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    public static class SizeCalculator
    {
        private const int StrideAlignment = 32;

        public static SizeInfo GetBlockLinearTextureSize(
            int width,
            int height,
            int depth,
            int levels,
            int layers,
            int blockWidth,
            int blockHeight,
            int bytesPerPixel,
            int gobBlocksInY,
            int gobBlocksInZ,
            int gobBlocksInTileX)
        {
            bool is3D = depth > 1;

            int layerSize = 0;

            int[] allOffsets = new int[levels * layers * depth];
            int[] mipOffsets = new int[levels];

            int mipGobBlocksInY = gobBlocksInY;
            int mipGobBlocksInZ = gobBlocksInZ;

            for (int level = 0; level < levels; level++)
            {
                int w = Math.Max(1, width  >> level);
                int h = Math.Max(1, height >> level);
                int d = Math.Max(1, depth  >> level);

                w = BitUtils.DivRoundUp(w, blockWidth);
                h = BitUtils.DivRoundUp(h, blockHeight);

                while (h <= (mipGobBlocksInY >> 1) * GobHeight && mipGobBlocksInY != 1)
                {
                    mipGobBlocksInY >>= 1;
                }

                while (d <= (mipGobBlocksInZ >> 1) && mipGobBlocksInZ != 1)
                {
                    mipGobBlocksInZ >>= 1;
                }

                int widthInGobs = BitUtils.AlignUp(BitUtils.DivRoundUp(w * bytesPerPixel, GobStride), gobBlocksInTileX);

                int totalBlocksOfGobsInZ = BitUtils.DivRoundUp(d, mipGobBlocksInZ);
                int totalBlocksOfGobsInY = BitUtils.DivRoundUp(BitUtils.DivRoundUp(h, GobHeight), mipGobBlocksInY);

                int robSize = widthInGobs * mipGobBlocksInY * mipGobBlocksInZ * GobSize;

                if (is3D)
                {
                    int gobSize = mipGobBlocksInY * GobSize;

                    int sliceSize = totalBlocksOfGobsInY * widthInGobs * gobSize;

                    int baseOffset = layerSize;

                    int mask = gobBlocksInZ - 1;

                    for (int z = 0; z < d; z++)
                    {
                        int zLow  = z &  mask;
                        int zHigh = z & ~mask;

                        allOffsets[z * levels + level] = baseOffset + zLow * gobSize + zHigh * sliceSize;
                    }
                }

                mipOffsets[level] = layerSize;

                layerSize += totalBlocksOfGobsInZ * totalBlocksOfGobsInY * robSize;
            }

            layerSize = AlignLayerSize(
                layerSize,
                height,
                depth,
                blockHeight,
                gobBlocksInY,
                gobBlocksInZ);

            if (!is3D)
            {
                for (int layer = 0; layer < layers; layer++)
                {
                    int baseIndex  = layer * levels;
                    int baseOffset = layer * layerSize;

                    for (int level = 0; level < levels; level++)
                    {
                        allOffsets[baseIndex + level] = baseOffset + mipOffsets[level];
                    }
                }
            }

            int totalSize = layerSize * layers;

            return new SizeInfo(mipOffsets, allOffsets, levels, layerSize, totalSize);
        }

        public static SizeInfo GetLinearTextureSize(int stride, int height, int blockHeight)
        {
            // Non-2D or mipmapped linear textures are not supported by the Switch GPU,
            // so we only need to handle a single case (2D textures without mipmaps).
            int totalSize = stride * BitUtils.DivRoundUp(height, blockHeight);

            return new SizeInfo(new int[] { 0 }, new int[] { 0 }, 1, totalSize, totalSize);
        }

        private static int AlignLayerSize(
            int size,
            int height,
            int depth,
            int blockHeight,
            int gobBlocksInY,
            int gobBlocksInZ)
        {
            height = BitUtils.DivRoundUp(height, blockHeight);

            while (height <= (gobBlocksInY >> 1) * GobHeight && gobBlocksInY != 1)
            {
                gobBlocksInY >>= 1;
            }

            while (depth <= (gobBlocksInZ >> 1) && gobBlocksInZ != 1)
            {
                gobBlocksInZ >>= 1;
            }

            int blockOfGobsSize = gobBlocksInY * gobBlocksInZ * GobSize;

            int sizeInBlockOfGobs = size / blockOfGobsSize;

            if (size != sizeInBlockOfGobs * blockOfGobsSize)
            {
                size = (sizeInBlockOfGobs + 1) * blockOfGobsSize;
            }

            return size;
        }

        public static Size GetBlockLinearAlignedSize(
            int width,
            int height,
            int depth,
            int blockWidth,
            int blockHeight,
            int bytesPerPixel,
            int gobBlocksInY,
            int gobBlocksInZ,
            int gobBlocksInTileX)
        {
            width  = BitUtils.DivRoundUp(width,  blockWidth);
            height = BitUtils.DivRoundUp(height, blockHeight);

            int gobWidth = gobBlocksInTileX * (GobStride / bytesPerPixel);

            int blockOfGobsHeight = gobBlocksInY * GobHeight;
            int blockOfGobsDepth  = gobBlocksInZ;

            width  = BitUtils.AlignUp(width,  gobWidth);
            height = BitUtils.AlignUp(height, blockOfGobsHeight);
            depth  = BitUtils.AlignUp(depth,  blockOfGobsDepth);

            return new Size(width, height, depth);
        }

        public static Size GetLinearAlignedSize(
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int bytesPerPixel)
        {
            width  = BitUtils.DivRoundUp(width,  blockWidth);
            height = BitUtils.DivRoundUp(height, blockHeight);

            int widthAlignment = StrideAlignment / bytesPerPixel;

            width = BitUtils.AlignUp(width, widthAlignment);

            return new Size(width, height, 1);
        }

        public static (int, int) GetMipGobBlockSizes(
            int height,
            int depth,
            int blockHeight,
            int gobBlocksInY,
            int gobBlocksInZ)
        {
            height = BitUtils.DivRoundUp(height, blockHeight);

            while (height <= (gobBlocksInY >> 1) * GobHeight && gobBlocksInY != 1)
            {
                gobBlocksInY >>= 1;
            }

            while (depth <= (gobBlocksInZ >> 1) && gobBlocksInZ != 1)
            {
                gobBlocksInZ >>= 1;
            }

            return (gobBlocksInY, gobBlocksInZ);
        }
    }
}