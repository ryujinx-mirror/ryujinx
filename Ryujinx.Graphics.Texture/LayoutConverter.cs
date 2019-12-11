using Ryujinx.Common;
using System;

using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    public static class LayoutConverter
    {
        private const int HostStrideAlignment = 4;

        public static Span<byte> ConvertBlockLinearToLinear(
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
            SizeInfo sizeInfo,
            ReadOnlySpan<byte> data)
        {
            int outSize = GetTextureSize(
                width,
                height,
                depth,
                levels,
                layers,
                blockWidth,
                blockHeight,
                bytesPerPixel);

            Span<byte> output = new byte[outSize];

            int outOffs = 0;

            int wAlignment = gobBlocksInTileX * (GobStride / bytesPerPixel);

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

                int strideTrunc = BitUtils.AlignDown(w * bytesPerPixel, 16);

                int xStart = strideTrunc / bytesPerPixel;

                int stride   = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);
                int wAligned = BitUtils.AlignUp(w, wAlignment);

                BlockLinearLayout layoutConverter = new BlockLinearLayout(
                    wAligned,
                    h,
                    d,
                    mipGobBlocksInY,
                    mipGobBlocksInZ,
                    bytesPerPixel);

                for (int layer = 0; layer < layers; layer++)
                {
                    int inBaseOffset = layer * sizeInfo.LayerSize + sizeInfo.GetMipOffset(level);

                    for (int z = 0; z < d; z++)
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < strideTrunc; x += 16)
                        {
                            int offset = inBaseOffset + layoutConverter.GetOffsetWithLineOffset(x, y, z);

                            Span<byte> dest = output.Slice(outOffs + x, 16);

                            data.Slice(offset, 16).CopyTo(dest);
                        }

                        for (int x = xStart; x < w; x++)
                        {
                            int offset = inBaseOffset + layoutConverter.GetOffset(x, y, z);

                            Span<byte> dest = output.Slice(outOffs + x * bytesPerPixel, bytesPerPixel);

                            data.Slice(offset, bytesPerPixel).CopyTo(dest);
                        }

                        outOffs += stride;
                    }
                }
            }

            return output;
        }

        public static Span<byte> ConvertLinearStridedToLinear(
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int stride,
            int bytesPerPixel,
            ReadOnlySpan<byte> data)
        {
            int w = BitUtils.DivRoundUp(width,  blockWidth);
            int h = BitUtils.DivRoundUp(height, blockHeight);

            int outStride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

            Span<byte> output = new byte[h * outStride];

            int outOffs = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * stride + x * bytesPerPixel;

                    Span<byte> dest = output.Slice(outOffs + x * bytesPerPixel, bytesPerPixel);

                    data.Slice(offset, bytesPerPixel).CopyTo(dest);
                }

                outOffs += outStride;
            }

            return output;
        }

        public static Span<byte> ConvertLinearToBlockLinear(
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
            SizeInfo sizeInfo,
            ReadOnlySpan<byte> data)
        {
            Span<byte> output = new byte[sizeInfo.TotalSize];

            int inOffs = 0;

            int wAlignment = gobBlocksInTileX * (GobStride / bytesPerPixel);

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

                int stride   = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);
                int wAligned = BitUtils.AlignUp(w, wAlignment);

                BlockLinearLayout layoutConverter = new BlockLinearLayout(
                    wAligned,
                    h,
                    d,
                    mipGobBlocksInY,
                    mipGobBlocksInZ,
                    bytesPerPixel);

                for (int layer = 0; layer < layers; layer++)
                {
                    int outBaseOffset = layer * sizeInfo.LayerSize + sizeInfo.GetMipOffset(level);

                    for (int z = 0; z < d; z++)
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int offset = outBaseOffset + layoutConverter.GetOffset(x, y, z);

                            Span<byte> dest = output.Slice(offset, bytesPerPixel);

                            data.Slice(inOffs + x * bytesPerPixel, bytesPerPixel).CopyTo(dest);
                        }

                        inOffs += stride;
                    }
                }
            }

            return output;
        }

        public static Span<byte> ConvertLinearToLinearStrided(
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int stride,
            int bytesPerPixel,
            ReadOnlySpan<byte> data)
        {
            int w = BitUtils.DivRoundUp(width,  blockWidth);
            int h = BitUtils.DivRoundUp(height, blockHeight);

            int inStride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

            Span<byte> output = new byte[h * stride];

            int inOffs = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * stride + x * bytesPerPixel;

                    Span<byte> dest = output.Slice(offset, bytesPerPixel);

                    data.Slice(inOffs + x * bytesPerPixel, bytesPerPixel).CopyTo(dest);
                }

                inOffs += inStride;
            }

            return output;
        }

        private static int GetTextureSize(
            int width,
            int height,
            int depth,
            int levels,
            int layers,
            int blockWidth,
            int blockHeight,
            int bytesPerPixel)
        {
            int layerSize = 0;

            for (int level = 0; level < levels; level++)
            {
                int w = Math.Max(1, width  >> level);
                int h = Math.Max(1, height >> level);
                int d = Math.Max(1, depth  >> level);

                w = BitUtils.DivRoundUp(w, blockWidth);
                h = BitUtils.DivRoundUp(h, blockHeight);

                int stride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

                layerSize += stride * h * d;
            }

            return layerSize * layers;
        }
    }
}