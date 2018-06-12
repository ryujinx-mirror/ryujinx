using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.HLE.Gpu
{
    static class TextureReader
    {
        public static byte[] Read(IAMemory Memory, Texture Texture)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.R32G32B32A32: return Read16Bpp   (Memory, Texture);
                case GalTextureFormat.R16G16B16A16: return Read8Bpp    (Memory, Texture);
                case GalTextureFormat.A8B8G8R8:     return Read4Bpp    (Memory, Texture);
                case GalTextureFormat.R32:          return Read4Bpp    (Memory, Texture);
                case GalTextureFormat.A1B5G5R5:     return Read5551    (Memory, Texture);
                case GalTextureFormat.B5G6R5:       return Read565     (Memory, Texture);
                case GalTextureFormat.G8R8:         return Read2Bpp    (Memory, Texture);
                case GalTextureFormat.R16:          return Read2Bpp    (Memory, Texture);
                case GalTextureFormat.R8:           return Read1Bpp    (Memory, Texture);
                case GalTextureFormat.BC7U:         return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.BC1:          return Read8Bpt4x4 (Memory, Texture);
                case GalTextureFormat.BC2:          return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.BC3:          return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.BC4:          return Read8Bpt4x4 (Memory, Texture);
                case GalTextureFormat.BC5:          return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.Astc2D4x4:    return Read16Bpt4x4(Memory, Texture);
            }

            throw new NotImplementedException(Texture.Format.ToString());
        }

        private unsafe static byte[] Read1Bpp(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 1);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    byte Pixel = CpuMem.ReadByteUnchecked(Position + Offset);

                    *(BuffPtr + OutOffs) = Pixel;

                    OutOffs++;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read5551(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 2);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    uint Pixel = (uint)CpuMem.ReadInt16Unchecked(Position + Offset);

                    Pixel = (Pixel & 0x001f) << 11 |
                            (Pixel & 0x03e0) << 1  |
                            (Pixel & 0x7c00) >> 9  |
                            (Pixel & 0x8000) >> 15;

                    *(short*)(BuffPtr + OutOffs) = (short)Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read565(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 2);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    uint Pixel = (uint)CpuMem.ReadInt16Unchecked(Position + Offset);

                    Pixel = (Pixel & 0x001f) << 11 |
                            (Pixel & 0x07e0)       |
                            (Pixel & 0xf800) >> 11;

                    *(short*)(BuffPtr + OutOffs) = (short)Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read2Bpp(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 2);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    short Pixel = CpuMem.ReadInt16Unchecked(Position + Offset);

                    *(short*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read4Bpp(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 4];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 4);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    int Pixel = CpuMem.ReadInt32Unchecked(Position + Offset);

                    *(int*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 4;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read8Bpp(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 8];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 8);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long Pixel = CpuMem.ReadInt64Unchecked(Position + Offset);

                    *(long*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 8;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read16Bpp(IAMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 16];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 16);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long PxLow  = CpuMem.ReadInt64Unchecked(Position + Offset + 0);
                    long PxHigh = CpuMem.ReadInt64Unchecked(Position + Offset + 8);

                    *(long*)(BuffPtr + OutOffs + 0) = PxLow;
                    *(long*)(BuffPtr + OutOffs + 8) = PxHigh;

                    OutOffs += 16;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read8Bpt4x4(IAMemory Memory, Texture Texture)
        {
            int Width  = (Texture.Width  + 3) / 4;
            int Height = (Texture.Height + 3) / 4;

            byte[] Output = new byte[Width * Height * 8];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 8);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long Tile = CpuMem.ReadInt64Unchecked(Position + Offset);

                    *(long*)(BuffPtr + OutOffs) = Tile;

                    OutOffs += 8;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read16Bpt4x4(IAMemory Memory, Texture Texture)
        {
            int Width  = (Texture.Width  + 3) / 4;
            int Height = (Texture.Height + 3) / 4;

            byte[] Output = new byte[Width * Height * 16];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 16);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long Tile0 = CpuMem.ReadInt64Unchecked(Position + Offset + 0);
                    long Tile1 = CpuMem.ReadInt64Unchecked(Position + Offset + 8);

                    *(long*)(BuffPtr + OutOffs + 0) = Tile0;
                    *(long*)(BuffPtr + OutOffs + 8) = Tile1;

                    OutOffs += 16;
                }
            }

            return Output;
        }
    }
}
