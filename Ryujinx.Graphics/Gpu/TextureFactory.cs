using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Graphics.Gpu
{
    static class TextureFactory
    {
        public static GalTexture MakeTexture(NsGpu Gpu, AMemory Memory, long TicPosition)
        {
            int[] Tic = ReadWords(Memory, TicPosition, 8);

            GalTextureFormat Format = (GalTextureFormat)(Tic[0] & 0x7f);

            long TextureAddress = (uint)Tic[1];

            TextureAddress |= (long)((ushort)Tic[2]) << 32;

            TextureAddress = Gpu.GetCpuAddr(TextureAddress);

            TextureSwizzle Swizzle = (TextureSwizzle)((Tic[2] >> 21) & 7);

            int BlockHeightLog2 = (Tic[3] >> 3) & 7;

            int BlockHeight = 1 << BlockHeightLog2;

            int Width  = (Tic[4] & 0xffff) + 1;
            int Height = (Tic[5] & 0xffff) + 1;

            Texture Texture = new Texture(
                TextureAddress,
                Width,
                Height,
                BlockHeight,
                Swizzle,
                Format);

            byte[] Data = TextureReader.Read(Memory, Texture);

            return new GalTexture(Data, Width, Height, Format);
        }

        public static GalTextureSampler MakeSampler(NsGpu Gpu, AMemory Memory, long TscPosition)
        {
            int[] Tsc = ReadWords(Memory, TscPosition, 8);

            GalTextureWrap AddressU = (GalTextureWrap)((Tsc[0] >> 0) & 7);
            GalTextureWrap AddressV = (GalTextureWrap)((Tsc[0] >> 3) & 7);
            GalTextureWrap AddressP = (GalTextureWrap)((Tsc[0] >> 6) & 7);

            GalTextureFilter    MagFilter = (GalTextureFilter)   ((Tsc[1] >> 0) & 3);
            GalTextureFilter    MinFilter = (GalTextureFilter)   ((Tsc[1] >> 4) & 3);
            GalTextureMipFilter MipFilter = (GalTextureMipFilter)((Tsc[1] >> 6) & 3);

            GalColorF BorderColor = new GalColorF(
                BitConverter.Int32BitsToSingle(Tsc[4]),
                BitConverter.Int32BitsToSingle(Tsc[5]),
                BitConverter.Int32BitsToSingle(Tsc[6]),
                BitConverter.Int32BitsToSingle(Tsc[7]));

            return new GalTextureSampler(
                AddressU,
                AddressV,
                AddressP,
                MinFilter,
                MagFilter,
                MipFilter,
                BorderColor);
        }

        private static int[] ReadWords(AMemory Memory, long Position, int Count)
        {
            int[] Words = new int[Count];

            for (int Index = 0; Index < Count; Index++, Position += 4)
            {
                Words[Index] = Memory.ReadInt32(Position);
            }

            return Words;
        }
    }
}