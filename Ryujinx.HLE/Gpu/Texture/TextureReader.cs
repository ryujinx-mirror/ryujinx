using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.HLE.Gpu.Texture
{
    static class TextureReader
    {
        public static byte[] Read(IAMemory Memory, TextureInfo Texture)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.R32G32B32A32: return Read16Bpp                 (Memory, Texture);
                case GalTextureFormat.R16G16B16A16: return Read8Bpp                  (Memory, Texture);
                case GalTextureFormat.A8B8G8R8:     return Read4Bpp                  (Memory, Texture);
                case GalTextureFormat.R32:          return Read4Bpp                  (Memory, Texture);
                case GalTextureFormat.BF10GF11RF11: return Read4Bpp                  (Memory, Texture);
                case GalTextureFormat.Z24S8:        return Read4Bpp                  (Memory, Texture);
                case GalTextureFormat.A1B5G5R5:     return Read5551                  (Memory, Texture);
                case GalTextureFormat.B5G6R5:       return Read565                   (Memory, Texture);
                case GalTextureFormat.G8R8:         return Read2Bpp                  (Memory, Texture);
                case GalTextureFormat.R16:          return Read2Bpp                  (Memory, Texture);
                case GalTextureFormat.R8:           return Read1Bpp                  (Memory, Texture);
                case GalTextureFormat.BC6H_SF16:    return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.BC6H_UF16:    return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.BC7U:         return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.BC1:          return Read8Bpt4x4               (Memory, Texture);
                case GalTextureFormat.BC2:          return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.BC3:          return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.BC4:          return Read8Bpt4x4               (Memory, Texture);
                case GalTextureFormat.BC5:          return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.ZF32:         return Read4Bpp                  (Memory, Texture);
                case GalTextureFormat.Astc2D4x4:    return Read16BptCompressedTexture(Memory, Texture, 4, 4);
                case GalTextureFormat.Astc2D5x5:    return Read16BptCompressedTexture(Memory, Texture, 5, 5);
                case GalTextureFormat.Astc2D6x6:    return Read16BptCompressedTexture(Memory, Texture, 6, 6);
                case GalTextureFormat.Astc2D8x8:    return Read16BptCompressedTexture(Memory, Texture, 8, 8);
                case GalTextureFormat.Astc2D10x10:  return Read16BptCompressedTexture(Memory, Texture, 10, 10);
                case GalTextureFormat.Astc2D12x12:  return Read16BptCompressedTexture(Memory, Texture, 12, 12);
                case GalTextureFormat.Astc2D5x4:    return Read16BptCompressedTexture(Memory, Texture, 5, 4);
                case GalTextureFormat.Astc2D6x5:    return Read16BptCompressedTexture(Memory, Texture, 6, 5);
                case GalTextureFormat.Astc2D8x6:    return Read16BptCompressedTexture(Memory, Texture, 8, 6);
                case GalTextureFormat.Astc2D10x8:   return Read16BptCompressedTexture(Memory, Texture, 10, 8);
                case GalTextureFormat.Astc2D12x10:  return Read16BptCompressedTexture(Memory, Texture, 12, 10);
                case GalTextureFormat.Astc2D8x5:    return Read16BptCompressedTexture(Memory, Texture, 8, 5);
                case GalTextureFormat.Astc2D10x5:   return Read16BptCompressedTexture(Memory, Texture, 10, 5);
                case GalTextureFormat.Astc2D10x6:   return Read16BptCompressedTexture(Memory, Texture, 10, 6);
             }

            throw new NotImplementedException(Texture.Format.ToString());
        }

        private unsafe static byte[] Read1Bpp(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read5551(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read565(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read2Bpp(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read4Bpp(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read8Bpp(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read16Bpp(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read8Bpt4x4(IAMemory Memory, TextureInfo Texture)
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

        private unsafe static byte[] Read16BptCompressedTexture(IAMemory Memory, TextureInfo Texture, int BlockWidth, int BlockHeight)
        {
            int Width  = (Texture.Width  + (BlockWidth - 1)) / BlockWidth;
            int Height = (Texture.Height + (BlockHeight - 1)) / BlockHeight;

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
