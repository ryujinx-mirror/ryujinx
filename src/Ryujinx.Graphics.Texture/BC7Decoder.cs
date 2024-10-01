using Ryujinx.Graphics.Texture.Utils;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture
{
    static class BC7Decoder
    {
        public static void Decode(Span<byte> output, ReadOnlySpan<byte> data, int width, int height)
        {
            ReadOnlySpan<Block> blocks = MemoryMarshal.Cast<byte, Block>(data);

            Span<uint> output32 = MemoryMarshal.Cast<byte, uint>(output);

            int wInBlocks = (width + 3) / 4;
            int hInBlocks = (height + 3) / 4;

            for (int y = 0; y < hInBlocks; y++)
            {
                int y2 = y * 4;
                int bh = Math.Min(4, height - y2);

                for (int x = 0; x < wInBlocks; x++)
                {
                    int x2 = x * 4;
                    int bw = Math.Min(4, width - x2);

                    DecodeBlock(blocks[y * wInBlocks + x], output32[(y2 * width + x2)..], bw, bh, width);
                }
            }
        }

        private static void DecodeBlock(Block block, Span<uint> output, int w, int h, int width)
        {
            int mode = BitOperations.TrailingZeroCount((byte)block.Low | 0x100);
            if (mode == 8)
            {
                // Mode is invalid, the spec mandates that hardware fills the block with
                // a transparent black color.
                for (int ty = 0; ty < h; ty++)
                {
                    int baseOffs = ty * width;

                    for (int tx = 0; tx < w; tx++)
                    {
                        int offs = baseOffs + tx;

                        output[offs] = 0;
                    }
                }

                return;
            }

            BC7ModeInfo modeInfo = BC67Tables.BC7ModeInfos[mode];

            int offset = mode + 1;
            int partition = (int)block.Decode(ref offset, modeInfo.PartitionBitCount);
            int rotation = (int)block.Decode(ref offset, modeInfo.RotationBitCount);
            int indexMode = (int)block.Decode(ref offset, modeInfo.IndexModeBitCount);

            Debug.Assert(partition < 64);
            Debug.Assert(rotation < 4);
            Debug.Assert(indexMode < 2);

            int endPointCount = modeInfo.SubsetCount * 2;

            Span<RgbaColor32> endPoints = stackalloc RgbaColor32[endPointCount];
            Span<byte> pValues = stackalloc byte[modeInfo.PBits];

            endPoints.Fill(new RgbaColor32(0, 0, 0, 255));

            for (int i = 0; i < endPointCount; i++)
            {
                endPoints[i].R = (int)block.Decode(ref offset, modeInfo.ColorDepth);
            }

            for (int i = 0; i < endPointCount; i++)
            {
                endPoints[i].G = (int)block.Decode(ref offset, modeInfo.ColorDepth);
            }

            for (int i = 0; i < endPointCount; i++)
            {
                endPoints[i].B = (int)block.Decode(ref offset, modeInfo.ColorDepth);
            }

            if (modeInfo.AlphaDepth != 0)
            {
                for (int i = 0; i < endPointCount; i++)
                {
                    endPoints[i].A = (int)block.Decode(ref offset, modeInfo.AlphaDepth);
                }
            }

            for (int i = 0; i < modeInfo.PBits; i++)
            {
                pValues[i] = (byte)block.Decode(ref offset, 1);
            }

            for (int i = 0; i < endPointCount; i++)
            {
                int pBit = -1;

                if (modeInfo.PBits != 0)
                {
                    int pIndex = (i * modeInfo.PBits) / endPointCount;
                    pBit = pValues[pIndex];
                }

                Unquantize(ref endPoints[i], modeInfo.ColorDepth, modeInfo.AlphaDepth, pBit);
            }

            byte[] partitionTable = BC67Tables.PartitionTable[modeInfo.SubsetCount - 1][partition];
            byte[] fixUpTable = BC67Tables.FixUpIndices[modeInfo.SubsetCount - 1][partition];

            Span<byte> colorIndices = stackalloc byte[16];

            for (int i = 0; i < 16; i++)
            {
                byte subset = partitionTable[i];
                int bitCount = i == fixUpTable[subset] ? modeInfo.ColorIndexBitCount - 1 : modeInfo.ColorIndexBitCount;

                colorIndices[i] = (byte)block.Decode(ref offset, bitCount);
                Debug.Assert(colorIndices[i] < 16);
            }

            Span<byte> alphaIndices = stackalloc byte[16];

            if (modeInfo.AlphaIndexBitCount != 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    int bitCount = i != 0 ? modeInfo.AlphaIndexBitCount : modeInfo.AlphaIndexBitCount - 1;

                    alphaIndices[i] = (byte)block.Decode(ref offset, bitCount);
                    Debug.Assert(alphaIndices[i] < 16);
                }
            }

            for (int ty = 0; ty < h; ty++)
            {
                int baseOffs = ty * width;

                for (int tx = 0; tx < w; tx++)
                {
                    int i = ty * 4 + tx;

                    RgbaColor32 color;

                    byte subset = partitionTable[i];

                    RgbaColor32 color1 = endPoints[subset * 2];
                    RgbaColor32 color2 = endPoints[subset * 2 + 1];

                    if (modeInfo.AlphaIndexBitCount != 0)
                    {
                        if (indexMode == 0)
                        {
                            color = BC67Utils.Interpolate(color1, color2, colorIndices[i], alphaIndices[i], modeInfo.ColorIndexBitCount, modeInfo.AlphaIndexBitCount);
                        }
                        else
                        {
                            color = BC67Utils.Interpolate(color1, color2, alphaIndices[i], colorIndices[i], modeInfo.AlphaIndexBitCount, modeInfo.ColorIndexBitCount);
                        }
                    }
                    else
                    {
                        color = BC67Utils.Interpolate(color1, color2, colorIndices[i], colorIndices[i], modeInfo.ColorIndexBitCount, modeInfo.ColorIndexBitCount);
                    }

                    if (rotation != 0)
                    {
                        int a = color.A;

                        switch (rotation)
                        {
                            case 1:
                                color.A = color.R;
                                color.R = a;
                                break;
                            case 2:
                                color.A = color.G;
                                color.G = a;
                                break;
                            case 3:
                                color.A = color.B;
                                color.B = a;
                                break;
                        }
                    }

                    RgbaColor8 color8 = color.GetColor8();

                    output[baseOffs + tx] = color8.ToUInt32();
                }
            }
        }

        private static void Unquantize(ref RgbaColor32 color, int colorDepth, int alphaDepth, int pBit)
        {
            color.R = UnquantizeComponent(color.R, colorDepth, pBit);
            color.G = UnquantizeComponent(color.G, colorDepth, pBit);
            color.B = UnquantizeComponent(color.B, colorDepth, pBit);
            color.A = alphaDepth != 0 ? UnquantizeComponent(color.A, alphaDepth, pBit) : 255;
        }

        private static int UnquantizeComponent(int component, int bits, int pBit)
        {
            int shift = 8 - bits;
            int value = component << shift;

            if (pBit >= 0)
            {
                Debug.Assert(pBit <= 1);
                value |= value >> (bits + 1);
                value |= pBit << (shift - 1);
            }
            else
            {
                value |= value >> bits;
            }

            return value;
        }
    }
}
