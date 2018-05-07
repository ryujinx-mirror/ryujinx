using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Core.Gpu
{
    public static class TextureWriter
    {
        public static void Write(IAMemory Memory, Texture Texture, byte[] Data)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.A8B8G8R8: Write4Bpp(Memory, Texture, Data); break;

                default: throw new NotImplementedException(Texture.Format.ToString());
            }
        }

        private unsafe static void Write4Bpp(IAMemory Memory, Texture Texture, byte[] Data)
        {
            int Width  = Texture.Width;
            int Height = Texture.Height;

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, Width, 4);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Data)
            {
                long InOffs = 0;

                for (int Y = 0; Y < Height; Y++)
                for (int X = 0; X < Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    int Pixel = *(int*)(BuffPtr + InOffs);

                    CpuMem.WriteInt32Unchecked(Position + Offset, Pixel);

                    InOffs += 4;
                }
            }
        }
    }
}
