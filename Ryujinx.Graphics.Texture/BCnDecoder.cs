using Ryujinx.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture
{
    public static class BCnDecoder
    {
        private const int BlockWidth = 4;
        private const int BlockHeight = 4;

        public static byte[] DecodeBC4(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers, bool signed)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers;
            }

            byte[] output = new byte[size];

            ReadOnlySpan<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);

            Span<byte> rPal = stackalloc byte[8];

            int baseOOffs = 0;

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

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int lineBaseOOffs = baseOOffs + baseX;

                                ulong block = data64[0];

                                rPal[0] = (byte)block;
                                rPal[1] = (byte)(block >> 8);

                                if (signed)
                                {
                                    CalculateBC3AlphaS(rPal);
                                }
                                else
                                {
                                    CalculateBC3Alpha(rPal);
                                }

                                ulong rI = block >> 16;

                                for (int texel = 0; texel < BlockWidth * BlockHeight; texel++)
                                {
                                    int tX = texel & 3;
                                    int tY = texel >> 2;

                                    if (baseX + tX >= width || baseY + tY >= height)
                                    {
                                        continue;
                                    }

                                    int shift = texel * 3;

                                    byte r = rPal[(int)((rI >> shift) & 7)];

                                    int oOffs = lineBaseOOffs + tY * width + tX;

                                    output[oOffs] = r;
                                }

                                data64 = data64.Slice(1);
                            }

                            baseOOffs += width * (baseY + BlockHeight > height ? (height & (BlockHeight - 1)) : BlockHeight);
                        }
                    }
                }

                width  = Math.Max(1, width  >> 1);
                height = Math.Max(1, height >> 1);
                depth  = Math.Max(1, depth  >> 1);
            }

            return output;
        }

        public static byte[] DecodeBC5(ReadOnlySpan<byte> data, int width, int height, int depth, int levels, int layers, bool signed)
        {
            int size = 0;

            for (int l = 0; l < levels; l++)
            {
                size += Math.Max(1, width >> l) * Math.Max(1, height >> l) * Math.Max(1, depth >> l) * layers * 2;
            }

            byte[] output = new byte[size];

            ReadOnlySpan<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);

            Span<byte> rPal = stackalloc byte[8];
            Span<byte> gPal = stackalloc byte[8];

            int baseOOffs = 0;

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

                            for (int x = 0; x < w; x++)
                            {
                                int baseX = x * BlockWidth;
                                int lineBaseOOffs = baseOOffs + baseX;

                                ulong blockL = data64[0];
                                ulong blockH = data64[1];

                                rPal[0] = (byte)blockL;
                                rPal[1] = (byte)(blockL >> 8);
                                gPal[0] = (byte)blockH;
                                gPal[1] = (byte)(blockH >> 8);

                                if (signed)
                                {
                                    CalculateBC3AlphaS(rPal);
                                    CalculateBC3AlphaS(gPal);
                                }
                                else
                                {
                                    CalculateBC3Alpha(rPal);
                                    CalculateBC3Alpha(gPal);
                                }

                                ulong rI = blockL >> 16;
                                ulong gI = blockH >> 16;

                                for (int texel = 0; texel < BlockWidth * BlockHeight; texel++)
                                {
                                    int tX = texel & 3;
                                    int tY = texel >> 2;

                                    if (baseX + tX >= width || baseY + tY >= height)
                                    {
                                        continue;
                                    }

                                    int shift = texel * 3;

                                    byte r = rPal[(int)((rI >> shift) & 7)];
                                    byte g = gPal[(int)((gI >> shift) & 7)];

                                    int oOffs = (lineBaseOOffs + tY * width + tX) * 2;

                                    output[oOffs + 0] = r;
                                    output[oOffs + 1] = g;
                                }

                                data64 = data64.Slice(2);
                            }

                            baseOOffs += width * (baseY + BlockHeight > height ? (height & (BlockHeight - 1)) : BlockHeight);
                        }
                    }
                }

                width  = Math.Max(1, width  >> 1);
                height = Math.Max(1, height >> 1);
                depth  = Math.Max(1, depth  >> 1);
            }

            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CalculateBC3Alpha(Span<byte> alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if (alpha[0] > alpha[1])
                {
                    alpha[i] = (byte)(((8 - i) * alpha[0] + (i - 1) * alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    alpha[i] = (byte)(((6 - i) * alpha[0] + (i - 1) * alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    alpha[i] = 0;
                }
                else /* i == 7 */
                {
                    alpha[i] = 0xff;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CalculateBC3AlphaS(Span<byte> alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if ((sbyte)alpha[0] > (sbyte)alpha[1])
                {
                    alpha[i] = (byte)(((8 - i) * (sbyte)alpha[0] + (i - 1) * (sbyte)alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    alpha[i] = (byte)(((6 - i) * (sbyte)alpha[0] + (i - 1) * (sbyte)alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    alpha[i] = 0x80;
                }
                else /* i == 7 */
                {
                    alpha[i] = 0x7f;
                }
            }
        }
    }
}