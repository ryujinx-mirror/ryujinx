using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Graphics.Texture
{
    delegate byte[] TextureReaderDelegate(IAMemory Memory, TextureInfo Texture);

    public static class TextureReader
    {
        public static byte[] Read(IAMemory Memory, TextureInfo Texture)
        {
            TextureReaderDelegate Reader = ImageUtils.GetReader(Texture.Format);

            return Reader(Memory, Texture);
        }

        internal unsafe static byte[] Read1Bpp(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 1);

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

                    byte Pixel = CpuMem.ReadByte(Position + Offset);

                    *(BuffPtr + OutOffs) = Pixel;

                    OutOffs++;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read5551(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 2);

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

                    uint Pixel = (uint)CpuMem.ReadInt16(Position + Offset);

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

        internal unsafe static byte[] Read565(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 2);

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

                    uint Pixel = (uint)CpuMem.ReadInt16(Position + Offset);

                    Pixel = (Pixel & 0x001f) << 11 |
                            (Pixel & 0x07e0)       |
                            (Pixel & 0xf800) >> 11;

                    *(short*)(BuffPtr + OutOffs) = (short)Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read2Bpp(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 2);

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

                    short Pixel = CpuMem.ReadInt16(Position + Offset);

                    *(short*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read4Bpp(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 4];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 4);

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

                    int Pixel = CpuMem.ReadInt32(Position + Offset);

                    *(int*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 4;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read8Bpp(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 8];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 8);

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

                    long Pixel = CpuMem.ReadInt64(Position + Offset);

                    *(long*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 8;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read16Bpp(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 16];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 16);

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

                    long PxLow  = CpuMem.ReadInt64(Position + Offset + 0);
                    long PxHigh = CpuMem.ReadInt64(Position + Offset + 8);

                    *(long*)(BuffPtr + OutOffs + 0) = PxLow;
                    *(long*)(BuffPtr + OutOffs + 8) = PxHigh;

                    OutOffs += 16;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read8Bpt4x4(IAMemory Memory, TextureInfo Texture)
        {
            int Width  = (Texture.Width  + 3) / 4;
            int Height = (Texture.Height + 3) / 4;

            byte[] Output = new byte[Width * Height * 8];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 4, 8);

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

                    long Tile = CpuMem.ReadInt64(Position + Offset);

                    *(long*)(BuffPtr + OutOffs) = Tile;

                    OutOffs += 8;
                }
            }

            return Output;
        }

        internal unsafe static byte[] Read16BptCompressedTexture(IAMemory Memory, TextureInfo Texture, int BlockWidth, int BlockHeight)
        {
            int Width  = (Texture.Width  + (BlockWidth - 1)) / BlockWidth;
            int Height = (Texture.Height + (BlockHeight - 1)) / BlockHeight;

            byte[] Output = new byte[Width * Height * 16];

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, BlockWidth, 16);

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

                    long Tile0 = CpuMem.ReadInt64(Position + Offset + 0);
                    long Tile1 = CpuMem.ReadInt64(Position + Offset + 8);

                    *(long*)(BuffPtr + OutOffs + 0) = Tile0;
                    *(long*)(BuffPtr + OutOffs + 8) = Tile1;

                    OutOffs += 16;
                }
            }

            return Output;
        }

        internal static byte[] Read16BptCompressedTexture4x4(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 4, 4);
        }

        internal static byte[] Read16BptCompressedTexture5x5(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 5, 5);
        }

        internal static byte[] Read16BptCompressedTexture6x6(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 6, 6);
        }

        internal static byte[] Read16BptCompressedTexture8x8(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 8, 8);
        }

        internal static byte[] Read16BptCompressedTexture10x10(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 10, 10);
        }

        internal static byte[] Read16BptCompressedTexture12x12(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 12, 12);
        }

        internal static byte[] Read16BptCompressedTexture5x4(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 5, 4);
        }

        internal static byte[] Read16BptCompressedTexture6x5(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 6, 5);
        }

        internal static byte[] Read16BptCompressedTexture8x6(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 8, 6);
        }

        internal static byte[] Read16BptCompressedTexture10x8(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 10, 8);
        }

        internal static byte[] Read16BptCompressedTexture12x10(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 12, 10);
        }

        internal static byte[] Read16BptCompressedTexture8x5(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 5, 5);
        }

        internal static byte[] Read16BptCompressedTexture10x5(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 10, 5);
        }

        internal static byte[] Read16BptCompressedTexture10x6(IAMemory Memory, TextureInfo Texture)
        {
            return Read16BptCompressedTexture(Memory, Texture, 10, 6);
        }
    }
}
