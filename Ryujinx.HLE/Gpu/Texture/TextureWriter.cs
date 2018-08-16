using ChocolArm64.Memory;

namespace Ryujinx.HLE.Gpu.Texture
{
    static class TextureWriter
    {
        public unsafe static void Write(IAMemory Memory, TextureInfo Texture, byte[] Data)
        {
            ISwizzle Swizzle = TextureHelper.GetSwizzle(Texture, 1, 4);

            (AMemory CpuMem, long Position) = TextureHelper.GetMemoryAndPosition(
                Memory,
                Texture.Position);

            fixed (byte* BuffPtr = Data)
            {
                long InOffs = 0;

                for (int Y = 0; Y < Texture.Height; Y++)
                for (int X = 0; X < Texture.Width;  X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    int Pixel = *(int*)(BuffPtr + InOffs);

                    CpuMem.WriteInt32(Position + Offset, Pixel);

                    InOffs += 4;
                }
            }
        }
    }
}
