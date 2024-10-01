using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Vic
{
    static class Scaler
    {
        public static void DeinterlaceWeave(Span<byte> data, ReadOnlySpan<byte> prevData, int width, int fieldSize, bool isTopField)
        {
            // Prev I    Curr I    Curr P
            // TTTTTTTT  BBBBBBBB  TTTTTTTT
            // --------  --------  BBBBBBBB

            if (isTopField)
            {
                for (int offset = 0; offset < data.Length; offset += fieldSize * 2)
                {
                    prevData.Slice(offset >> 1, width).CopyTo(data.Slice(offset + fieldSize, width));
                }
            }
            else
            {
                for (int offset = 0; offset < data.Length; offset += fieldSize * 2)
                {
                    prevData.Slice(offset >> 1, width).CopyTo(data.Slice(offset, width));
                }
            }
        }

        public static void DeinterlaceBob(Span<byte> data, int width, int fieldSize, bool isTopField)
        {
            // Curr I    Curr P
            // TTTTTTTT  TTTTTTTT
            // --------  TTTTTTTT

            if (isTopField)
            {
                for (int offset = 0; offset < data.Length; offset += fieldSize * 2)
                {
                    data.Slice(offset, width).CopyTo(data.Slice(offset + fieldSize, width));
                }
            }
            else
            {
                for (int offset = 0; offset < data.Length; offset += fieldSize * 2)
                {
                    data.Slice(offset + fieldSize, width).CopyTo(data.Slice(offset, width));
                }
            }
        }

        public unsafe static void DeinterlaceMotionAdaptive(
            Span<byte> data,
            ReadOnlySpan<byte> prevData,
            ReadOnlySpan<byte> nextData,
            int width,
            int fieldSize,
            bool isTopField)
        {
            // Very simple motion adaptive algorithm.
            // If the pixel changed between previous and next frame, use Bob, otherwise use Weave.
            //
            // Example pseudo code:
            // C_even = (P_even == N_even) ? P_even : C_odd
            // Where: C is current frame, P is previous frame and N is next frame, and even/odd are the fields.
            //
            // Note: This does not fully match the hardware algorithm.
            // The motion adaptive deinterlacing implemented on hardware is considerably more complex,
            // and hard to implement accurately without proper documentation as for example, the
            // method used for motion estimation is unknown.

            int start = isTopField ? fieldSize : 0;
            int otherFieldOffset = isTopField ? -fieldSize : fieldSize;

            fixed (byte* pData = data, pPrevData = prevData, pNextData = nextData)
            {
                for (int offset = start; offset < data.Length; offset += fieldSize * 2)
                {
                    int refOffset = (offset - start) >> 1;
                    int x = 0;

                    if (Avx2.IsSupported)
                    {
                        for (; x < (width & ~0x1f); x += 32)
                        {
                            Vector256<byte> prevPixels = Avx.LoadVector256(pPrevData + refOffset + x);
                            Vector256<byte> nextPixels = Avx.LoadVector256(pNextData + refOffset + x);
                            Vector256<byte> bob = Avx.LoadVector256(pData + offset + otherFieldOffset + x);
                            Vector256<byte> diff = Avx2.CompareEqual(prevPixels, nextPixels);
                            Avx.Store(pData + offset + x, Avx2.BlendVariable(bob, prevPixels, diff));
                        }
                    }
                    else if (Sse41.IsSupported)
                    {
                        for (; x < (width & ~0xf); x += 16)
                        {
                            Vector128<byte> prevPixels = Sse2.LoadVector128(pPrevData + refOffset + x);
                            Vector128<byte> nextPixels = Sse2.LoadVector128(pNextData + refOffset + x);
                            Vector128<byte> bob = Sse2.LoadVector128(pData + offset + otherFieldOffset + x);
                            Vector128<byte> diff = Sse2.CompareEqual(prevPixels, nextPixels);
                            Sse2.Store(pData + offset + x, Sse41.BlendVariable(bob, prevPixels, diff));
                        }
                    }

                    for (; x < width; x++)
                    {
                        byte prevPixel = prevData[refOffset + x];
                        byte nextPixel = nextData[refOffset + x];

                        if (nextPixel != prevPixel)
                        {
                            data[offset + x] = data[offset + otherFieldOffset + x];
                        }
                        else
                        {
                            data[offset + x] = prevPixel;
                        }
                    }
                }
            }
        }
    }
}
