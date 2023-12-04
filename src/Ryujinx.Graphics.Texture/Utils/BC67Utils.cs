using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Texture.Utils
{
    static class BC67Utils
    {
        private static readonly byte[][] _quantizationLut;
        private static readonly byte[][] _quantizationLutNoPBit;

        static BC67Utils()
        {
            _quantizationLut = new byte[5][];
            _quantizationLutNoPBit = new byte[5][];

            for (int depth = 4; depth < 9; depth++)
            {
                byte[] lut = new byte[512];
                byte[] lutNoPBit = new byte[256];

                for (int i = 0; i < lut.Length; i++)
                {
                    lut[i] = QuantizeComponentForLut((byte)i, depth, i >> 8);

                    if (i < lutNoPBit.Length)
                    {
                        lutNoPBit[i] = QuantizeComponentForLut((byte)i, depth);
                    }
                }

                _quantizationLut[depth - 4] = lut;
                _quantizationLutNoPBit[depth - 4] = lutNoPBit;
            }
        }

        public static (RgbaColor8, RgbaColor8) GetMinMaxColors(ReadOnlySpan<uint> tile, int w, int h)
        {
            if (Sse41.IsSupported && w == 4 && h == 4)
            {
                GetMinMaxColorsOneSubset4x4Sse41(tile, out RgbaColor8 minColor, out RgbaColor8 maxColor);

                return (minColor, maxColor);
            }
            else
            {
                RgbaColor8 minColor = new(255, 255, 255, 255);
                RgbaColor8 maxColor = default;

                for (int i = 0; i < tile.Length; i++)
                {
                    RgbaColor8 color = RgbaColor8.FromUInt32(tile[i]);

                    minColor.R = Math.Min(minColor.R, color.R);
                    minColor.G = Math.Min(minColor.G, color.G);
                    minColor.B = Math.Min(minColor.B, color.B);
                    minColor.A = Math.Min(minColor.A, color.A);

                    maxColor.R = Math.Max(maxColor.R, color.R);
                    maxColor.G = Math.Max(maxColor.G, color.G);
                    maxColor.B = Math.Max(maxColor.B, color.B);
                    maxColor.A = Math.Max(maxColor.A, color.A);
                }

                return (minColor, maxColor);
            }
        }

        public static void GetMinMaxColors(
            ReadOnlySpan<byte> partitionTable,
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            Span<RgbaColor8> minColors,
            Span<RgbaColor8> maxColors,
            int subsetCount)
        {
            if (Sse41.IsSupported && w == 4 && h == 4)
            {
                if (subsetCount == 1)
                {
                    GetMinMaxColorsOneSubset4x4Sse41(tile, out minColors[0], out maxColors[0]);
                    return;
                }
                else if (subsetCount == 2)
                {
                    GetMinMaxColorsTwoSubsets4x4Sse41(partitionTable, tile, minColors, maxColors);
                    return;
                }
            }

            minColors.Fill(new RgbaColor8(255, 255, 255, 255));

            int i = 0;
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++)
                {
                    int subset = partitionTable[ty * w + tx];
                    RgbaColor8 color = RgbaColor8.FromUInt32(tile[i++]);

                    minColors[subset].R = Math.Min(minColors[subset].R, color.R);
                    minColors[subset].G = Math.Min(minColors[subset].G, color.G);
                    minColors[subset].B = Math.Min(minColors[subset].B, color.B);
                    minColors[subset].A = Math.Min(minColors[subset].A, color.A);

                    maxColors[subset].R = Math.Max(maxColors[subset].R, color.R);
                    maxColors[subset].G = Math.Max(maxColors[subset].G, color.G);
                    maxColors[subset].B = Math.Max(maxColors[subset].B, color.B);
                    maxColors[subset].A = Math.Max(maxColors[subset].A, color.A);
                }
            }
        }

        private static unsafe void GetMinMaxColorsOneSubset4x4Sse41(ReadOnlySpan<uint> tile, out RgbaColor8 minColor, out RgbaColor8 maxColor)
        {
            Vector128<byte> min = Vector128<byte>.AllBitsSet;
            Vector128<byte> max = Vector128<byte>.Zero;
            Vector128<byte> row0, row1, row2, row3;

            fixed (uint* pTile = tile)
            {
                row0 = Sse2.LoadVector128(pTile).AsByte();
                row1 = Sse2.LoadVector128(pTile + 4).AsByte();
                row2 = Sse2.LoadVector128(pTile + 8).AsByte();
                row3 = Sse2.LoadVector128(pTile + 12).AsByte();
            }

            min = Sse2.Min(min, row0);
            max = Sse2.Max(max, row0);
            min = Sse2.Min(min, row1);
            max = Sse2.Max(max, row1);
            min = Sse2.Min(min, row2);
            max = Sse2.Max(max, row2);
            min = Sse2.Min(min, row3);
            max = Sse2.Max(max, row3);

            minColor = HorizontalMin(min);
            maxColor = HorizontalMax(max);
        }

        private static unsafe void GetMinMaxColorsTwoSubsets4x4Sse41(
            ReadOnlySpan<byte> partitionTable,
            ReadOnlySpan<uint> tile,
            Span<RgbaColor8> minColors,
            Span<RgbaColor8> maxColors)
        {
            Vector128<byte> partitionMask;

            fixed (byte* pPartitionTable = partitionTable)
            {
                partitionMask = Sse2.LoadVector128(pPartitionTable);
            }

            Vector128<byte> subset0Mask = Sse2.CompareEqual(partitionMask, Vector128<byte>.Zero);

            Vector128<byte> subset0MaskRep16Low = Sse2.UnpackLow(subset0Mask, subset0Mask);
            Vector128<byte> subset0MaskRep16High = Sse2.UnpackHigh(subset0Mask, subset0Mask);

            Vector128<byte> subset0Mask0 = Sse2.UnpackLow(subset0MaskRep16Low.AsInt16(), subset0MaskRep16Low.AsInt16()).AsByte();
            Vector128<byte> subset0Mask1 = Sse2.UnpackHigh(subset0MaskRep16Low.AsInt16(), subset0MaskRep16Low.AsInt16()).AsByte();
            Vector128<byte> subset0Mask2 = Sse2.UnpackLow(subset0MaskRep16High.AsInt16(), subset0MaskRep16High.AsInt16()).AsByte();
            Vector128<byte> subset0Mask3 = Sse2.UnpackHigh(subset0MaskRep16High.AsInt16(), subset0MaskRep16High.AsInt16()).AsByte();

            Vector128<byte> min0 = Vector128<byte>.AllBitsSet;
            Vector128<byte> min1 = Vector128<byte>.AllBitsSet;
            Vector128<byte> max0 = Vector128<byte>.Zero;
            Vector128<byte> max1 = Vector128<byte>.Zero;

            Vector128<byte> row0, row1, row2, row3;

            fixed (uint* pTile = tile)
            {
                row0 = Sse2.LoadVector128(pTile).AsByte();
                row1 = Sse2.LoadVector128(pTile + 4).AsByte();
                row2 = Sse2.LoadVector128(pTile + 8).AsByte();
                row3 = Sse2.LoadVector128(pTile + 12).AsByte();
            }

            min0 = Sse2.Min(min0, Sse41.BlendVariable(min0, row0, subset0Mask0));
            min0 = Sse2.Min(min0, Sse41.BlendVariable(min0, row1, subset0Mask1));
            min0 = Sse2.Min(min0, Sse41.BlendVariable(min0, row2, subset0Mask2));
            min0 = Sse2.Min(min0, Sse41.BlendVariable(min0, row3, subset0Mask3));

            min1 = Sse2.Min(min1, Sse2.Or(row0, subset0Mask0));
            min1 = Sse2.Min(min1, Sse2.Or(row1, subset0Mask1));
            min1 = Sse2.Min(min1, Sse2.Or(row2, subset0Mask2));
            min1 = Sse2.Min(min1, Sse2.Or(row3, subset0Mask3));

            max0 = Sse2.Max(max0, Sse2.And(row0, subset0Mask0));
            max0 = Sse2.Max(max0, Sse2.And(row1, subset0Mask1));
            max0 = Sse2.Max(max0, Sse2.And(row2, subset0Mask2));
            max0 = Sse2.Max(max0, Sse2.And(row3, subset0Mask3));

            max1 = Sse2.Max(max1, Sse2.AndNot(subset0Mask0, row0));
            max1 = Sse2.Max(max1, Sse2.AndNot(subset0Mask1, row1));
            max1 = Sse2.Max(max1, Sse2.AndNot(subset0Mask2, row2));
            max1 = Sse2.Max(max1, Sse2.AndNot(subset0Mask3, row3));

            minColors[0] = HorizontalMin(min0);
            minColors[1] = HorizontalMin(min1);
            maxColors[0] = HorizontalMax(max0);
            maxColors[1] = HorizontalMax(max1);
        }

        private static RgbaColor8 HorizontalMin(Vector128<byte> x)
        {
            x = Sse2.Min(x, Sse2.Shuffle(x.AsInt32(), 0x31).AsByte());
            x = Sse2.Min(x, Sse2.Shuffle(x.AsInt32(), 2).AsByte());
            return RgbaColor8.FromUInt32(x.AsUInt32().GetElement(0));
        }

        private static RgbaColor8 HorizontalMax(Vector128<byte> x)
        {
            x = Sse2.Max(x, Sse2.Shuffle(x.AsInt32(), 0x31).AsByte());
            x = Sse2.Max(x, Sse2.Shuffle(x.AsInt32(), 2).AsByte());
            return RgbaColor8.FromUInt32(x.AsUInt32().GetElement(0));
        }

        public static int SelectIndices(
            ReadOnlySpan<uint> values,
            uint endPoint0,
            uint endPoint1,
            int pBit0,
            int pBit1,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            uint alphaMask)
        {
            if (Sse41.IsSupported)
            {
                if (indexBitCount == 2)
                {
                    return Select2BitIndicesSse41(
                        values,
                        endPoint0,
                        endPoint1,
                        pBit0,
                        pBit1,
                        indexBitCount,
                        indexCount,
                        colorDepth,
                        alphaDepth,
                        alphaMask);
                }
                else if (indexBitCount == 3)
                {
                    return Select3BitIndicesSse41(
                        values,
                        endPoint0,
                        endPoint1,
                        pBit0,
                        pBit1,
                        indexBitCount,
                        indexCount,
                        colorDepth,
                        alphaDepth,
                        alphaMask);
                }
                else if (indexBitCount == 4)
                {
                    return Select4BitIndicesOneSubsetSse41(
                        values,
                        endPoint0,
                        endPoint1,
                        pBit0,
                        pBit1,
                        indexBitCount,
                        indexCount,
                        colorDepth,
                        alphaDepth,
                        alphaMask);
                }
            }

            return SelectIndicesFallback(
                values,
                endPoint0,
                endPoint1,
                pBit0,
                pBit1,
                indexBitCount,
                indexCount,
                colorDepth,
                alphaDepth,
                alphaMask);
        }

        private static unsafe int Select2BitIndicesSse41(
            ReadOnlySpan<uint> values,
            uint endPoint0,
            uint endPoint1,
            int pBit0,
            int pBit1,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            uint alphaMask)
        {
            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoint0), colorDepth, alphaDepth, pBit0);
            RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoint1), colorDepth, alphaDepth, pBit1);

            Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
            Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

            Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

            Vector128<byte> rWeights;
            Vector128<byte> lWeights;

            fixed (byte* pWeights = BC67Tables.Weights[0], pInvWeights = BC67Tables.InverseWeights[0])
            {
                rWeights = Sse2.LoadScalarVector128((uint*)pWeights).AsByte();
                lWeights = Sse2.LoadScalarVector128((uint*)pInvWeights).AsByte();
            }

            Vector128<byte> iWeights = Sse2.UnpackLow(lWeights, rWeights);
            Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
            Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();

            Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
            Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));

            for (int i = 0; i < values.Length; i++)
            {
                uint c = values[i] | alphaMask;

                Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                Vector128<short> delta1 = Sse2.Subtract(color, pal1);

                Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);

                Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);

                Vector128<ushort> delta = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum01);

                Vector128<ushort> min = Sse41.MinHorizontal(delta);

                ushort error = min.GetElement(0);

                errorSum += error;
            }

            return errorSum;
        }

        private static unsafe int Select3BitIndicesSse41(
            ReadOnlySpan<uint> values,
            uint endPoint0,
            uint endPoint1,
            int pBit0,
            int pBit1,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            uint alphaMask)
        {
            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoint0), colorDepth, alphaDepth, pBit0);
            RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoint1), colorDepth, alphaDepth, pBit1);

            Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
            Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

            Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

            Vector128<byte> rWeights;
            Vector128<byte> lWeights;

            fixed (byte* pWeights = BC67Tables.Weights[1], pInvWeights = BC67Tables.InverseWeights[1])
            {
                rWeights = Sse2.LoadScalarVector128((ulong*)pWeights).AsByte();
                lWeights = Sse2.LoadScalarVector128((ulong*)pInvWeights).AsByte();
            }

            Vector128<byte> iWeights = Sse2.UnpackLow(lWeights, rWeights);
            Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
            Vector128<byte> iWeights23 = Sse2.UnpackHigh(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
            Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights2 = Sse2.UnpackLow(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
            Vector128<byte> iWeights3 = Sse2.UnpackHigh(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();

            Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
            Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));
            Vector128<short> pal2 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights2.AsSByte()));
            Vector128<short> pal3 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights3.AsSByte()));

            for (int i = 0; i < values.Length; i++)
            {
                uint c = values[i] | alphaMask;

                Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                Vector128<short> delta1 = Sse2.Subtract(color, pal1);
                Vector128<short> delta2 = Sse2.Subtract(color, pal2);
                Vector128<short> delta3 = Sse2.Subtract(color, pal3);

                Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);
                Vector128<int> deltaSum2 = Sse2.MultiplyAddAdjacent(delta2, delta2);
                Vector128<int> deltaSum3 = Sse2.MultiplyAddAdjacent(delta3, delta3);

                Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);
                Vector128<int> deltaSum23 = Ssse3.HorizontalAdd(deltaSum2, deltaSum3);

                Vector128<ushort> delta = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum23);

                Vector128<ushort> min = Sse41.MinHorizontal(delta);

                ushort error = min.GetElement(0);

                errorSum += error;
            }

            return errorSum;
        }

        private static unsafe int Select4BitIndicesOneSubsetSse41(
            ReadOnlySpan<uint> values,
            uint endPoint0,
            uint endPoint1,
            int pBit0,
            int pBit1,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            uint alphaMask)
        {
            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoint0), colorDepth, alphaDepth, pBit0);
            RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoint1), colorDepth, alphaDepth, pBit1);

            Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
            Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

            Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

            Vector128<byte> rWeights;
            Vector128<byte> lWeights;

            fixed (byte* pWeights = BC67Tables.Weights[2], pInvWeights = BC67Tables.InverseWeights[2])
            {
                rWeights = Sse2.LoadVector128(pWeights);
                lWeights = Sse2.LoadVector128(pInvWeights);
            }

            Vector128<byte> iWeightsLow = Sse2.UnpackLow(lWeights, rWeights);
            Vector128<byte> iWeightsHigh = Sse2.UnpackHigh(lWeights, rWeights);
            Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeightsLow.AsInt16(), iWeightsLow.AsInt16()).AsByte();
            Vector128<byte> iWeights23 = Sse2.UnpackHigh(iWeightsLow.AsInt16(), iWeightsLow.AsInt16()).AsByte();
            Vector128<byte> iWeights45 = Sse2.UnpackLow(iWeightsHigh.AsInt16(), iWeightsHigh.AsInt16()).AsByte();
            Vector128<byte> iWeights67 = Sse2.UnpackHigh(iWeightsHigh.AsInt16(), iWeightsHigh.AsInt16()).AsByte();
            Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights2 = Sse2.UnpackLow(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
            Vector128<byte> iWeights3 = Sse2.UnpackHigh(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
            Vector128<byte> iWeights4 = Sse2.UnpackLow(iWeights45.AsInt16(), iWeights45.AsInt16()).AsByte();
            Vector128<byte> iWeights5 = Sse2.UnpackHigh(iWeights45.AsInt16(), iWeights45.AsInt16()).AsByte();
            Vector128<byte> iWeights6 = Sse2.UnpackLow(iWeights67.AsInt16(), iWeights67.AsInt16()).AsByte();
            Vector128<byte> iWeights7 = Sse2.UnpackHigh(iWeights67.AsInt16(), iWeights67.AsInt16()).AsByte();

            Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
            Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));
            Vector128<short> pal2 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights2.AsSByte()));
            Vector128<short> pal3 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights3.AsSByte()));
            Vector128<short> pal4 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights4.AsSByte()));
            Vector128<short> pal5 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights5.AsSByte()));
            Vector128<short> pal6 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights6.AsSByte()));
            Vector128<short> pal7 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights7.AsSByte()));

            for (int i = 0; i < values.Length; i++)
            {
                uint c = values[i] | alphaMask;

                Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                Vector128<short> delta1 = Sse2.Subtract(color, pal1);
                Vector128<short> delta2 = Sse2.Subtract(color, pal2);
                Vector128<short> delta3 = Sse2.Subtract(color, pal3);
                Vector128<short> delta4 = Sse2.Subtract(color, pal4);
                Vector128<short> delta5 = Sse2.Subtract(color, pal5);
                Vector128<short> delta6 = Sse2.Subtract(color, pal6);
                Vector128<short> delta7 = Sse2.Subtract(color, pal7);

                Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);
                Vector128<int> deltaSum2 = Sse2.MultiplyAddAdjacent(delta2, delta2);
                Vector128<int> deltaSum3 = Sse2.MultiplyAddAdjacent(delta3, delta3);
                Vector128<int> deltaSum4 = Sse2.MultiplyAddAdjacent(delta4, delta4);
                Vector128<int> deltaSum5 = Sse2.MultiplyAddAdjacent(delta5, delta5);
                Vector128<int> deltaSum6 = Sse2.MultiplyAddAdjacent(delta6, delta6);
                Vector128<int> deltaSum7 = Sse2.MultiplyAddAdjacent(delta7, delta7);

                Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);
                Vector128<int> deltaSum23 = Ssse3.HorizontalAdd(deltaSum2, deltaSum3);
                Vector128<int> deltaSum45 = Ssse3.HorizontalAdd(deltaSum4, deltaSum5);
                Vector128<int> deltaSum67 = Ssse3.HorizontalAdd(deltaSum6, deltaSum7);

                Vector128<ushort> delta0123 = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum23);
                Vector128<ushort> delta4567 = Sse41.PackUnsignedSaturate(deltaSum45, deltaSum67);

                Vector128<ushort> min0123 = Sse41.MinHorizontal(delta0123);
                Vector128<ushort> min4567 = Sse41.MinHorizontal(delta4567);

                ushort minPos0123 = min0123.GetElement(0);
                ushort minPos4567 = min4567.GetElement(0);

                if (minPos4567 < minPos0123)
                {
                    errorSum += minPos4567;
                }
                else
                {
                    errorSum += minPos0123;
                }
            }

            return errorSum;
        }

        private static int SelectIndicesFallback(
            ReadOnlySpan<uint> values,
            uint endPoint0,
            uint endPoint1,
            int pBit0,
            int pBit1,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            uint alphaMask)
        {
            int errorSum = 0;

            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            Span<uint> palette = stackalloc uint[indexCount];

            RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoint0), colorDepth, alphaDepth, pBit0);
            RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoint1), colorDepth, alphaDepth, pBit1);

            Unsafe.As<RgbaColor8, uint>(ref c0) |= alphaMaskForPalette;
            Unsafe.As<RgbaColor8, uint>(ref c1) |= alphaMaskForPalette;

            palette[0] = c0.ToUInt32();
            palette[indexCount - 1] = c1.ToUInt32();

            for (int j = 1; j < indexCount - 1; j++)
            {
                palette[j] = Interpolate(c0, c1, j, indexBitCount).ToUInt32();
            }

            for (int i = 0; i < values.Length; i++)
            {
                uint color = values[i] | alphaMask;

                int bestMatchScore = int.MaxValue;
                int bestMatchIndex = 0;

                for (int j = 0; j < indexCount; j++)
                {
                    int score = SquaredDifference(
                        RgbaColor8.FromUInt32(color).GetColor32(),
                        RgbaColor8.FromUInt32(palette[j]).GetColor32());

                    if (score < bestMatchScore)
                    {
                        bestMatchScore = score;
                        bestMatchIndex = j;
                    }
                }

                errorSum += bestMatchScore;
            }

            return errorSum;
        }

        public static int SelectIndices(
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            ReadOnlySpan<uint> endPoints0,
            ReadOnlySpan<uint> endPoints1,
            ReadOnlySpan<int> pBitValues,
            Span<byte> indices,
            int subsetCount,
            int partition,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            int pBits,
            uint alphaMask)
        {
            if (Sse41.IsSupported)
            {
                if (indexBitCount == 2)
                {
                    return Select2BitIndicesSse41(
                        tile,
                        w,
                        h,
                        endPoints0,
                        endPoints1,
                        pBitValues,
                        indices,
                        subsetCount,
                        partition,
                        colorDepth,
                        alphaDepth,
                        pBits,
                        alphaMask);
                }
                else if (indexBitCount == 3)
                {
                    return Select3BitIndicesSse41(
                        tile,
                        w,
                        h,
                        endPoints0,
                        endPoints1,
                        pBitValues,
                        indices,
                        subsetCount,
                        partition,
                        colorDepth,
                        alphaDepth,
                        pBits,
                        alphaMask);
                }
                else if (indexBitCount == 4)
                {
                    Debug.Assert(subsetCount == 1);

                    return Select4BitIndicesOneSubsetSse41(
                        tile,
                        w,
                        h,
                        endPoints0[0],
                        endPoints1[0],
                        pBitValues,
                        indices,
                        partition,
                        colorDepth,
                        alphaDepth,
                        pBits,
                        alphaMask);
                }
            }

            return SelectIndicesFallback(
                tile,
                w,
                h,
                endPoints0,
                endPoints1,
                pBitValues,
                indices,
                subsetCount,
                partition,
                indexBitCount,
                indexCount,
                colorDepth,
                alphaDepth,
                pBits,
                alphaMask);
        }

        private static unsafe int Select2BitIndicesSse41(
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            ReadOnlySpan<uint> endPoints0,
            ReadOnlySpan<uint> endPoints1,
            ReadOnlySpan<int> pBitValues,
            Span<byte> indices,
            int subsetCount,
            int partition,
            int colorDepth,
            int alphaDepth,
            int pBits,
            uint alphaMask)
        {
            byte[] partitionTable = BC67Tables.PartitionTable[subsetCount - 1][partition];

            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            for (int subset = 0; subset < subsetCount; subset++)
            {
                int pBit0 = -1, pBit1 = -1;

                if (pBits == subsetCount)
                {
                    pBit0 = pBit1 = pBitValues[subset];
                }
                else if (pBits != 0)
                {
                    pBit0 = pBitValues[subset * 2];
                    pBit1 = pBitValues[subset * 2 + 1];
                }

                RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoints0[subset]), colorDepth, alphaDepth, pBit0);
                RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoints1[subset]), colorDepth, alphaDepth, pBit1);

                Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
                Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

                Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

                Vector128<byte> rWeights;
                Vector128<byte> lWeights;

                fixed (byte* pWeights = BC67Tables.Weights[0], pInvWeights = BC67Tables.InverseWeights[0])
                {
                    rWeights = Sse2.LoadScalarVector128((uint*)pWeights).AsByte();
                    lWeights = Sse2.LoadScalarVector128((uint*)pInvWeights).AsByte();
                }

                Vector128<byte> iWeights = Sse2.UnpackLow(lWeights, rWeights);
                Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
                Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
                Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();

                Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
                Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));

                int i = 0;
                for (int ty = 0; ty < h; ty++)
                {
                    for (int tx = 0; tx < w; tx++, i++)
                    {
                        int tileOffset = ty * 4 + tx;
                        if (partitionTable[tileOffset] != subset)
                        {
                            continue;
                        }

                        uint c = tile[i] | alphaMask;

                        Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                        Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                        Vector128<short> delta1 = Sse2.Subtract(color, pal1);

                        Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                        Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);

                        Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);

                        Vector128<ushort> delta = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum01);

                        Vector128<ushort> min = Sse41.MinHorizontal(delta);

                        uint minPos = min.AsUInt32().GetElement(0);
                        ushort error = (ushort)minPos;
                        uint index = minPos >> 16;

                        indices[tileOffset] = (byte)index;
                        errorSum += error;
                    }
                }
            }

            return errorSum;
        }

        private static unsafe int Select3BitIndicesSse41(
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            ReadOnlySpan<uint> endPoints0,
            ReadOnlySpan<uint> endPoints1,
            ReadOnlySpan<int> pBitValues,
            Span<byte> indices,
            int subsetCount,
            int partition,
            int colorDepth,
            int alphaDepth,
            int pBits,
            uint alphaMask)
        {
            byte[] partitionTable = BC67Tables.PartitionTable[subsetCount - 1][partition];

            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            for (int subset = 0; subset < subsetCount; subset++)
            {
                int pBit0 = -1, pBit1 = -1;

                if (pBits == subsetCount)
                {
                    pBit0 = pBit1 = pBitValues[subset];
                }
                else if (pBits != 0)
                {
                    pBit0 = pBitValues[subset * 2];
                    pBit1 = pBitValues[subset * 2 + 1];
                }

                RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoints0[subset]), colorDepth, alphaDepth, pBit0);
                RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoints1[subset]), colorDepth, alphaDepth, pBit1);

                Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
                Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

                Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

                Vector128<byte> rWeights;
                Vector128<byte> lWeights;

                fixed (byte* pWeights = BC67Tables.Weights[1], pInvWeights = BC67Tables.InverseWeights[1])
                {
                    rWeights = Sse2.LoadScalarVector128((ulong*)pWeights).AsByte();
                    lWeights = Sse2.LoadScalarVector128((ulong*)pInvWeights).AsByte();
                }

                Vector128<byte> iWeights = Sse2.UnpackLow(lWeights, rWeights);
                Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
                Vector128<byte> iWeights23 = Sse2.UnpackHigh(iWeights.AsInt16(), iWeights.AsInt16()).AsByte();
                Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
                Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
                Vector128<byte> iWeights2 = Sse2.UnpackLow(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
                Vector128<byte> iWeights3 = Sse2.UnpackHigh(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();

                Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
                Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));
                Vector128<short> pal2 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights2.AsSByte()));
                Vector128<short> pal3 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights3.AsSByte()));

                int i = 0;
                for (int ty = 0; ty < h; ty++)
                {
                    for (int tx = 0; tx < w; tx++, i++)
                    {
                        int tileOffset = ty * 4 + tx;
                        if (partitionTable[tileOffset] != subset)
                        {
                            continue;
                        }

                        uint c = tile[i] | alphaMask;

                        Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                        Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                        Vector128<short> delta1 = Sse2.Subtract(color, pal1);
                        Vector128<short> delta2 = Sse2.Subtract(color, pal2);
                        Vector128<short> delta3 = Sse2.Subtract(color, pal3);

                        Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                        Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);
                        Vector128<int> deltaSum2 = Sse2.MultiplyAddAdjacent(delta2, delta2);
                        Vector128<int> deltaSum3 = Sse2.MultiplyAddAdjacent(delta3, delta3);

                        Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);
                        Vector128<int> deltaSum23 = Ssse3.HorizontalAdd(deltaSum2, deltaSum3);

                        Vector128<ushort> delta = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum23);

                        Vector128<ushort> min = Sse41.MinHorizontal(delta);

                        uint minPos = min.AsUInt32().GetElement(0);
                        ushort error = (ushort)minPos;
                        uint index = minPos >> 16;

                        indices[tileOffset] = (byte)index;
                        errorSum += error;
                    }
                }
            }

            return errorSum;
        }

        private static unsafe int Select4BitIndicesOneSubsetSse41(
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            uint endPoint0,
            uint endPoint1,
            ReadOnlySpan<int> pBitValues,
            Span<byte> indices,
            int partition,
            int colorDepth,
            int alphaDepth,
            int pBits,
            uint alphaMask)
        {
            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            int errorSum = 0;

            int pBit0 = -1, pBit1 = -1;

            if (pBits != 0)
            {
                pBit0 = pBitValues[0];
                pBit1 = pBitValues[1];
            }

            RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoint0), colorDepth, alphaDepth, pBit0);
            RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoint1), colorDepth, alphaDepth, pBit1);

            Vector128<byte> c0Rep = Vector128.Create(c0.ToUInt32() | alphaMaskForPalette).AsByte();
            Vector128<byte> c1Rep = Vector128.Create(c1.ToUInt32() | alphaMaskForPalette).AsByte();

            Vector128<byte> c0c1 = Sse2.UnpackLow(c0Rep, c1Rep);

            Vector128<byte> rWeights;
            Vector128<byte> lWeights;

            fixed (byte* pWeights = BC67Tables.Weights[2], pInvWeights = BC67Tables.InverseWeights[2])
            {
                rWeights = Sse2.LoadVector128(pWeights);
                lWeights = Sse2.LoadVector128(pInvWeights);
            }

            Vector128<byte> iWeightsLow = Sse2.UnpackLow(lWeights, rWeights);
            Vector128<byte> iWeightsHigh = Sse2.UnpackHigh(lWeights, rWeights);
            Vector128<byte> iWeights01 = Sse2.UnpackLow(iWeightsLow.AsInt16(), iWeightsLow.AsInt16()).AsByte();
            Vector128<byte> iWeights23 = Sse2.UnpackHigh(iWeightsLow.AsInt16(), iWeightsLow.AsInt16()).AsByte();
            Vector128<byte> iWeights45 = Sse2.UnpackLow(iWeightsHigh.AsInt16(), iWeightsHigh.AsInt16()).AsByte();
            Vector128<byte> iWeights67 = Sse2.UnpackHigh(iWeightsHigh.AsInt16(), iWeightsHigh.AsInt16()).AsByte();
            Vector128<byte> iWeights0 = Sse2.UnpackLow(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights1 = Sse2.UnpackHigh(iWeights01.AsInt16(), iWeights01.AsInt16()).AsByte();
            Vector128<byte> iWeights2 = Sse2.UnpackLow(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
            Vector128<byte> iWeights3 = Sse2.UnpackHigh(iWeights23.AsInt16(), iWeights23.AsInt16()).AsByte();
            Vector128<byte> iWeights4 = Sse2.UnpackLow(iWeights45.AsInt16(), iWeights45.AsInt16()).AsByte();
            Vector128<byte> iWeights5 = Sse2.UnpackHigh(iWeights45.AsInt16(), iWeights45.AsInt16()).AsByte();
            Vector128<byte> iWeights6 = Sse2.UnpackLow(iWeights67.AsInt16(), iWeights67.AsInt16()).AsByte();
            Vector128<byte> iWeights7 = Sse2.UnpackHigh(iWeights67.AsInt16(), iWeights67.AsInt16()).AsByte();

            Vector128<short> pal0 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights0.AsSByte()));
            Vector128<short> pal1 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights1.AsSByte()));
            Vector128<short> pal2 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights2.AsSByte()));
            Vector128<short> pal3 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights3.AsSByte()));
            Vector128<short> pal4 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights4.AsSByte()));
            Vector128<short> pal5 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights5.AsSByte()));
            Vector128<short> pal6 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights6.AsSByte()));
            Vector128<short> pal7 = ShiftRoundToNearest(Ssse3.MultiplyAddAdjacent(c0c1, iWeights7.AsSByte()));

            int i = 0;
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++, i++)
                {
                    uint c = tile[i] | alphaMask;

                    Vector128<short> color = Sse41.ConvertToVector128Int16(Vector128.Create(c).AsByte());

                    Vector128<short> delta0 = Sse2.Subtract(color, pal0);
                    Vector128<short> delta1 = Sse2.Subtract(color, pal1);
                    Vector128<short> delta2 = Sse2.Subtract(color, pal2);
                    Vector128<short> delta3 = Sse2.Subtract(color, pal3);
                    Vector128<short> delta4 = Sse2.Subtract(color, pal4);
                    Vector128<short> delta5 = Sse2.Subtract(color, pal5);
                    Vector128<short> delta6 = Sse2.Subtract(color, pal6);
                    Vector128<short> delta7 = Sse2.Subtract(color, pal7);

                    Vector128<int> deltaSum0 = Sse2.MultiplyAddAdjacent(delta0, delta0);
                    Vector128<int> deltaSum1 = Sse2.MultiplyAddAdjacent(delta1, delta1);
                    Vector128<int> deltaSum2 = Sse2.MultiplyAddAdjacent(delta2, delta2);
                    Vector128<int> deltaSum3 = Sse2.MultiplyAddAdjacent(delta3, delta3);
                    Vector128<int> deltaSum4 = Sse2.MultiplyAddAdjacent(delta4, delta4);
                    Vector128<int> deltaSum5 = Sse2.MultiplyAddAdjacent(delta5, delta5);
                    Vector128<int> deltaSum6 = Sse2.MultiplyAddAdjacent(delta6, delta6);
                    Vector128<int> deltaSum7 = Sse2.MultiplyAddAdjacent(delta7, delta7);

                    Vector128<int> deltaSum01 = Ssse3.HorizontalAdd(deltaSum0, deltaSum1);
                    Vector128<int> deltaSum23 = Ssse3.HorizontalAdd(deltaSum2, deltaSum3);
                    Vector128<int> deltaSum45 = Ssse3.HorizontalAdd(deltaSum4, deltaSum5);
                    Vector128<int> deltaSum67 = Ssse3.HorizontalAdd(deltaSum6, deltaSum7);

                    Vector128<ushort> delta0123 = Sse41.PackUnsignedSaturate(deltaSum01, deltaSum23);
                    Vector128<ushort> delta4567 = Sse41.PackUnsignedSaturate(deltaSum45, deltaSum67);

                    Vector128<ushort> min0123 = Sse41.MinHorizontal(delta0123);
                    Vector128<ushort> min4567 = Sse41.MinHorizontal(delta4567);

                    uint minPos0123 = min0123.AsUInt32().GetElement(0);
                    uint minPos4567 = min4567.AsUInt32().GetElement(0);

                    if ((ushort)minPos4567 < (ushort)minPos0123)
                    {
                        errorSum += (ushort)minPos4567;
                        indices[ty * 4 + tx] = (byte)(8 + (minPos4567 >> 16));
                    }
                    else
                    {
                        errorSum += (ushort)minPos0123;
                        indices[ty * 4 + tx] = (byte)(minPos0123 >> 16);
                    }
                }
            }

            return errorSum;
        }

        private static Vector128<short> ShiftRoundToNearest(Vector128<short> x)
        {
            return Sse2.ShiftRightLogical(Sse2.Add(x, Vector128.Create((short)32)), 6);
        }

        private static int SelectIndicesFallback(
            ReadOnlySpan<uint> tile,
            int w,
            int h,
            ReadOnlySpan<uint> endPoints0,
            ReadOnlySpan<uint> endPoints1,
            ReadOnlySpan<int> pBitValues,
            Span<byte> indices,
            int subsetCount,
            int partition,
            int indexBitCount,
            int indexCount,
            int colorDepth,
            int alphaDepth,
            int pBits,
            uint alphaMask)
        {
            int errorSum = 0;

            uint alphaMaskForPalette = alphaMask;

            if (alphaDepth == 0)
            {
                alphaMaskForPalette |= new RgbaColor8(0, 0, 0, 255).ToUInt32();
            }

            Span<uint> palette = stackalloc uint[subsetCount * indexCount];

            for (int subset = 0; subset < subsetCount; subset++)
            {
                int palBase = subset * indexCount;

                int pBit0 = -1, pBit1 = -1;

                if (pBits == subsetCount)
                {
                    pBit0 = pBit1 = pBitValues[subset];
                }
                else if (pBits != 0)
                {
                    pBit0 = pBitValues[subset * 2];
                    pBit1 = pBitValues[subset * 2 + 1];
                }

                RgbaColor8 c0 = Quantize(RgbaColor8.FromUInt32(endPoints0[subset]), colorDepth, alphaDepth, pBit0);
                RgbaColor8 c1 = Quantize(RgbaColor8.FromUInt32(endPoints1[subset]), colorDepth, alphaDepth, pBit1);

                Unsafe.As<RgbaColor8, uint>(ref c0) |= alphaMaskForPalette;
                Unsafe.As<RgbaColor8, uint>(ref c1) |= alphaMaskForPalette;

                palette[palBase + 0] = c0.ToUInt32();
                palette[palBase + indexCount - 1] = c1.ToUInt32();

                for (int j = 1; j < indexCount - 1; j++)
                {
                    palette[palBase + j] = Interpolate(c0, c1, j, indexBitCount).ToUInt32();
                }
            }

            int i = 0;
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++)
                {
                    int subset = BC67Tables.PartitionTable[subsetCount - 1][partition][ty * 4 + tx];
                    uint color = tile[i++] | alphaMask;

                    int bestMatchScore = int.MaxValue;
                    int bestMatchIndex = 0;

                    for (int j = 0; j < indexCount; j++)
                    {
                        int score = SquaredDifference(
                            RgbaColor8.FromUInt32(color).GetColor32(),
                            RgbaColor8.FromUInt32(palette[subset * indexCount + j]).GetColor32());

                        if (score < bestMatchScore)
                        {
                            bestMatchScore = score;
                            bestMatchIndex = j;
                        }
                    }

                    indices[ty * 4 + tx] = (byte)bestMatchIndex;
                    errorSum += bestMatchScore;
                }
            }

            return errorSum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SquaredDifference(RgbaColor32 color1, RgbaColor32 color2)
        {
            RgbaColor32 delta = color1 - color2;
            return RgbaColor32.Dot(delta, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RgbaColor8 Interpolate(RgbaColor8 color1, RgbaColor8 color2, int weightIndex, int indexBitCount)
        {
            return Interpolate(color1.GetColor32(), color2.GetColor32(), weightIndex, indexBitCount).GetColor8();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RgbaColor32 Interpolate(RgbaColor32 color1, RgbaColor32 color2, int weightIndex, int indexBitCount)
        {
            Debug.Assert(indexBitCount >= 2 && indexBitCount <= 4);

            int weight = (((weightIndex << 7) / ((1 << indexBitCount) - 1)) + 1) >> 1;

            RgbaColor32 weightV = new(weight);
            RgbaColor32 invWeightV = new(64 - weight);

            return (color1 * invWeightV + color2 * weightV + new RgbaColor32(32)) >> 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RgbaColor32 Interpolate(
            RgbaColor32 color1,
            RgbaColor32 color2,
            int colorWeightIndex,
            int alphaWeightIndex,
            int colorIndexBitCount,
            int alphaIndexBitCount)
        {
            Debug.Assert(colorIndexBitCount >= 2 && colorIndexBitCount <= 4);
            Debug.Assert(alphaIndexBitCount >= 2 && alphaIndexBitCount <= 4);

            int colorWeight = BC67Tables.Weights[colorIndexBitCount - 2][colorWeightIndex];
            int alphaWeight = BC67Tables.Weights[alphaIndexBitCount - 2][alphaWeightIndex];

            RgbaColor32 weightV = new(colorWeight)
            {
                A = alphaWeight,
            };
            RgbaColor32 invWeightV = new RgbaColor32(64) - weightV;

            return (color1 * invWeightV + color2 * weightV + new RgbaColor32(32)) >> 6;
        }

        public static RgbaColor8 Quantize(RgbaColor8 color, int colorBits, int alphaBits, int pBit = -1)
        {
            if (alphaBits == 0)
            {
                int colorShift = 8 - colorBits;

                uint c;

                if (pBit >= 0)
                {
                    byte[] lutColor = _quantizationLut[colorBits - 4];

                    Debug.Assert(pBit <= 1);
                    int high = pBit << 8;
                    uint mask = (0xffu >> (colorBits + 1)) * 0x10101;

                    c = lutColor[color.R | high];
                    c |= (uint)lutColor[color.G | high] << 8;
                    c |= (uint)lutColor[color.B | high] << 16;

                    c <<= colorShift;
                    c |= (c >> (colorBits + 1)) & mask;
                    c |= ((uint)pBit * 0x10101) << (colorShift - 1);
                }
                else
                {
                    byte[] lutColor = _quantizationLutNoPBit[colorBits - 4];

                    uint mask = (0xffu >> colorBits) * 0x10101;

                    c = lutColor[color.R];
                    c |= (uint)lutColor[color.G] << 8;
                    c |= (uint)lutColor[color.B] << 16;

                    c <<= colorShift;
                    c |= (c >> colorBits) & mask;
                }

                c |= (uint)color.A << 24;

                return RgbaColor8.FromUInt32(c);
            }

            return QuantizeFallback(color, colorBits, alphaBits, pBit);
        }

        private static RgbaColor8 QuantizeFallback(RgbaColor8 color, int colorBits, int alphaBits, int pBit = -1)
        {
            byte r = UnquantizeComponent(QuantizeComponent(color.R, colorBits, pBit), colorBits, pBit);
            byte g = UnquantizeComponent(QuantizeComponent(color.G, colorBits, pBit), colorBits, pBit);
            byte b = UnquantizeComponent(QuantizeComponent(color.B, colorBits, pBit), colorBits, pBit);
            byte a = alphaBits == 0 ? color.A : UnquantizeComponent(QuantizeComponent(color.A, alphaBits, pBit), alphaBits, pBit);
            return new RgbaColor8(r, g, b, a);
        }

        public static byte QuantizeComponent(byte component, int bits, int pBit = -1)
        {
            return pBit >= 0 ? _quantizationLut[bits - 4][component | (pBit << 8)] : _quantizationLutNoPBit[bits - 4][component];
        }

        private static byte QuantizeComponentForLut(byte component, int bits, int pBit = -1)
        {
            int shift = 8 - bits;
            int fill = component >> bits;

            if (pBit >= 0)
            {
                Debug.Assert(pBit <= 1);
                fill >>= 1;
                fill |= pBit << (shift - 1);
            }

            int q1 = component >> shift;
            int q2 = Math.Max(q1 - 1, 0);
            int q3 = Math.Min(q1 + 1, (1 << bits) - 1);

            int delta1 = FastAbs(((q1 << shift) | fill) - component);
            int delta2 = component - ((q2 << shift) | fill);
            int delta3 = ((q3 << shift) | fill) - component;

            if (delta1 < delta2 && delta1 < delta3)
            {
                return (byte)q1;
            }
            else if (delta2 < delta3)
            {
                return (byte)q2;
            }
            else
            {
                return (byte)q3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastAbs(int x)
        {
            int sign = x >> 31;
            return (x + sign) ^ sign;
        }

        private static byte UnquantizeComponent(byte component, int bits, int pBit)
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

            return (byte)value;
        }
    }
}
