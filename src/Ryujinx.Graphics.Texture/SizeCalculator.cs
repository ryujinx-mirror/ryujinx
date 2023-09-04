using Ryujinx.Common;
using System;
using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    public static class SizeCalculator
    {
        private const int StrideAlignment = 32;

        private static int Calculate3DOffsetCount(int levels, int depth)
        {
            int offsetCount = depth;

            while (--levels > 0)
            {
                depth = Math.Max(1, depth >> 1);
                offsetCount += depth;
            }

            return offsetCount;
        }

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
            int gobBlocksInTileX,
            int gpuLayerSize = 0)
        {
            bool is3D = depth > 1 || gobBlocksInZ > 1;

            int layerSize = 0;
            int layerSizeAligned = 0;

            int[] allOffsets = new int[is3D ? Calculate3DOffsetCount(levels, depth) : levels * layers * depth];
            int[] mipOffsets = new int[levels];
            int[] sliceSizes = new int[levels];
            int[] levelSizes = new int[levels];

            int mipGobBlocksInY = gobBlocksInY;
            int mipGobBlocksInZ = gobBlocksInZ;

            int gobWidth = (GobStride / bytesPerPixel) * gobBlocksInTileX;
            int gobHeight = gobBlocksInY * GobHeight;

            int depthLevelOffset = 0;

            for (int level = 0; level < levels; level++)
            {
                int w = Math.Max(1, width >> level);
                int h = Math.Max(1, height >> level);
                int d = Math.Max(1, depth >> level);

                w = BitUtils.DivRoundUp(w, blockWidth);
                h = BitUtils.DivRoundUp(h, blockHeight);

                while (h <= (mipGobBlocksInY >> 1) * GobHeight && mipGobBlocksInY != 1)
                {
                    mipGobBlocksInY >>= 1;
                }

                if (level > 0 && d <= (mipGobBlocksInZ >> 1) && mipGobBlocksInZ != 1)
                {
                    mipGobBlocksInZ >>= 1;
                }

                int widthInGobs = BitUtils.DivRoundUp(w * bytesPerPixel, GobStride);

                int alignment = gobBlocksInTileX;

                if (d < gobBlocksInZ || w <= gobWidth || h <= gobHeight)
                {
                    alignment = 1;
                }

                widthInGobs = BitUtils.AlignUp(widthInGobs, alignment);

                int totalBlocksOfGobsInZ = BitUtils.DivRoundUp(d, mipGobBlocksInZ);
                int totalBlocksOfGobsInY = BitUtils.DivRoundUp(BitUtils.DivRoundUp(h, GobHeight), mipGobBlocksInY);

                int robSize = widthInGobs * mipGobBlocksInY * mipGobBlocksInZ * GobSize;

                mipOffsets[level] = layerSize;
                sliceSizes[level] = totalBlocksOfGobsInY * robSize;
                levelSizes[level] = totalBlocksOfGobsInZ * sliceSizes[level];

                layerSizeAligned += levelSizes[level];

                if (is3D)
                {
                    int gobSize = mipGobBlocksInY * GobSize;

                    int sliceSize = totalBlocksOfGobsInY * widthInGobs * gobSize;

                    int baseOffset = layerSize;

                    int mask = gobBlocksInZ - 1;

                    for (int z = 0; z < d; z++)
                    {
                        int zLow = z & mask;
                        int zHigh = z & ~mask;

                        allOffsets[z + depthLevelOffset] = baseOffset + zLow * gobSize + zHigh * sliceSize;
                    }

                    int gobRemainderZ = d % mipGobBlocksInZ;

                    if (gobRemainderZ != 0 && level == levels - 1)
                    {
                        // The slice only covers up to the end of this slice's depth, rather than the full aligned size.
                        // Avoids size being too large on partial views of 3d textures.

                        levelSizes[level] -= gobSize * (mipGobBlocksInZ - gobRemainderZ);

                        if (sliceSizes[level] > levelSizes[level])
                        {
                            sliceSizes[level] = levelSizes[level];
                        }
                    }
                }

                layerSize += levelSizes[level];

                depthLevelOffset += d;
            }

            int totalSize;

            if (layers > 1)
            {
                layerSizeAligned = AlignLayerSize(
                    layerSizeAligned,
                    height,
                    depth,
                    blockHeight,
                    gobBlocksInY,
                    gobBlocksInZ,
                    gobBlocksInTileX);

                if (layerSizeAligned < gpuLayerSize)
                {
                    totalSize = (layers - 1) * gpuLayerSize + layerSizeAligned;
                    layerSizeAligned = gpuLayerSize;
                }
                else
                {
                    totalSize = layerSizeAligned * layers;
                }
            }
            else
            {
                totalSize = layerSize;
            }

            if (!is3D)
            {
                for (int layer = 0; layer < layers; layer++)
                {
                    int baseIndex = layer * levels;
                    int baseOffset = layer * layerSizeAligned;

                    for (int level = 0; level < levels; level++)
                    {
                        allOffsets[baseIndex + level] = baseOffset + mipOffsets[level];
                    }
                }
            }

            return new SizeInfo(mipOffsets, allOffsets, sliceSizes, levelSizes, depth, levels, layerSizeAligned, totalSize, is3D);
        }

        public static SizeInfo GetLinearTextureSize(int stride, int height, int blockHeight)
        {
            // Non-2D or mipmapped linear textures are not supported by the Switch GPU,
            // so we only need to handle a single case (2D textures without mipmaps).
            int totalSize = stride * BitUtils.DivRoundUp(height, blockHeight);

            return new SizeInfo(totalSize);
        }

        private static int AlignLayerSize(
            int size,
            int height,
            int depth,
            int blockHeight,
            int gobBlocksInY,
            int gobBlocksInZ,
            int gobBlocksInTileX)
        {
            if (gobBlocksInTileX < 2)
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
            }
            else
            {
                int alignment = (gobBlocksInTileX * GobSize) * gobBlocksInY * gobBlocksInZ;

                size = BitUtils.AlignUp(size, alignment);
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
            width = BitUtils.DivRoundUp(width, blockWidth);
            height = BitUtils.DivRoundUp(height, blockHeight);

            int gobWidth = (GobStride / bytesPerPixel) * gobBlocksInTileX;
            int gobHeight = gobBlocksInY * GobHeight;

            int alignment = gobWidth;

            if (depth < gobBlocksInZ || width <= gobWidth || height <= gobHeight)
            {
                alignment = GobStride / bytesPerPixel;
            }

            // Height has already been divided by block height, so pass it as 1.
            (gobBlocksInY, gobBlocksInZ) = GetMipGobBlockSizes(height, depth, 1, gobBlocksInY, gobBlocksInZ);

            int blockOfGobsHeight = gobBlocksInY * GobHeight;
            int blockOfGobsDepth = gobBlocksInZ;

            width = BitUtils.AlignUp(width, alignment);
            height = BitUtils.AlignUp(height, blockOfGobsHeight);
            depth = BitUtils.AlignUp(depth, blockOfGobsDepth);

            return new Size(width, height, depth);
        }

        public static Size GetLinearAlignedSize(
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int bytesPerPixel)
        {
            width = BitUtils.DivRoundUp(width, blockWidth);
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
            int gobBlocksInZ,
            int level = int.MaxValue)
        {
            height = BitUtils.DivRoundUp(height, blockHeight);

            while (height <= (gobBlocksInY >> 1) * GobHeight && gobBlocksInY != 1)
            {
                gobBlocksInY >>= 1;
            }

            while (level-- > 0 && depth <= (gobBlocksInZ >> 1) && gobBlocksInZ != 1)
            {
                gobBlocksInZ >>= 1;
            }

            return (gobBlocksInY, gobBlocksInZ);
        }
    }
}
