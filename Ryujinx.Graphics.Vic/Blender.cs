using Ryujinx.Graphics.Vic.Image;
using Ryujinx.Graphics.Vic.Types;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Vic
{
    static class Blender
    {
        public static void BlendOne(Surface dst, Surface src, ref SlotStruct slot)
        {
            if (Sse41.IsSupported && (dst.Width & 3) == 0)
            {
                BlendOneSse41(dst, src, ref slot);
                return;
            }

            for (int y = 0; y < dst.Height; y++)
            {
                for (int x = 0; x < dst.Width; x++)
                {
                    int inR = src.GetR(x, y);
                    int inG = src.GetG(x, y);
                    int inB = src.GetB(x, y);

                    MatrixMultiply(ref slot.ColorMatrixStruct, inR, inG, inB, out int r, out int g, out int b);

                    r = Math.Clamp(r, slot.SlotConfig.SoftClampLow, slot.SlotConfig.SoftClampHigh);
                    g = Math.Clamp(g, slot.SlotConfig.SoftClampLow, slot.SlotConfig.SoftClampHigh);
                    b = Math.Clamp(b, slot.SlotConfig.SoftClampLow, slot.SlotConfig.SoftClampHigh);

                    dst.SetR(x, y, (ushort)r);
                    dst.SetG(x, y, (ushort)g);
                    dst.SetB(x, y, (ushort)b);
                    dst.SetA(x, y, src.GetA(x, y));
                }
            }
        }

        private unsafe static void BlendOneSse41(Surface dst, Surface src, ref SlotStruct slot)
        {
            Debug.Assert((dst.Width & 3) == 0);

            ref MatrixStruct mtx = ref slot.ColorMatrixStruct;

            int one = 1 << (mtx.MatrixRShift + 8);

            Vector128<int> col1 = Vector128.Create(mtx.MatrixCoeff00, mtx.MatrixCoeff10, mtx.MatrixCoeff20, 0);
            Vector128<int> col2 = Vector128.Create(mtx.MatrixCoeff01, mtx.MatrixCoeff11, mtx.MatrixCoeff21, 0);
            Vector128<int> col3 = Vector128.Create(mtx.MatrixCoeff02, mtx.MatrixCoeff12, mtx.MatrixCoeff22, one);
            Vector128<int> col4 = Vector128.Create(mtx.MatrixCoeff03, mtx.MatrixCoeff13, mtx.MatrixCoeff23, 0);

            Vector128<int> rShift = Vector128.CreateScalar(mtx.MatrixRShift);
            Vector128<ushort> clMin = Vector128.Create((ushort)slot.SlotConfig.SoftClampLow);
            Vector128<ushort> clMax = Vector128.Create((ushort)slot.SlotConfig.SoftClampHigh);

            fixed (Pixel* srcPtr = src.Data, dstPtr = dst.Data)
            {
                Pixel* ip = srcPtr;
                Pixel* op = dstPtr;

                for (int y = 0; y < dst.Height; y++, ip += src.Width, op += dst.Width)
                {
                    for (int x = 0; x < dst.Width; x += 4)
                    {
                        Vector128<int> pixel1 = Sse41.ConvertToVector128Int32((ushort*)(ip + (uint)x));
                        Vector128<int> pixel2 = Sse41.ConvertToVector128Int32((ushort*)(ip + (uint)x + 1));
                        Vector128<int> pixel3 = Sse41.ConvertToVector128Int32((ushort*)(ip + (uint)x + 2));
                        Vector128<int> pixel4 = Sse41.ConvertToVector128Int32((ushort*)(ip + (uint)x + 3));

                        Vector128<ushort> pixel12, pixel34;

                        if (mtx.MatrixEnable)
                        {
                            pixel12 = Sse41.PackUnsignedSaturate(
                                MatrixMultiplySse41(pixel1, col1, col2, col3, col4, rShift),
                                MatrixMultiplySse41(pixel2, col1, col2, col3, col4, rShift));
                            pixel34 = Sse41.PackUnsignedSaturate(
                                MatrixMultiplySse41(pixel3, col1, col2, col3, col4, rShift),
                                MatrixMultiplySse41(pixel4, col1, col2, col3, col4, rShift));
                        }
                        else
                        {
                            pixel12 = Sse41.PackUnsignedSaturate(pixel1, pixel2);
                            pixel34 = Sse41.PackUnsignedSaturate(pixel3, pixel4);
                        }

                        pixel12 = Sse41.Min(pixel12, clMax);
                        pixel34 = Sse41.Min(pixel34, clMax);
                        pixel12 = Sse41.Max(pixel12, clMin);
                        pixel34 = Sse41.Max(pixel34, clMin);

                        Sse2.Store((ushort*)(op + (uint)x + 0), pixel12);
                        Sse2.Store((ushort*)(op + (uint)x + 2), pixel34);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MatrixMultiply(ref MatrixStruct mtx, int x, int y, int z, out int r, out int g, out int b)
        {
            if (mtx.MatrixEnable)
            {
                r = x * mtx.MatrixCoeff00 + y * mtx.MatrixCoeff01 + z * mtx.MatrixCoeff02;
                g = x * mtx.MatrixCoeff10 + y * mtx.MatrixCoeff11 + z * mtx.MatrixCoeff12;
                b = x * mtx.MatrixCoeff20 + y * mtx.MatrixCoeff21 + z * mtx.MatrixCoeff22;

                r >>= mtx.MatrixRShift;
                g >>= mtx.MatrixRShift;
                b >>= mtx.MatrixRShift;

                r += mtx.MatrixCoeff03;
                g += mtx.MatrixCoeff13;
                b += mtx.MatrixCoeff23;

                r >>= 8;
                g >>= 8;
                b >>= 8;
            }
            else
            {
                r = x;
                g = y;
                b = z;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> MatrixMultiplySse41(
            Vector128<int> pixel,
            Vector128<int> col1,
            Vector128<int> col2,
            Vector128<int> col3,
            Vector128<int> col4,
            Vector128<int> rShift)
        {
            Vector128<int> x = Sse2.Shuffle(pixel, 0);
            Vector128<int> y = Sse2.Shuffle(pixel, 0x55);
            Vector128<int> z = Sse2.Shuffle(pixel, 0xea);

            col1 = Sse41.MultiplyLow(col1, x);
            col2 = Sse41.MultiplyLow(col2, y);
            col3 = Sse41.MultiplyLow(col3, z);

            Vector128<int> res = Sse2.Add(col3, Sse2.Add(col1, col2));

            res = Sse2.ShiftRightArithmetic(res, rShift);
            res = Sse2.Add(res, col4);
            res = Sse2.ShiftRightArithmetic(res, 8);

            return res;
        }
    }
}
