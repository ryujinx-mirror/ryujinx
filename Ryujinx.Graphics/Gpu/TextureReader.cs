using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Graphics.Gpu
{
    static class TextureReader
    {
        public static byte[] Read(AMemory Memory, Texture Texture)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.A8B8G8R8: return Read4Bpp    (Memory, Texture);
                case GalTextureFormat.A1B5G5R5: return Read2Bpp    (Memory, Texture);
                case GalTextureFormat.B5G6R5:   return Read2Bpp    (Memory, Texture);
                case GalTextureFormat.BC1:      return Read8Bpt4x4 (Memory, Texture);
                case GalTextureFormat.BC2:      return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.BC3:      return Read16Bpt4x4(Memory, Texture);
                case GalTextureFormat.BC4:      return Read8Bpt4x4 (Memory, Texture);
                case GalTextureFormat.BC5:      return Read16Bpt4x4(Memory, Texture);
            }

            throw new NotImplementedException(Texture.Format.ToString());
        }

        private unsafe static byte[] Read2Bpp(AMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 2];

            ISwizzle Swizzle = GetSwizzle(Texture, Width, 2);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    short Pixel = Memory.ReadInt16Unchecked(Texture.Position + Offset);

                    *(short*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 2;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read4Bpp(AMemory Memory, Texture Texture)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            byte[] Output = new byte[Width * Height * 4];

            ISwizzle Swizzle = GetSwizzle(Texture, Width, 4);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    int Pixel = Memory.ReadInt32Unchecked(Texture.Position + Offset);

                    *(int*)(BuffPtr + OutOffs) = Pixel;

                    OutOffs += 4;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read8Bpt4x4(AMemory Memory, Texture Texture)
        {
            int Width  = (Texture.Width  + 3) / 4;
            int Height = (Texture.Height + 3) / 4;

            byte[] Output = new byte[Width * Height * 8];

            ISwizzle Swizzle = GetSwizzle(Texture, Width, 8);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long Tile = Memory.ReadInt64Unchecked(Texture.Position + Offset);

                    *(long*)(BuffPtr + OutOffs) = Tile;

                    OutOffs += 8;
                }
            }

            return Output;
        }

        private unsafe static byte[] Read16Bpt4x4(AMemory Memory, Texture Texture)
        {
            int Width  = (Texture.Width  + 3) / 4;
            int Height = (Texture.Height + 3) / 4;

            byte[] Output = new byte[Width * Height * 16];

            ISwizzle Swizzle = GetSwizzle(Texture, Width, 16);

            fixed (byte* BuffPtr = Output)
            {
                long OutOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    long Tile0 = Memory.ReadInt64Unchecked(Texture.Position + Offset + 0);
                    long Tile1 = Memory.ReadInt64Unchecked(Texture.Position + Offset + 8);

                    *(long*)(BuffPtr + OutOffs + 0) = Tile0;
                    *(long*)(BuffPtr + OutOffs + 8) = Tile1;

                    OutOffs += 16;
                }
            }

            return Output;
        }

        private static ISwizzle GetSwizzle(Texture Texture, int Width, int Bpp)
        {
            switch (Texture.Swizzle)
            {
                case TextureSwizzle.Pitch:
                case TextureSwizzle.PitchColorKey:
                     return new LinearSwizzle(Texture.Pitch, Bpp);

                case TextureSwizzle.BlockLinear:
                case TextureSwizzle.BlockLinearColorKey:
                    return new BlockLinearSwizzle(Width, Bpp, Texture.BlockHeight);
            }

            throw new NotImplementedException(Texture.Swizzle.ToString());
        }
    }
}
