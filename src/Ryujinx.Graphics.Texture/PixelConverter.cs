using Ryujinx.Common;
using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Texture
{
    public static class PixelConverter
    {
        private static (int remainder, int outRemainder, int height) GetLineRemainders(int length, int width, int bpp, int outBpp)
        {
            int stride = BitUtils.AlignUp(width * bpp, LayoutConverter.HostStrideAlignment);
            int remainder = stride / bpp - width;

            int outStride = BitUtils.AlignUp(width * outBpp, LayoutConverter.HostStrideAlignment);
            int outRemainder = outStride / outBpp - width;

            return (remainder, outRemainder, length / stride);
        }

        public unsafe static MemoryOwner<byte> ConvertR4G4ToR4G4B4A4(ReadOnlySpan<byte> data, int width)
        {
            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(data.Length * 2);
            Span<byte> outputSpan = output.Span;

            (int remainder, int outRemainder, int height) = GetLineRemainders(data.Length, width, 1, 2);

            Span<ushort> outputSpanUInt16 = MemoryMarshal.Cast<byte, ushort>(outputSpan);

            if (remainder == 0)
            {
                int start = 0;

                if (Sse41.IsSupported)
                {
                    int sizeTrunc = data.Length & ~7;
                    start = sizeTrunc;

                    fixed (byte* inputPtr = data, outputPtr = outputSpan)
                    {
                        for (ulong offset = 0; offset < (ulong)sizeTrunc; offset += 8)
                        {
                            Sse2.Store(outputPtr + offset * 2, Sse41.ConvertToVector128Int16(inputPtr + offset).AsByte());
                        }
                    }
                }

                for (int i = start; i < data.Length; i++)
                {
                    outputSpanUInt16[i] = data[i];
                }
            }
            else
            {
                int offset = 0;
                int outOffset = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        outputSpanUInt16[outOffset++] = data[offset++];
                    }

                    offset += remainder;
                    outOffset += outRemainder;
                }
            }

            return output;
        }

        public static MemoryOwner<byte> ConvertR5G6B5ToR8G8B8A8(ReadOnlySpan<byte> data, int width)
        {
            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(data.Length * 2);
            int offset = 0;
            int outOffset = 0;

            (int remainder, int outRemainder, int height) = GetLineRemainders(data.Length, width, 2, 4);

            ReadOnlySpan<ushort> inputSpan = MemoryMarshal.Cast<byte, ushort>(data);
            Span<uint> outputSpan = MemoryMarshal.Cast<byte, uint>(output.Span);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint packed = inputSpan[offset++];

                    uint outputPacked = 0xff000000;
                    outputPacked |= (packed << 3) & 0x000000f8;
                    outputPacked |= (packed << 8) & 0x00f80000;

                    // Replicate 5 bit components.
                    outputPacked |= (outputPacked >> 5) & 0x00070007;

                    // Include and replicate 6 bit component.
                    outputPacked |= ((packed << 5) & 0x0000fc00) | ((packed >> 1) & 0x00000300);

                    outputSpan[outOffset++] = outputPacked;
                }

                offset += remainder;
                outOffset += outRemainder;
            }

            return output;
        }

        public static MemoryOwner<byte> ConvertR5G5B5ToR8G8B8A8(ReadOnlySpan<byte> data, int width, bool forceAlpha)
        {
            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(data.Length * 2);
            int offset = 0;
            int outOffset = 0;

            (int remainder, int outRemainder, int height) = GetLineRemainders(data.Length, width, 2, 4);

            ReadOnlySpan<ushort> inputSpan = MemoryMarshal.Cast<byte, ushort>(data);
            Span<uint> outputSpan = MemoryMarshal.Cast<byte, uint>(output.Span);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint packed = inputSpan[offset++];

                    uint a = forceAlpha ? 1 : (packed >> 15);

                    uint outputPacked = a * 0xff000000;
                    outputPacked |= (packed << 3) & 0x000000f8;
                    outputPacked |= (packed << 6) & 0x0000f800;
                    outputPacked |= (packed << 9) & 0x00f80000;

                    // Replicate 5 bit components.
                    outputPacked |= (outputPacked >> 5) & 0x00070707;

                    outputSpan[outOffset++] = outputPacked;
                }

                offset += remainder;
                outOffset += outRemainder;
            }

            return output;
        }

        public static MemoryOwner<byte> ConvertA1B5G5R5ToR8G8B8A8(ReadOnlySpan<byte> data, int width)
        {
            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(data.Length * 2);
            int offset = 0;
            int outOffset = 0;

            (int remainder, int outRemainder, int height) = GetLineRemainders(data.Length, width, 2, 4);

            ReadOnlySpan<ushort> inputSpan = MemoryMarshal.Cast<byte, ushort>(data);
            Span<uint> outputSpan = MemoryMarshal.Cast<byte, uint>(output.Span);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint packed = inputSpan[offset++];

                    uint a = packed >> 15;

                    uint outputPacked = a * 0xff000000;
                    outputPacked |= (packed >> 8) & 0x000000f8;
                    outputPacked |= (packed << 5) & 0x0000f800;
                    outputPacked |= (packed << 18) & 0x00f80000;

                    // Replicate 5 bit components.
                    outputPacked |= (outputPacked >> 5) & 0x00070707;

                    outputSpan[outOffset++] = outputPacked;
                }

                offset += remainder;
                outOffset += outRemainder;
            }

            return output;
        }

        public static MemoryOwner<byte> ConvertR4G4B4A4ToR8G8B8A8(ReadOnlySpan<byte> data, int width)
        {
            MemoryOwner<byte> output = MemoryOwner<byte>.Rent(data.Length * 2);
            int offset = 0;
            int outOffset = 0;

            (int remainder, int outRemainder, int height) = GetLineRemainders(data.Length, width, 2, 4);

            ReadOnlySpan<ushort> inputSpan = MemoryMarshal.Cast<byte, ushort>(data);
            Span<uint> outputSpan = MemoryMarshal.Cast<byte, uint>(output.Span);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint packed = inputSpan[offset++];

                    uint outputPacked = packed & 0x0000000f;
                    outputPacked |= (packed << 4) & 0x00000f00;
                    outputPacked |= (packed << 8) & 0x000f0000;
                    outputPacked |= (packed << 12) & 0x0f000000;

                    outputSpan[outOffset++] = outputPacked * 0x11;
                }

                offset += remainder;
                outOffset += outRemainder;
            }

            return output;
        }
    }
}
