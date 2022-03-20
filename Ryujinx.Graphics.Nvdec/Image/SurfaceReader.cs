using Ryujinx.Common;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Video;
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Ryujinx.Graphics.Nvdec.Image.SurfaceCommon;

namespace Ryujinx.Graphics.Nvdec.Image
{
    static class SurfaceReader
    {
        public static void Read(MemoryManager gmm, ISurface surface, uint lumaOffset, uint chromaOffset)
        {
            int width = surface.Width;
            int height = surface.Height;
            int stride = surface.Stride;

            ReadOnlySpan<byte> luma = gmm.DeviceGetSpan(lumaOffset, GetBlockLinearSize(width, height, 1));

            ReadLuma(surface.YPlane.AsSpan(), luma, stride, width, height);

            int uvWidth = surface.UvWidth;
            int uvHeight = surface.UvHeight;
            int uvStride = surface.UvStride;

            ReadOnlySpan<byte> chroma = gmm.DeviceGetSpan(chromaOffset, GetBlockLinearSize(uvWidth, uvHeight, 2));

            ReadChroma(surface.UPlane.AsSpan(), surface.VPlane.AsSpan(), chroma, uvStride, uvWidth, uvHeight);
        }

        private static void ReadLuma(Span<byte> dst, ReadOnlySpan<byte> src, int dstStride, int width, int height)
        {
            LayoutConverter.ConvertBlockLinearToLinear(dst, width, height, dstStride, 1, 2, src);
        }

        private unsafe static void ReadChroma(
            Span<byte> dstU,
            Span<byte> dstV,
            ReadOnlySpan<byte> src,
            int dstStride,
            int width,
            int height)
        {
            OffsetCalculator calc = new OffsetCalculator(width, height, 0, false, 2, 2);

            if (Sse2.IsSupported)
            {
                int strideTrunc64 = BitUtils.AlignDown(width * 2, 64);

                int outStrideGap = dstStride - width;

                fixed (byte* dstUPtr = dstU, dstVPtr = dstV, dataPtr = src)
                {
                    byte* uPtr = dstUPtr;
                    byte* vPtr = dstVPtr;

                    for (int y = 0; y < height; y++)
                    {
                        calc.SetY(y);

                        for (int x = 0; x < strideTrunc64; x += 64, uPtr += 32, vPtr += 32)
                        {
                            byte* offset = dataPtr + calc.GetOffsetWithLineOffset64(x);
                            byte* offset2 = offset + 0x20;
                            byte* offset3 = offset + 0x100;
                            byte* offset4 = offset + 0x120;

                            Vector128<byte> value = *(Vector128<byte>*)offset;
                            Vector128<byte> value2 = *(Vector128<byte>*)offset2;
                            Vector128<byte> value3 = *(Vector128<byte>*)offset3;
                            Vector128<byte> value4 = *(Vector128<byte>*)offset4;

                            Vector128<byte> u00 = Sse2.UnpackLow(value, value2);
                            Vector128<byte> v00 = Sse2.UnpackHigh(value, value2);
                            Vector128<byte> u01 = Sse2.UnpackLow(value3, value4);
                            Vector128<byte> v01 = Sse2.UnpackHigh(value3, value4);

                            Vector128<byte> u10 = Sse2.UnpackLow(u00, v00);
                            Vector128<byte> v10 = Sse2.UnpackHigh(u00, v00);
                            Vector128<byte> u11 = Sse2.UnpackLow(u01, v01);
                            Vector128<byte> v11 = Sse2.UnpackHigh(u01, v01);

                            Vector128<byte> u20 = Sse2.UnpackLow(u10, v10);
                            Vector128<byte> v20 = Sse2.UnpackHigh(u10, v10);
                            Vector128<byte> u21 = Sse2.UnpackLow(u11, v11);
                            Vector128<byte> v21 = Sse2.UnpackHigh(u11, v11);

                            Vector128<byte> u30 = Sse2.UnpackLow(u20, v20);
                            Vector128<byte> v30 = Sse2.UnpackHigh(u20, v20);
                            Vector128<byte> u31 = Sse2.UnpackLow(u21, v21);
                            Vector128<byte> v31 = Sse2.UnpackHigh(u21, v21);

                            *(Vector128<byte>*)uPtr = u30;
                            *(Vector128<byte>*)(uPtr + 16) = u31;
                            *(Vector128<byte>*)vPtr = v30;
                            *(Vector128<byte>*)(vPtr + 16) = v31;
                        }

                        for (int x = strideTrunc64 / 2; x < width; x++, uPtr++, vPtr++)
                        {
                            byte* offset = dataPtr + calc.GetOffset(x);

                            *uPtr = *offset;
                            *vPtr = *(offset + 1);
                        }

                        uPtr += outStrideGap;
                        vPtr += outStrideGap;
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    int dstBaseOffset = y * dstStride;

                    calc.SetY(y);

                    for (int x = 0; x < width; x++)
                    {
                        int srcOffset = calc.GetOffset(x);

                        dstU[dstBaseOffset + x] = src[srcOffset];
                        dstV[dstBaseOffset + x] = src[srcOffset + 1];
                    }
                }
            }
        }
    }
}
