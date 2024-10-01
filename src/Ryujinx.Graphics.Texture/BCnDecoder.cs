using Ryujinx.Common;
using Ryujinx.Common.Memory;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Texture
{
    public static class BCnDecoder
    {
        private const int BlockWidth = 4;
        private const int BlockHeight = 4;

        public static MemoryOwner<byte> DecodeBC1(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 4;
            }

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);

            Span<byte> tile = stackalloc byte[BlockWidth * BlockHeight * 4];

            Span<uint> tileAsUint = MemoryMarshal.Cast<byte, uint>(tile);
            Span<uint> outputAsUint = MemoryMarshal.Cast<byte, uint>(output.Span);

            Span<Vector128<byte>> tileAsVector128 = MemoryMarshal.Cast<byte, Vector128<byte>>(tile);

            Span<Vector128<byte>> outputLine0 = default;
            Span<Vector128<byte>> outputLine1 = default;
            Span<Vector128<byte>> outputLine2 = default;
            Span<Vector128<byte>> outputLine3 = default;

            int imageBaseOOffs = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int baseY = y * BlockHeight;
                            int copyHeight = Math.Min(BlockHeight, height - baseY);
                            int lineBaseOOffs = imageBaseOOffs + baseY * width;

                            if (copyHeight == 4)
                            {
                                outputLine0 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[lineBaseOOffs..]);
                                outputLine1 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width)..]);
                                outputLine2 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 2)..]);
                                outputLine3 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 3)..]);
                            }

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int copyWidth = Math.Min(BlockWidth, width - baseX);

                                BC1DecodeTileRgb(tile, data);

                                if ((copyWidth | copyHeight) == 4)
                                {
                                    outputLine0[x] = tileAsVector128[0];
                                    outputLine1[x] = tileAsVector128[1];
                                    outputLine2[x] = tileAsVector128[2];
                                    outputLine3[x] = tileAsVector128[3];
                                }
                                else
                                {
                                    int pixelBaseOOffs = lineBaseOOffs + baseX;

                                    for (int tY = 0; tY < copyHeight; tY++)
                                    {
                                        tileAsUint.Slice(tY * 4, copyWidth).CopyTo(outputAsUint.Slice(pixelBaseOOffs + width * tY, copyWidth));
                                    }
                                }

                                data = data[8..];
                            }
                        }

                        imageBaseOOffs += width * height;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC2(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 4;
            }

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);

            Span<byte> tile = stackalloc byte[BlockWidth * BlockHeight * 4];

            Span<uint> tileAsUint = MemoryMarshal.Cast<byte, uint>(tile);
            Span<uint> outputAsUint = MemoryMarshal.Cast<byte, uint>(output.Span);

            Span<Vector128<byte>> tileAsVector128 = MemoryMarshal.Cast<byte, Vector128<byte>>(tile);

            Span<Vector128<byte>> outputLine0 = default;
            Span<Vector128<byte>> outputLine1 = default;
            Span<Vector128<byte>> outputLine2 = default;
            Span<Vector128<byte>> outputLine3 = default;

            int imageBaseOOffs = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int baseY = y * BlockHeight;
                            int copyHeight = Math.Min(BlockHeight, height - baseY);
                            int lineBaseOOffs = imageBaseOOffs + baseY * width;

                            if (copyHeight == 4)
                            {
                                outputLine0 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[lineBaseOOffs..]);
                                outputLine1 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width)..]);
                                outputLine2 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 2)..]);
                                outputLine3 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 3)..]);
                            }

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int copyWidth = Math.Min(BlockWidth, width - baseX);

                                BC23DecodeTileRgb(tile, data[8..]);

                                ulong block = BinaryPrimitives.ReadUInt64LittleEndian(data);

                                for (int i = 3; i < BlockWidth * BlockHeight * 4; i += 4, block >>= 4)
                                {
                                    tile[i] = (byte)((block & 0xf) | (block << 4));
                                }

                                if ((copyWidth | copyHeight) == 4)
                                {
                                    outputLine0[x] = tileAsVector128[0];
                                    outputLine1[x] = tileAsVector128[1];
                                    outputLine2[x] = tileAsVector128[2];
                                    outputLine3[x] = tileAsVector128[3];
                                }
                                else
                                {
                                    int pixelBaseOOffs = lineBaseOOffs + baseX;

                                    for (int tY = 0; tY < copyHeight; tY++)
                                    {
                                        tileAsUint.Slice(tY * 4, copyWidth).CopyTo(outputAsUint.Slice(pixelBaseOOffs + width * tY, copyWidth));
                                    }
                                }

                                data = data[16..];
                            }
                        }

                        imageBaseOOffs += width * height;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC3(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 4;
            }

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);

            Span<byte> tile = stackalloc byte[BlockWidth * BlockHeight * 4];
            Span<byte> rPal = stackalloc byte[8];

            Span<uint> tileAsUint = MemoryMarshal.Cast<byte, uint>(tile);
            Span<uint> outputAsUint = MemoryMarshal.Cast<byte, uint>(output.Span);

            Span<Vector128<byte>> tileAsVector128 = MemoryMarshal.Cast<byte, Vector128<byte>>(tile);

            Span<Vector128<byte>> outputLine0 = default;
            Span<Vector128<byte>> outputLine1 = default;
            Span<Vector128<byte>> outputLine2 = default;
            Span<Vector128<byte>> outputLine3 = default;

            int imageBaseOOffs = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int baseY = y * BlockHeight;
                            int copyHeight = Math.Min(BlockHeight, height - baseY);
                            int lineBaseOOffs = imageBaseOOffs + baseY * width;

                            if (copyHeight == 4)
                            {
                                outputLine0 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[lineBaseOOffs..]);
                                outputLine1 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width)..]);
                                outputLine2 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 2)..]);
                                outputLine3 = MemoryMarshal.Cast<uint, Vector128<byte>>(outputAsUint[(lineBaseOOffs + width * 3)..]);
                            }

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int copyWidth = Math.Min(BlockWidth, width - baseX);

                                BC23DecodeTileRgb(tile, data[8..]);

                                ulong block = BinaryPrimitives.ReadUInt64LittleEndian(data);

                                rPal[0] = (byte)block;
                                rPal[1] = (byte)(block >> 8);

                                BCnLerpAlphaUnorm(rPal);
                                BCnDecodeTileAlphaRgba(tile, rPal, block >> 16);

                                if ((copyWidth | copyHeight) == 4)
                                {
                                    outputLine0[x] = tileAsVector128[0];
                                    outputLine1[x] = tileAsVector128[1];
                                    outputLine2[x] = tileAsVector128[2];
                                    outputLine3[x] = tileAsVector128[3];
                                }
                                else
                                {
                                    int pixelBaseOOffs = lineBaseOOffs + baseX;

                                    for (int tY = 0; tY < copyHeight; tY++)
                                    {
                                        tileAsUint.Slice(tY * 4, copyWidth).CopyTo(outputAsUint.Slice(pixelBaseOOffs + width * tY, copyWidth));
                                    }
                                }

                                data = data[16..];
                            }
                        }

                        imageBaseOOffs += width * height;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC4(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers, bool signed)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += BitUtils.AlignUp(Math.Max(1, width >> l), 4) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers;
            }

            // Backends currently expect a stride alignment of 4 bytes, so output width must be aligned.
            int alignedWidth = BitUtils.AlignUp(width, 4);

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);
            Span<byte> outputSpan = output.Span;

            ReadOnlySpan<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);

            Span<byte> tile = stackalloc byte[BlockWidth * BlockHeight];
            Span<byte> rPal = stackalloc byte[8];

            Span<uint> tileAsUint = MemoryMarshal.Cast<byte, uint>(tile);

            Span<uint> outputLine0 = default;
            Span<uint> outputLine1 = default;
            Span<uint> outputLine2 = default;
            Span<uint> outputLine3 = default;

            int imageBaseOOffs = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int baseY = y * BlockHeight;
                            int copyHeight = Math.Min(BlockHeight, height - baseY);
                            int lineBaseOOffs = imageBaseOOffs + baseY * alignedWidth;

                            if (copyHeight == 4)
                            {
                                outputLine0 = MemoryMarshal.Cast<byte, uint>(outputSpan[lineBaseOOffs..]);
                                outputLine1 = MemoryMarshal.Cast<byte, uint>(outputSpan[(lineBaseOOffs + alignedWidth)..]);
                                outputLine2 = MemoryMarshal.Cast<byte, uint>(outputSpan[(lineBaseOOffs + alignedWidth * 2)..]);
                                outputLine3 = MemoryMarshal.Cast<byte, uint>(outputSpan[(lineBaseOOffs + alignedWidth * 3)..]);
                            }

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int copyWidth = Math.Min(BlockWidth, width - baseX);

                                ulong block = data64[0];

                                rPal[0] = (byte)block;
                                rPal[1] = (byte)(block >> 8);

                                if (signed)
                                {
                                    BCnLerpAlphaSnorm(rPal);
                                }
                                else
                                {
                                    BCnLerpAlphaUnorm(rPal);
                                }

                                BCnDecodeTileAlpha(tile, rPal, block >> 16);

                                if ((copyWidth | copyHeight) == 4)
                                {
                                    outputLine0[x] = tileAsUint[0];
                                    outputLine1[x] = tileAsUint[1];
                                    outputLine2[x] = tileAsUint[2];
                                    outputLine3[x] = tileAsUint[3];
                                }
                                else
                                {
                                    int pixelBaseOOffs = lineBaseOOffs + baseX;

                                    for (int tY = 0; tY < copyHeight; tY++)
                                    {
                                        tile.Slice(tY * 4, copyWidth).CopyTo(outputSpan.Slice(pixelBaseOOffs + alignedWidth * tY, copyWidth));
                                    }
                                }

                                data64 = data64[1..];
                            }
                        }

                        imageBaseOOffs += alignedWidth * height;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);

                alignedWidth = BitUtils.AlignUp(width, 4);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC5(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers, bool signed)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += BitUtils.AlignUp(Math.Max(1, width >> l), 2) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 2;
            }

            // Backends currently expect a stride alignment of 4 bytes, so output width must be aligned.
            int alignedWidth = BitUtils.AlignUp(width, 2);

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);

            ReadOnlySpan<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);

            Span<byte> rTile = stackalloc byte[BlockWidth * BlockHeight * 2];
            Span<byte> gTile = stackalloc byte[BlockWidth * BlockHeight * 2];
            Span<byte> rPal = stackalloc byte[8];
            Span<byte> gPal = stackalloc byte[8];

            Span<ushort> outputAsUshort = MemoryMarshal.Cast<byte, ushort>(output.Span);

            Span<uint> rTileAsUint = MemoryMarshal.Cast<byte, uint>(rTile);
            Span<uint> gTileAsUint = MemoryMarshal.Cast<byte, uint>(gTile);

            Span<ulong> outputLine0 = default;
            Span<ulong> outputLine1 = default;
            Span<ulong> outputLine2 = default;
            Span<ulong> outputLine3 = default;

            int imageBaseOOffs = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int baseY = y * BlockHeight;
                            int copyHeight = Math.Min(BlockHeight, height - baseY);
                            int lineBaseOOffs = imageBaseOOffs + baseY * alignedWidth;

                            if (copyHeight == 4)
                            {
                                outputLine0 = MemoryMarshal.Cast<ushort, ulong>(outputAsUshort[lineBaseOOffs..]);
                                outputLine1 = MemoryMarshal.Cast<ushort, ulong>(outputAsUshort[(lineBaseOOffs + alignedWidth)..]);
                                outputLine2 = MemoryMarshal.Cast<ushort, ulong>(outputAsUshort[(lineBaseOOffs + alignedWidth * 2)..]);
                                outputLine3 = MemoryMarshal.Cast<ushort, ulong>(outputAsUshort[(lineBaseOOffs + alignedWidth * 3)..]);
                            }

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int copyWidth = Math.Min(BlockWidth, width - baseX);

                                ulong blockL = data64[0];
                                ulong blockH = data64[1];

                                rPal[0] = (byte)blockL;
                                rPal[1] = (byte)(blockL >> 8);
                                gPal[0] = (byte)blockH;
                                gPal[1] = (byte)(blockH >> 8);

                                if (signed)
                                {
                                    BCnLerpAlphaSnorm(rPal);
                                    BCnLerpAlphaSnorm(gPal);
                                }
                                else
                                {
                                    BCnLerpAlphaUnorm(rPal);
                                    BCnLerpAlphaUnorm(gPal);
                                }

                                BCnDecodeTileAlpha(rTile, rPal, blockL >> 16);
                                BCnDecodeTileAlpha(gTile, gPal, blockH >> 16);

                                if ((copyWidth | copyHeight) == 4)
                                {
                                    outputLine0[x] = InterleaveBytes(rTileAsUint[0], gTileAsUint[0]);
                                    outputLine1[x] = InterleaveBytes(rTileAsUint[1], gTileAsUint[1]);
                                    outputLine2[x] = InterleaveBytes(rTileAsUint[2], gTileAsUint[2]);
                                    outputLine3[x] = InterleaveBytes(rTileAsUint[3], gTileAsUint[3]);
                                }
                                else
                                {
                                    int pixelBaseOOffs = lineBaseOOffs + baseX;

                                    for (int tY = 0; tY < copyHeight; tY++)
                                    {
                                        int line = pixelBaseOOffs + alignedWidth * tY;

                                        for (int tX = 0; tX < copyWidth; tX++)
                                        {
                                            int texel = tY * BlockWidth + tX;

                                            outputAsUshort[line + tX] = (ushort)(rTile[texel] | (gTile[texel] << 8));
                                        }
                                    }
                                }

                                data64 = data64[2..];
                            }
                        }

                        imageBaseOOffs += alignedWidth * height;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);

                alignedWidth = BitUtils.AlignUp(width, 2);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC6(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers, bool signed)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 8;
            }

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);
            Span<byte> outputSpan = output.Span;

            int inputOffset = 0;
            int outputOffset = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        BC6Decoder.Decode(outputSpan[outputOffset..], data[inputOffset..], width, height, signed);

                        inputOffset += w * h * 16;
                        outputOffset += width * height * 8;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);
            }

            return output;
        }

        public static MemoryOwner<byte> DecodeBC7(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 4;
            }

            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(size);
            Span<byte> outputSpan = output.Span;

            int inputOffset = 0;
            int outputOffset = 0;

            for (int l = 0; l < levels; l++)
            {
                int w = BitUtils.DivRoundUp(width, BlockWidth);
                int h = BitUtils.DivRoundUp(height, BlockHeight);

                for (int l2 = 0; l2 < layers; l2++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        BC7Decoder.Decode(outputSpan[outputOffset..], data[inputOffset..], width, height);

                        inputOffset += w * h * 16;
                        outputOffset += width * height * 4;
                    }
                }

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                depth = Math.Max(1, depth >> 1);
            }

            return output;
        }

        private static ulong InterleaveBytes(uint left, uint right)
        {
            return InterleaveBytesWithZeros(left) | (InterleaveBytesWithZeros(right) << 8);
        }

        private static ulong InterleaveBytesWithZeros(uint value)
        {
            ulong output = value;
            output = (output ^ (output << 16)) & 0xffff0000ffffUL;
            output = (output ^ (output << 8)) & 0xff00ff00ff00ffUL;
            return output;
        }

        private static void BCnLerpAlphaUnorm(Span<byte> alpha)
        {
            byte a0 = alpha[0];
            byte a1 = alpha[1];

            if (a0 > a1)
            {
                alpha[2] = (byte)((6 * a0 + 1 * a1) / 7);
                alpha[3] = (byte)((5 * a0 + 2 * a1) / 7);
                alpha[4] = (byte)((4 * a0 + 3 * a1) / 7);
                alpha[5] = (byte)((3 * a0 + 4 * a1) / 7);
                alpha[6] = (byte)((2 * a0 + 5 * a1) / 7);
                alpha[7] = (byte)((1 * a0 + 6 * a1) / 7);
            }
            else
            {
                alpha[2] = (byte)((4 * a0 + 1 * a1) / 5);
                alpha[3] = (byte)((3 * a0 + 2 * a1) / 5);
                alpha[4] = (byte)((2 * a0 + 3 * a1) / 5);
                alpha[5] = (byte)((1 * a0 + 4 * a1) / 5);
                alpha[6] = 0;
                alpha[7] = 0xff;
            }
        }

        private static void BCnLerpAlphaSnorm(Span<byte> alpha)
        {
            sbyte a0 = (sbyte)alpha[0];
            sbyte a1 = (sbyte)alpha[1];

            if (a0 > a1)
            {
                alpha[2] = (byte)((6 * a0 + 1 * a1) / 7);
                alpha[3] = (byte)((5 * a0 + 2 * a1) / 7);
                alpha[4] = (byte)((4 * a0 + 3 * a1) / 7);
                alpha[5] = (byte)((3 * a0 + 4 * a1) / 7);
                alpha[6] = (byte)((2 * a0 + 5 * a1) / 7);
                alpha[7] = (byte)((1 * a0 + 6 * a1) / 7);
            }
            else
            {
                alpha[2] = (byte)((4 * a0 + 1 * a1) / 5);
                alpha[3] = (byte)((3 * a0 + 2 * a1) / 5);
                alpha[4] = (byte)((2 * a0 + 3 * a1) / 5);
                alpha[5] = (byte)((1 * a0 + 4 * a1) / 5);
                alpha[6] = 0x80;
                alpha[7] = 0x7f;
            }
        }

        private unsafe static void BCnDecodeTileAlpha(Span<byte> output, Span<byte> rPal, ulong rI)
        {
            if (Avx2.IsSupported)
            {
                Span<Vector128<byte>> outputAsVector128 = MemoryMarshal.Cast<byte, Vector128<byte>>(output);

                Vector128<uint> shifts = Vector128.Create(0u, 3u, 6u, 9u);
                Vector128<uint> masks = Vector128.Create(7u);

                Vector128<byte> vClut;

                fixed (byte* pRPal = rPal)
                {
                    vClut = Sse2.LoadScalarVector128((ulong*)pRPal).AsByte();
                }

                Vector128<uint> indices0 = Vector128.Create((uint)rI);
                Vector128<uint> indices1 = Vector128.Create((uint)(rI >> 24));
                Vector128<uint> indices00 = Avx2.ShiftRightLogicalVariable(indices0, shifts);
                Vector128<uint> indices10 = Avx2.ShiftRightLogicalVariable(indices1, shifts);
                Vector128<uint> indices01 = Sse2.ShiftRightLogical(indices00, 12);
                Vector128<uint> indices11 = Sse2.ShiftRightLogical(indices10, 12);
                indices00 = Sse2.And(indices00, masks);
                indices10 = Sse2.And(indices10, masks);
                indices01 = Sse2.And(indices01, masks);
                indices11 = Sse2.And(indices11, masks);

                Vector128<ushort> indicesW0 = Sse41.PackUnsignedSaturate(indices00.AsInt32(), indices01.AsInt32());
                Vector128<ushort> indicesW1 = Sse41.PackUnsignedSaturate(indices10.AsInt32(), indices11.AsInt32());

                Vector128<byte> indices = Sse2.PackUnsignedSaturate(indicesW0.AsInt16(), indicesW1.AsInt16());

                outputAsVector128[0] = Ssse3.Shuffle(vClut, indices);
            }
            else
            {
                for (int i = 0; i < BlockWidth * BlockHeight; i++, rI >>= 3)
                {
                    output[i] = rPal[(int)(rI & 7)];
                }
            }
        }

        private unsafe static void BCnDecodeTileAlphaRgba(Span<byte> output, Span<byte> rPal, ulong rI)
        {
            if (Avx2.IsSupported)
            {
                Span<Vector256<uint>> outputAsVector256 = MemoryMarshal.Cast<byte, Vector256<uint>>(output);

                Vector256<uint> shifts = Vector256.Create(0u, 3u, 6u, 9u, 12u, 15u, 18u, 21u);

                Vector128<uint> vClut128;

                fixed (byte* pRPal = rPal)
                {
                    vClut128 = Sse2.LoadScalarVector128((ulong*)pRPal).AsUInt32();
                }

                Vector256<uint> vClut = Avx2.ConvertToVector256Int32(vClut128.AsByte()).AsUInt32();
                vClut = Avx2.ShiftLeftLogical(vClut, 24);

                Vector256<uint> indices0 = Vector256.Create((uint)rI);
                Vector256<uint> indices1 = Vector256.Create((uint)(rI >> 24));

                indices0 = Avx2.ShiftRightLogicalVariable(indices0, shifts);
                indices1 = Avx2.ShiftRightLogicalVariable(indices1, shifts);

                outputAsVector256[0] = Avx2.Or(outputAsVector256[0], Avx2.PermuteVar8x32(vClut, indices0));
                outputAsVector256[1] = Avx2.Or(outputAsVector256[1], Avx2.PermuteVar8x32(vClut, indices1));
            }
            else
            {
                for (int i = 3; i < BlockWidth * BlockHeight * 4; i += 4, rI >>= 3)
                {
                    output[i] = rPal[(int)(rI & 7)];
                }
            }
        }

        private unsafe static void BC1DecodeTileRgb(Span<byte> output, ReadOnlySpan<byte> input)
        {
            Span<uint> clut = stackalloc uint[4];

            uint c0c1 = BinaryPrimitives.ReadUInt32LittleEndian(input);
            uint c0 = (ushort)c0c1;
            uint c1 = (ushort)(c0c1 >> 16);

            clut[0] = ConvertRgb565ToRgb888(c0) | 0xff000000;
            clut[1] = ConvertRgb565ToRgb888(c1) | 0xff000000;
            clut[2] = BC1LerpRgb2(clut[0], clut[1], c0, c1);
            clut[3] = BC1LerpRgb3(clut[0], clut[1], c0, c1);

            BCnDecodeTileRgb(clut, output, input);
        }

        private unsafe static void BC23DecodeTileRgb(Span<byte> output, ReadOnlySpan<byte> input)
        {
            Span<uint> clut = stackalloc uint[4];

            uint c0c1 = BinaryPrimitives.ReadUInt32LittleEndian(input);
            uint c0 = (ushort)c0c1;
            uint c1 = (ushort)(c0c1 >> 16);

            clut[0] = ConvertRgb565ToRgb888(c0);
            clut[1] = ConvertRgb565ToRgb888(c1);
            clut[2] = BC23LerpRgb2(clut[0], clut[1]);
            clut[3] = BC23LerpRgb3(clut[0], clut[1]);

            BCnDecodeTileRgb(clut, output, input);
        }

        private unsafe static void BCnDecodeTileRgb(Span<uint> clut, Span<byte> output, ReadOnlySpan<byte> input)
        {
            if (Avx2.IsSupported)
            {
                Span<Vector256<uint>> outputAsVector256 = MemoryMarshal.Cast<byte, Vector256<uint>>(output);

                Vector256<uint> shifts0 = Vector256.Create(0u, 2u, 4u, 6u, 8u, 10u, 12u, 14u);
                Vector256<uint> shifts1 = Vector256.Create(16u, 18u, 20u, 22u, 24u, 26u, 28u, 30u);
                Vector256<uint> masks = Vector256.Create(3u);

                Vector256<uint> vClut;

                fixed (uint* pClut = &clut[0])
                {
                    vClut = Sse2.LoadVector128(pClut).ToVector256Unsafe();
                }

                Vector256<uint> indices0;

                fixed (byte* pInput = input)
                {
                    indices0 = Avx2.BroadcastScalarToVector256((uint*)(pInput + 4));
                }

                Vector256<uint> indices1 = indices0;

                indices0 = Avx2.ShiftRightLogicalVariable(indices0, shifts0);
                indices1 = Avx2.ShiftRightLogicalVariable(indices1, shifts1);
                indices0 = Avx2.And(indices0, masks);
                indices1 = Avx2.And(indices1, masks);

                outputAsVector256[0] = Avx2.PermuteVar8x32(vClut, indices0);
                outputAsVector256[1] = Avx2.PermuteVar8x32(vClut, indices1);
            }
            else
            {
                Span<uint> outputAsUint = MemoryMarshal.Cast<byte, uint>(output);

                uint indices = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);

                for (int i = 0; i < BlockWidth * BlockHeight; i++, indices >>= 2)
                {
                    outputAsUint[i] = clut[(int)(indices & 3)];
                }
            }
        }

        private static uint BC1LerpRgb2(uint color0, uint color1, uint c0, uint c1)
        {
            if (c0 > c1)
            {
                return BC23LerpRgb2(color0, color1) | 0xff000000;
            }

            uint carry = color0 & color1;
            uint addHalve = ((color0 ^ color1) >> 1) & 0x7f7f7f;
            return (addHalve + carry) | 0xff000000;
        }

        private static uint BC23LerpRgb2(uint color0, uint color1)
        {
            uint r0 = (byte)color0;
            uint g0 = color0 & 0xff00;
            uint b0 = color0 & 0xff0000;

            uint r1 = (byte)color1;
            uint g1 = color1 & 0xff00;
            uint b1 = color1 & 0xff0000;

            uint mixR = (2 * r0 + r1) / 3;
            uint mixG = (2 * g0 + g1) / 3;
            uint mixB = (2 * b0 + b1) / 3;

            return mixR | (mixG & 0xff00) | (mixB & 0xff0000);
        }

        private static uint BC1LerpRgb3(uint color0, uint color1, uint c0, uint c1)
        {
            if (c0 > c1)
            {
                return BC23LerpRgb3(color0, color1) | 0xff000000;
            }

            return 0;
        }

        private static uint BC23LerpRgb3(uint color0, uint color1)
        {
            uint r0 = (byte)color0;
            uint g0 = color0 & 0xff00;
            uint b0 = color0 & 0xff0000;

            uint r1 = (byte)color1;
            uint g1 = color1 & 0xff00;
            uint b1 = color1 & 0xff0000;

            uint mixR = (2 * r1 + r0) / 3;
            uint mixG = (2 * g1 + g0) / 3;
            uint mixB = (2 * b1 + b0) / 3;

            return mixR | (mixG & 0xff00) | (mixB & 0xff0000);
        }

        private static uint ConvertRgb565ToRgb888(uint value)
        {
            uint b = (value & 0x1f) << 19;
            uint g = (value << 5) & 0xfc00;
            uint r = (value >> 8) & 0xf8;

            b |= b >> 5;
            g |= g >> 6;
            r |= r >> 5;

            return r | (g & 0xff00) | (b & 0xff0000);
        }
    }
}
