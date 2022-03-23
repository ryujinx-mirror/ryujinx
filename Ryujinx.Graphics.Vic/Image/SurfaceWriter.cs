using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Vic.Types;
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Ryujinx.Graphics.Vic.Image.SurfaceCommon;

namespace Ryujinx.Graphics.Vic.Image
{
    class SurfaceWriter
    {
        public static void Write(ResourceManager rm, Surface input, ref OutputSurfaceConfig config, ref PlaneOffsets offsets)
        {
            switch (config.OutPixelFormat)
            {
                case PixelFormat.A8B8G8R8:
                case PixelFormat.X8B8G8R8:
                    WriteA8B8G8R8(rm, input, ref config, ref offsets);
                    break;
                case PixelFormat.A8R8G8B8:
                    WriteA8R8G8B8(rm, input, ref config, ref offsets);
                    break;
                case PixelFormat.Y8___V8U8_N420:
                    WriteNv12(rm, input, ref config, ref offsets);
                    break;
                default:
                    Logger.Error?.Print(LogClass.Vic, $"Unsupported pixel format \"{config.OutPixelFormat}\".");
                    break;
            }
        }

        private unsafe static void WriteA8B8G8R8(ResourceManager rm, Surface input, ref OutputSurfaceConfig config, ref PlaneOffsets offsets)
        {
            int width = input.Width;
            int height = input.Height;
            int stride = GetPitch(width, 4);

            int dstIndex = rm.BufferPool.Rent(height * stride, out Span<byte> dst);

            if (Sse2.IsSupported)
            {
                int widthTrunc = width & ~7;
                int strideGap = stride - width * 4;

                fixed (Pixel* srcPtr = input.Data)
                {
                    Pixel* ip = srcPtr;

                    fixed (byte* dstPtr = dst)
                    {
                        byte* op = dstPtr;

                        for (int y = 0; y < height; y++, ip += input.Width)
                        {
                            int x = 0;

                            for (; x < widthTrunc; x += 8)
                            {
                                Vector128<ushort> pixel12 = Sse2.LoadVector128((ushort*)(ip + (uint)x));
                                Vector128<ushort> pixel34 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 2));
                                Vector128<ushort> pixel56 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 4));
                                Vector128<ushort> pixel78 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 6));

                                pixel12 = Sse2.ShiftRightLogical(pixel12, 2);
                                pixel34 = Sse2.ShiftRightLogical(pixel34, 2);
                                pixel56 = Sse2.ShiftRightLogical(pixel56, 2);
                                pixel78 = Sse2.ShiftRightLogical(pixel78, 2);

                                Vector128<byte> pixel1234 = Sse2.PackUnsignedSaturate(pixel12.AsInt16(), pixel34.AsInt16());
                                Vector128<byte> pixel5678 = Sse2.PackUnsignedSaturate(pixel56.AsInt16(), pixel78.AsInt16());

                                Sse2.Store(op + 0x00, pixel1234);
                                Sse2.Store(op + 0x10, pixel5678);

                                op += 0x20;
                            }

                            for (; x < width; x++)
                            {
                                Pixel* px = ip + (uint)x;

                                *(op + 0) = Downsample(px->R);
                                *(op + 1) = Downsample(px->G);
                                *(op + 2) = Downsample(px->B);
                                *(op + 3) = Downsample(px->A);

                                op += 4;
                            }

                            op += strideGap;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    int baseOffs = y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int offs = baseOffs + x * 4;

                        dst[offs + 0] = Downsample(input.GetR(x, y));
                        dst[offs + 1] = Downsample(input.GetG(x, y));
                        dst[offs + 2] = Downsample(input.GetB(x, y));
                        dst[offs + 3] = Downsample(input.GetA(x, y));
                    }
                }
            }

            bool outLinear = config.OutBlkKind == 0;

            int gobBlocksInY = 1 << config.OutBlkHeight;

            WriteBuffer(rm, dst, offsets.LumaOffset, outLinear, width, height, 4, gobBlocksInY);

            rm.BufferPool.Return(dstIndex);
        }

        private unsafe static void WriteA8R8G8B8(ResourceManager rm, Surface input, ref OutputSurfaceConfig config, ref PlaneOffsets offsets)
        {
            int width = input.Width;
            int height = input.Height;
            int stride = GetPitch(width, 4);

            int dstIndex = rm.BufferPool.Rent(height * stride, out Span<byte> dst);

            if (Ssse3.IsSupported)
            {
                Vector128<byte> shuffleMask = Vector128.Create(
                    (byte)2, (byte)1, (byte)0, (byte)3,
                    (byte)6, (byte)5, (byte)4, (byte)7,
                    (byte)10, (byte)9, (byte)8, (byte)11,
                    (byte)14, (byte)13, (byte)12, (byte)15);

                int widthTrunc = width & ~7;
                int strideGap = stride - width * 4;

                fixed (Pixel* srcPtr = input.Data)
                {
                    Pixel* ip = srcPtr;

                    fixed (byte* dstPtr = dst)
                    {
                        byte* op = dstPtr;

                        for (int y = 0; y < height; y++, ip += input.Width)
                        {
                            int x = 0;

                            for (; x < widthTrunc; x += 8)
                            {
                                Vector128<ushort> pixel12 = Sse2.LoadVector128((ushort*)(ip + (uint)x));
                                Vector128<ushort> pixel34 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 2));
                                Vector128<ushort> pixel56 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 4));
                                Vector128<ushort> pixel78 = Sse2.LoadVector128((ushort*)(ip + (uint)x + 6));

                                pixel12 = Sse2.ShiftRightLogical(pixel12, 2);
                                pixel34 = Sse2.ShiftRightLogical(pixel34, 2);
                                pixel56 = Sse2.ShiftRightLogical(pixel56, 2);
                                pixel78 = Sse2.ShiftRightLogical(pixel78, 2);

                                Vector128<byte> pixel1234 = Sse2.PackUnsignedSaturate(pixel12.AsInt16(), pixel34.AsInt16());
                                Vector128<byte> pixel5678 = Sse2.PackUnsignedSaturate(pixel56.AsInt16(), pixel78.AsInt16());

                                pixel1234 = Ssse3.Shuffle(pixel1234, shuffleMask);
                                pixel5678 = Ssse3.Shuffle(pixel5678, shuffleMask);

                                Sse2.Store(op + 0x00, pixel1234);
                                Sse2.Store(op + 0x10, pixel5678);

                                op += 0x20;
                            }

                            for (; x < width; x++)
                            {
                                Pixel* px = ip + (uint)x;

                                *(op + 0) = Downsample(px->B);
                                *(op + 1) = Downsample(px->G);
                                *(op + 2) = Downsample(px->R);
                                *(op + 3) = Downsample(px->A);

                                op += 4;
                            }

                            op += strideGap;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    int baseOffs = y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int offs = baseOffs + x * 4;

                        dst[offs + 0] = Downsample(input.GetB(x, y));
                        dst[offs + 1] = Downsample(input.GetG(x, y));
                        dst[offs + 2] = Downsample(input.GetR(x, y));
                        dst[offs + 3] = Downsample(input.GetA(x, y));
                    }
                }
            }

            bool outLinear = config.OutBlkKind == 0;

            int gobBlocksInY = 1 << config.OutBlkHeight;

            WriteBuffer(rm, dst, offsets.LumaOffset, outLinear, width, height, 4, gobBlocksInY);

            rm.BufferPool.Return(dstIndex);
        }

        private unsafe static void WriteNv12(ResourceManager rm, Surface input, ref OutputSurfaceConfig config, ref PlaneOffsets offsets)
        {
            int gobBlocksInY = 1 << config.OutBlkHeight;

            bool outLinear = config.OutBlkKind == 0;

            int width = Math.Min(config.OutLumaWidth + 1, input.Width);
            int height = Math.Min(config.OutLumaHeight + 1, input.Height);
            int yStride = GetPitch(config.OutLumaWidth + 1, 1);

            int dstYIndex = rm.BufferPool.Rent((config.OutLumaHeight + 1) * yStride, out Span<byte> dstY);

            if (Sse41.IsSupported)
            {
                Vector128<ushort> mask = Vector128.Create(0xffffUL).AsUInt16();

                int widthTrunc = width & ~0xf;
                int strideGap = yStride - width;

                fixed (Pixel* srcPtr = input.Data)
                {
                    Pixel* ip = srcPtr;

                    fixed (byte* dstPtr = dstY)
                    {
                        byte* op = dstPtr;

                        for (int y = 0; y < height; y++, ip += input.Width)
                        {
                            int x = 0;

                            for (; x < widthTrunc; x += 16)
                            {
                                byte* baseOffset = (byte*)(ip + (ulong)(uint)x);

                                Vector128<ushort> pixelp1 = Sse2.LoadVector128((ushort*)baseOffset);
                                Vector128<ushort> pixelp2 = Sse2.LoadVector128((ushort*)(baseOffset + 0x10));
                                Vector128<ushort> pixelp3 = Sse2.LoadVector128((ushort*)(baseOffset + 0x20));
                                Vector128<ushort> pixelp4 = Sse2.LoadVector128((ushort*)(baseOffset + 0x30));
                                Vector128<ushort> pixelp5 = Sse2.LoadVector128((ushort*)(baseOffset + 0x40));
                                Vector128<ushort> pixelp6 = Sse2.LoadVector128((ushort*)(baseOffset + 0x50));
                                Vector128<ushort> pixelp7 = Sse2.LoadVector128((ushort*)(baseOffset + 0x60));
                                Vector128<ushort> pixelp8 = Sse2.LoadVector128((ushort*)(baseOffset + 0x70));

                                pixelp1 = Sse2.And(pixelp1, mask);
                                pixelp2 = Sse2.And(pixelp2, mask);
                                pixelp3 = Sse2.And(pixelp3, mask);
                                pixelp4 = Sse2.And(pixelp4, mask);
                                pixelp5 = Sse2.And(pixelp5, mask);
                                pixelp6 = Sse2.And(pixelp6, mask);
                                pixelp7 = Sse2.And(pixelp7, mask);
                                pixelp8 = Sse2.And(pixelp8, mask);

                                Vector128<ushort> pixelq1 = Sse41.PackUnsignedSaturate(pixelp1.AsInt32(), pixelp2.AsInt32());
                                Vector128<ushort> pixelq2 = Sse41.PackUnsignedSaturate(pixelp3.AsInt32(), pixelp4.AsInt32());
                                Vector128<ushort> pixelq3 = Sse41.PackUnsignedSaturate(pixelp5.AsInt32(), pixelp6.AsInt32());
                                Vector128<ushort> pixelq4 = Sse41.PackUnsignedSaturate(pixelp7.AsInt32(), pixelp8.AsInt32());

                                pixelq1 = Sse41.PackUnsignedSaturate(pixelq1.AsInt32(), pixelq2.AsInt32());
                                pixelq2 = Sse41.PackUnsignedSaturate(pixelq3.AsInt32(), pixelq4.AsInt32());

                                pixelq1 = Sse2.ShiftRightLogical(pixelq1, 2);
                                pixelq2 = Sse2.ShiftRightLogical(pixelq2, 2);

                                Vector128<byte> pixel = Sse2.PackUnsignedSaturate(pixelq1.AsInt16(), pixelq2.AsInt16());

                                Sse2.Store(op, pixel);

                                op += 0x10;
                            }

                            for (; x < width; x++)
                            {
                                Pixel* px = ip + (uint)x;

                                *op++ = Downsample(px->R);
                            }

                            op += strideGap;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        dstY[y * yStride + x] = Downsample(input.GetR(x, y));
                    }
                }
            }

            WriteBuffer(
                rm,
                dstY,
                offsets.LumaOffset,
                outLinear,
                config.OutLumaWidth + 1,
                config.OutLumaHeight + 1,
                1,
                gobBlocksInY);

            rm.BufferPool.Return(dstYIndex);

            int uvWidth = Math.Min(config.OutChromaWidth + 1, (width + 1) >> 1);
            int uvHeight = Math.Min(config.OutChromaHeight + 1, (height + 1) >> 1);
            int uvStride = GetPitch(config.OutChromaWidth + 1, 2);

            int dstUvIndex = rm.BufferPool.Rent((config.OutChromaHeight + 1) * uvStride, out Span<byte> dstUv);

            if (Sse2.IsSupported)
            {
                int widthTrunc = uvWidth & ~7;
                int strideGap = uvStride - uvWidth * 2;

                fixed (Pixel* srcPtr = input.Data)
                {
                    Pixel* ip = srcPtr;

                    fixed (byte* dstPtr = dstUv)
                    {
                        byte* op = dstPtr;

                        for (int y = 0; y < uvHeight; y++, ip += input.Width * 2)
                        {
                            int x = 0;

                            for (; x < widthTrunc; x += 8)
                            {
                                byte* baseOffset = (byte*)ip + (ulong)(uint)x * 16;

                                Vector128<uint> pixel1 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x02));
                                Vector128<uint> pixel2 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x12));
                                Vector128<uint> pixel3 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x22));
                                Vector128<uint> pixel4 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x32));
                                Vector128<uint> pixel5 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x42));
                                Vector128<uint> pixel6 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x52));
                                Vector128<uint> pixel7 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x62));
                                Vector128<uint> pixel8 = Sse2.LoadScalarVector128((uint*)(baseOffset + 0x72));

                                Vector128<uint> pixel12 = Sse2.UnpackLow(pixel1, pixel2);
                                Vector128<uint> pixel34 = Sse2.UnpackLow(pixel3, pixel4);
                                Vector128<uint> pixel56 = Sse2.UnpackLow(pixel5, pixel6);
                                Vector128<uint> pixel78 = Sse2.UnpackLow(pixel7, pixel8);

                                Vector128<ulong> pixel1234 = Sse2.UnpackLow(pixel12.AsUInt64(), pixel34.AsUInt64());
                                Vector128<ulong> pixel5678 = Sse2.UnpackLow(pixel56.AsUInt64(), pixel78.AsUInt64());

                                pixel1234 = Sse2.ShiftRightLogical(pixel1234, 2);
                                pixel5678 = Sse2.ShiftRightLogical(pixel5678, 2);

                                Vector128<byte> pixel = Sse2.PackUnsignedSaturate(pixel1234.AsInt16(), pixel5678.AsInt16());

                                Sse2.Store(op, pixel);

                                op += 0x10;
                            }

                            for (; x < uvWidth; x++)
                            {
                                Pixel* px = ip + (uint)(x << 1);

                                *op++ = Downsample(px->G);
                                *op++ = Downsample(px->B);
                            }

                            op += strideGap;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < uvHeight; y++)
                {
                    for (int x = 0; x < uvWidth; x++)
                    {
                        int xx = x << 1;
                        int yy = y << 1;

                        int uvOffs = y * uvStride + xx;

                        dstUv[uvOffs + 0] = Downsample(input.GetG(xx, yy));
                        dstUv[uvOffs + 1] = Downsample(input.GetB(xx, yy));
                    }
                }
            }

            WriteBuffer(
                rm,
                dstUv,
                offsets.ChromaUOffset,
                outLinear,
                config.OutChromaWidth + 1,
                config.OutChromaHeight + 1, 2,
                gobBlocksInY);

            rm.BufferPool.Return(dstUvIndex);
        }

        private static void WriteBuffer(
            ResourceManager rm,
            ReadOnlySpan<byte> src,
            uint offset,
            bool linear,
            int width,
            int height,
            int bytesPerPixel,
            int gobBlocksInY)
        {
            if (linear)
            {
                rm.Gmm.WriteMapped(ExtendOffset(offset), src);
                return;
            }

            WriteBuffer(rm, src, offset, width, height, bytesPerPixel, gobBlocksInY);
        }

        private static void WriteBuffer(
            ResourceManager rm,
            ReadOnlySpan<byte> src,
            uint offset,
            int width,
            int height,
            int bytesPerPixel,
            int gobBlocksInY)
        {
            int outSize = GetBlockLinearSize(width, height, bytesPerPixel, gobBlocksInY);
            int dstStride = GetPitch(width, bytesPerPixel);

            int dstIndex = rm.BufferPool.Rent(outSize, out Span<byte> dst);

            LayoutConverter.ConvertLinearToBlockLinear(dst, width, height, dstStride, bytesPerPixel, gobBlocksInY, src);

            rm.Gmm.WriteMapped(ExtendOffset(offset), dst);

            rm.BufferPool.Return(dstIndex);
        }
    }
}
