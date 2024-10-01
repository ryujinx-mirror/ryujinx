using Ryujinx.Common;
using Ryujinx.Common.Memory;
using System;
using System.Runtime.Intrinsics;
using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    public static class LayoutConverter
    {
        public const int HostStrideAlignment = 4;

        public static void ConvertBlockLinearToLinear(
            Span<byte> dst,
            int width,
            int height,
            int stride,
            int bytesPerPixel,
            int gobBlocksInY,
            ReadOnlySpan<byte> data)
        {
            int gobHeight = gobBlocksInY * GobHeight;

            int strideTrunc = BitUtils.AlignDown(width * bytesPerPixel, 16);
            int strideTrunc64 = BitUtils.AlignDown(width * bytesPerPixel, 64);

            int xStart = strideTrunc / bytesPerPixel;

            int outStrideGap = stride - width * bytesPerPixel;

            int alignment = GobStride / bytesPerPixel;

            int wAligned = BitUtils.AlignUp(width, alignment);

            BlockLinearLayout layoutConverter = new(wAligned, height, gobBlocksInY, 1, bytesPerPixel);

            unsafe bool Convert<T>(Span<byte> output, ReadOnlySpan<byte> data) where T : unmanaged
            {
                fixed (byte* outputPtr = output, dataPtr = data)
                {
                    byte* outPtr = outputPtr;

                    for (int y = 0; y < height; y++)
                    {
                        layoutConverter.SetY(y);

                        for (int x = 0; x < strideTrunc64; x += 64, outPtr += 64)
                        {
                            byte* offset = dataPtr + layoutConverter.GetOffsetWithLineOffset64(x);
                            byte* offset2 = offset + 0x20;
                            byte* offset3 = offset + 0x100;
                            byte* offset4 = offset + 0x120;

                            Vector128<byte> value = *(Vector128<byte>*)offset;
                            Vector128<byte> value2 = *(Vector128<byte>*)offset2;
                            Vector128<byte> value3 = *(Vector128<byte>*)offset3;
                            Vector128<byte> value4 = *(Vector128<byte>*)offset4;

                            *(Vector128<byte>*)outPtr = value;
                            *(Vector128<byte>*)(outPtr + 16) = value2;
                            *(Vector128<byte>*)(outPtr + 32) = value3;
                            *(Vector128<byte>*)(outPtr + 48) = value4;
                        }

                        for (int x = strideTrunc64; x < strideTrunc; x += 16, outPtr += 16)
                        {
                            byte* offset = dataPtr + layoutConverter.GetOffsetWithLineOffset16(x);

                            *(Vector128<byte>*)outPtr = *(Vector128<byte>*)offset;
                        }

                        for (int x = xStart; x < width; x++, outPtr += bytesPerPixel)
                        {
                            byte* offset = dataPtr + layoutConverter.GetOffset(x);

                            *(T*)outPtr = *(T*)offset;
                        }

                        outPtr += outStrideGap;
                    }
                }
                return true;
            }

            bool _ = bytesPerPixel switch
            {
                1 => Convert<byte>(dst, data),
                2 => Convert<ushort>(dst, data),
                4 => Convert<uint>(dst, data),
                8 => Convert<ulong>(dst, data),
                12 => Convert<Bpp12Pixel>(dst, data),
                16 => Convert<Vector128<byte>>(dst, data),
                _ => throw new NotSupportedException($"Unable to convert ${bytesPerPixel} bpp pixel format."),
            };
        }

        public static MemoryOwner<byte> ConvertBlockLinearToLinear(
            int width,
            int height,
            int depth,
            int sliceDepth,
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
                sliceDepth,
                levels,
                layers,
                blockWidth,
                blockHeight,
                bytesPerPixel);

            MemoryOwner<byte> outputOwner = MemoryOwner<byte>.Rent(outSize);
            Span<byte> output = outputOwner.Span;

            int outOffs = 0;

            int mipGobBlocksInY = gobBlocksInY;
            int mipGobBlocksInZ = gobBlocksInZ;

            int gobWidth = (GobStride / bytesPerPixel) * gobBlocksInTileX;
            int gobHeight = gobBlocksInY * GobHeight;

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

                int strideTrunc = BitUtils.AlignDown(w * bytesPerPixel, 16);
                int strideTrunc64 = BitUtils.AlignDown(w * bytesPerPixel, 64);

                int xStart = strideTrunc / bytesPerPixel;

                int stride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

                int outStrideGap = stride - w * bytesPerPixel;

                int alignment = gobWidth;

                if (d < gobBlocksInZ || w <= gobWidth || h <= gobHeight)
                {
                    alignment = GobStride / bytesPerPixel;
                }

                int wAligned = BitUtils.AlignUp(w, alignment);

                BlockLinearLayout layoutConverter = new(
                    wAligned,
                    h,
                    mipGobBlocksInY,
                    mipGobBlocksInZ,
                    bytesPerPixel);

                int sd = Math.Max(1, sliceDepth >> level);

                unsafe bool Convert<T>(Span<byte> output, ReadOnlySpan<byte> data) where T : unmanaged
                {
                    fixed (byte* outputPtr = output, dataPtr = data)
                    {
                        byte* outPtr = outputPtr + outOffs;
                        for (int layer = 0; layer < layers; layer++)
                        {
                            byte* inBaseOffset = dataPtr + (layer * sizeInfo.LayerSize + sizeInfo.GetMipOffset(level));

                            for (int z = 0; z < sd; z++)
                            {
                                layoutConverter.SetZ(z);
                                for (int y = 0; y < h; y++)
                                {
                                    layoutConverter.SetY(y);

                                    for (int x = 0; x < strideTrunc64; x += 64, outPtr += 64)
                                    {
                                        byte* offset = inBaseOffset + layoutConverter.GetOffsetWithLineOffset64(x);
                                        byte* offset2 = offset + 0x20;
                                        byte* offset3 = offset + 0x100;
                                        byte* offset4 = offset + 0x120;

                                        Vector128<byte> value = *(Vector128<byte>*)offset;
                                        Vector128<byte> value2 = *(Vector128<byte>*)offset2;
                                        Vector128<byte> value3 = *(Vector128<byte>*)offset3;
                                        Vector128<byte> value4 = *(Vector128<byte>*)offset4;

                                        *(Vector128<byte>*)outPtr = value;
                                        *(Vector128<byte>*)(outPtr + 16) = value2;
                                        *(Vector128<byte>*)(outPtr + 32) = value3;
                                        *(Vector128<byte>*)(outPtr + 48) = value4;
                                    }

                                    for (int x = strideTrunc64; x < strideTrunc; x += 16, outPtr += 16)
                                    {
                                        byte* offset = inBaseOffset + layoutConverter.GetOffsetWithLineOffset16(x);

                                        *(Vector128<byte>*)outPtr = *(Vector128<byte>*)offset;
                                    }

                                    for (int x = xStart; x < w; x++, outPtr += bytesPerPixel)
                                    {
                                        byte* offset = inBaseOffset + layoutConverter.GetOffset(x);

                                        *(T*)outPtr = *(T*)offset;
                                    }

                                    outPtr += outStrideGap;
                                }
                            }
                        }
                        outOffs += stride * h * d * layers;
                    }
                    return true;
                }

                bool _ = bytesPerPixel switch
                {
                    1 => Convert<byte>(output, data),
                    2 => Convert<ushort>(output, data),
                    4 => Convert<uint>(output, data),
                    8 => Convert<ulong>(output, data),
                    12 => Convert<Bpp12Pixel>(output, data),
                    16 => Convert<Vector128<byte>>(output, data),
                    _ => throw new NotSupportedException($"Unable to convert ${bytesPerPixel} bpp pixel format."),
                };
            }
            return outputOwner;
        }

        public static MemoryOwner<byte> ConvertLinearStridedToLinear(
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int lineSize,
            int stride,
            int bytesPerPixel,
            ReadOnlySpan<byte> data)
        {
            int w = BitUtils.DivRoundUp(width, blockWidth);
            int h = BitUtils.DivRoundUp(height, blockHeight);

            int outStride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);
            lineSize = Math.Min(lineSize, outStride);

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(h * outStride);
            Span<byte> outSpan = output.Span;

            int outOffs = 0;
            int inOffs = 0;

            for (int y = 0; y < h; y++)
            {
                data.Slice(inOffs, lineSize).CopyTo(outSpan.Slice(outOffs, lineSize));

                inOffs += stride;
                outOffs += outStride;
            }

            return output;
        }

        public static void ConvertLinearToBlockLinear(
            Span<byte> dst,
            int width,
            int height,
            int stride,
            int bytesPerPixel,
            int gobBlocksInY,
            ReadOnlySpan<byte> data)
        {
            int gobHeight = gobBlocksInY * GobHeight;

            int strideTrunc = BitUtils.AlignDown(width * bytesPerPixel, 16);
            int strideTrunc64 = BitUtils.AlignDown(width * bytesPerPixel, 64);

            int xStart = strideTrunc / bytesPerPixel;

            int inStrideGap = stride - width * bytesPerPixel;

            int alignment = GobStride / bytesPerPixel;

            int wAligned = BitUtils.AlignUp(width, alignment);

            BlockLinearLayout layoutConverter = new(wAligned, height, gobBlocksInY, 1, bytesPerPixel);

            unsafe bool Convert<T>(Span<byte> output, ReadOnlySpan<byte> data) where T : unmanaged
            {
                fixed (byte* outputPtr = output, dataPtr = data)
                {
                    byte* inPtr = dataPtr;

                    for (int y = 0; y < height; y++)
                    {
                        layoutConverter.SetY(y);

                        for (int x = 0; x < strideTrunc64; x += 64, inPtr += 64)
                        {
                            byte* offset = outputPtr + layoutConverter.GetOffsetWithLineOffset64(x);
                            byte* offset2 = offset + 0x20;
                            byte* offset3 = offset + 0x100;
                            byte* offset4 = offset + 0x120;

                            Vector128<byte> value = *(Vector128<byte>*)inPtr;
                            Vector128<byte> value2 = *(Vector128<byte>*)(inPtr + 16);
                            Vector128<byte> value3 = *(Vector128<byte>*)(inPtr + 32);
                            Vector128<byte> value4 = *(Vector128<byte>*)(inPtr + 48);

                            *(Vector128<byte>*)offset = value;
                            *(Vector128<byte>*)offset2 = value2;
                            *(Vector128<byte>*)offset3 = value3;
                            *(Vector128<byte>*)offset4 = value4;
                        }

                        for (int x = strideTrunc64; x < strideTrunc; x += 16, inPtr += 16)
                        {
                            byte* offset = outputPtr + layoutConverter.GetOffsetWithLineOffset16(x);

                            *(Vector128<byte>*)offset = *(Vector128<byte>*)inPtr;
                        }

                        for (int x = xStart; x < width; x++, inPtr += bytesPerPixel)
                        {
                            byte* offset = outputPtr + layoutConverter.GetOffset(x);

                            *(T*)offset = *(T*)inPtr;
                        }

                        inPtr += inStrideGap;
                    }
                }
                return true;
            }

            bool _ = bytesPerPixel switch
            {
                1 => Convert<byte>(dst, data),
                2 => Convert<ushort>(dst, data),
                4 => Convert<uint>(dst, data),
                8 => Convert<ulong>(dst, data),
                12 => Convert<Bpp12Pixel>(dst, data),
                16 => Convert<Vector128<byte>>(dst, data),
                _ => throw new NotSupportedException($"Unable to convert ${bytesPerPixel} bpp pixel format."),
            };
        }

        public static ReadOnlySpan<byte> ConvertLinearToBlockLinear(
            Span<byte> output,
            int width,
            int height,
            int depth,
            int sliceDepth,
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
            if (output.Length == 0)
            {
                output = new byte[sizeInfo.TotalSize];
            }

            int inOffs = 0;

            int mipGobBlocksInY = gobBlocksInY;
            int mipGobBlocksInZ = gobBlocksInZ;

            int gobWidth = (GobStride / bytesPerPixel) * gobBlocksInTileX;
            int gobHeight = gobBlocksInY * GobHeight;

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

                int strideTrunc = BitUtils.AlignDown(w * bytesPerPixel, 16);
                int strideTrunc64 = BitUtils.AlignDown(w * bytesPerPixel, 64);

                int xStart = strideTrunc / bytesPerPixel;

                int stride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

                int inStrideGap = stride - w * bytesPerPixel;

                int alignment = gobWidth;

                if (d < gobBlocksInZ || w <= gobWidth || h <= gobHeight)
                {
                    alignment = GobStride / bytesPerPixel;
                }

                int wAligned = BitUtils.AlignUp(w, alignment);

                BlockLinearLayout layoutConverter = new(
                    wAligned,
                    h,
                    mipGobBlocksInY,
                    mipGobBlocksInZ,
                    bytesPerPixel);

                int sd = Math.Max(1, sliceDepth >> level);

                unsafe bool Convert<T>(Span<byte> output, ReadOnlySpan<byte> data) where T : unmanaged
                {
                    fixed (byte* outputPtr = output, dataPtr = data)
                    {
                        byte* inPtr = dataPtr + inOffs;
                        for (int layer = 0; layer < layers; layer++)
                        {
                            byte* outBaseOffset = outputPtr + (layer * sizeInfo.LayerSize + sizeInfo.GetMipOffset(level));

                            for (int z = 0; z < sd; z++)
                            {
                                layoutConverter.SetZ(z);
                                for (int y = 0; y < h; y++)
                                {
                                    layoutConverter.SetY(y);

                                    for (int x = 0; x < strideTrunc64; x += 64, inPtr += 64)
                                    {
                                        byte* offset = outBaseOffset + layoutConverter.GetOffsetWithLineOffset64(x);
                                        byte* offset2 = offset + 0x20;
                                        byte* offset3 = offset + 0x100;
                                        byte* offset4 = offset + 0x120;

                                        Vector128<byte> value = *(Vector128<byte>*)inPtr;
                                        Vector128<byte> value2 = *(Vector128<byte>*)(inPtr + 16);
                                        Vector128<byte> value3 = *(Vector128<byte>*)(inPtr + 32);
                                        Vector128<byte> value4 = *(Vector128<byte>*)(inPtr + 48);

                                        *(Vector128<byte>*)offset = value;
                                        *(Vector128<byte>*)offset2 = value2;
                                        *(Vector128<byte>*)offset3 = value3;
                                        *(Vector128<byte>*)offset4 = value4;
                                    }

                                    for (int x = strideTrunc64; x < strideTrunc; x += 16, inPtr += 16)
                                    {
                                        byte* offset = outBaseOffset + layoutConverter.GetOffsetWithLineOffset16(x);

                                        *(Vector128<byte>*)offset = *(Vector128<byte>*)inPtr;
                                    }

                                    for (int x = xStart; x < w; x++, inPtr += bytesPerPixel)
                                    {
                                        byte* offset = outBaseOffset + layoutConverter.GetOffset(x);

                                        *(T*)offset = *(T*)inPtr;
                                    }

                                    inPtr += inStrideGap;
                                }
                            }
                        }
                        inOffs += stride * h * d * layers;
                    }
                    return true;
                }

                bool _ = bytesPerPixel switch
                {
                    1 => Convert<byte>(output, data),
                    2 => Convert<ushort>(output, data),
                    4 => Convert<uint>(output, data),
                    8 => Convert<ulong>(output, data),
                    12 => Convert<Bpp12Pixel>(output, data),
                    16 => Convert<Vector128<byte>>(output, data),
                    _ => throw new NotSupportedException($"Unable to convert ${bytesPerPixel} bpp pixel format."),
                };
            }

            return output;
        }

        public static ReadOnlySpan<byte> ConvertLinearToLinearStrided(
            Span<byte> output,
            int width,
            int height,
            int blockWidth,
            int blockHeight,
            int stride,
            int bytesPerPixel,
            ReadOnlySpan<byte> data)
        {
            int w = BitUtils.DivRoundUp(width, blockWidth);
            int h = BitUtils.DivRoundUp(height, blockHeight);

            int inStride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);
            int lineSize = width * bytesPerPixel;

            if (inStride == stride)
            {
                if (output.Length != 0)
                {
                    data.CopyTo(output);
                    return output;
                }
                else
                {
                    return data;
                }
            }

            if (output.Length == 0)
            {
                output = new byte[h * stride];
            }

            int inOffs = 0;
            int outOffs = 0;

            for (int y = 0; y < h; y++)
            {
                data.Slice(inOffs, lineSize).CopyTo(output.Slice(outOffs, lineSize));

                inOffs += inStride;
                outOffs += stride;
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
                int w = Math.Max(1, width >> level);
                int h = Math.Max(1, height >> level);
                int d = Math.Max(1, depth >> level);

                w = BitUtils.DivRoundUp(w, blockWidth);
                h = BitUtils.DivRoundUp(h, blockHeight);

                int stride = BitUtils.AlignUp(w * bytesPerPixel, HostStrideAlignment);

                layerSize += stride * h * d;
            }

            return layerSize * layers;
        }
    }
}
