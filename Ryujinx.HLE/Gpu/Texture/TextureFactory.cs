using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Gpu.Memory;
using System;

namespace Ryujinx.HLE.Gpu.Texture
{
    static class TextureFactory
    {
        public static GalImage MakeTexture(NvGpuVmm Vmm, long TicPosition)
        {
            int[] Tic = ReadWords(Vmm, TicPosition, 8);

            GalTextureType RType = (GalTextureType)((Tic[0] >> 7)  & 7);
            GalTextureType GType = (GalTextureType)((Tic[0] >> 10) & 7);
            GalTextureType BType = (GalTextureType)((Tic[0] >> 13) & 7);
            GalTextureType AType = (GalTextureType)((Tic[0] >> 16) & 7);

            GalImageFormat Format = ImageFormatConverter.ConvertTexture((GalTextureFormat)(Tic[0] & 0x7f), RType, GType, BType, AType);

            GalTextureSource XSource = (GalTextureSource)((Tic[0] >> 19) & 7);
            GalTextureSource YSource = (GalTextureSource)((Tic[0] >> 22) & 7);
            GalTextureSource ZSource = (GalTextureSource)((Tic[0] >> 25) & 7);
            GalTextureSource WSource = (GalTextureSource)((Tic[0] >> 28) & 7);

            int Width  = (Tic[4] & 0xffff) + 1;
            int Height = (Tic[5] & 0xffff) + 1;

            return new GalImage(
                Width,
                Height,
                Format,
                XSource,
                YSource,
                ZSource,
                WSource);
        }

        public static byte[] GetTextureData(NvGpuVmm Vmm, long TicPosition)
        {
            int[] Tic = ReadWords(Vmm, TicPosition, 8);

            GalTextureFormat Format = (GalTextureFormat)(Tic[0] & 0x7f);

            long TextureAddress = (uint)Tic[1];

            TextureAddress |= (long)((ushort)Tic[2]) << 32;

            TextureSwizzle Swizzle = (TextureSwizzle)((Tic[2] >> 21) & 7);

            if (Swizzle == TextureSwizzle.BlockLinear ||
                Swizzle == TextureSwizzle.BlockLinearColorKey)
            {
                TextureAddress &= ~0x1ffL;
            }
            else if (Swizzle == TextureSwizzle.Pitch ||
                     Swizzle == TextureSwizzle.PitchColorKey)
            {
                TextureAddress &= ~0x1fL;
            }

            int Pitch = (Tic[3] & 0xffff) << 5;

            int BlockHeightLog2 = (Tic[3] >> 3)  & 7;
            int TileWidthLog2   = (Tic[3] >> 10) & 7;

            int BlockHeight = 1 << BlockHeightLog2;
            int TileWidth   = 1 << TileWidthLog2;

            int Width  = (Tic[4] & 0xffff) + 1;
            int Height = (Tic[5] & 0xffff) + 1;

            TextureInfo Texture = new TextureInfo(
                TextureAddress,
                Width,
                Height,
                Pitch,
                BlockHeight,
                TileWidth,
                Swizzle,
                Format);

            return TextureReader.Read(Vmm, Texture);
        }

        public static GalTextureSampler MakeSampler(NvGpu Gpu, NvGpuVmm Vmm, long TscPosition)
        {
            int[] Tsc = ReadWords(Vmm, TscPosition, 8);

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

        private static int[] ReadWords(NvGpuVmm Vmm, long Position, int Count)
        {
            int[] Words = new int[Count];

            for (int Index = 0; Index < Count; Index++, Position += 4)
            {
                Words[Index] = Vmm.ReadInt32(Position);
            }

            return Words;
        }
    }
}