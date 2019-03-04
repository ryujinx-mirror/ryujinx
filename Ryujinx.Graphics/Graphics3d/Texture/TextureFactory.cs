using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using System;

namespace Ryujinx.Graphics.Texture
{
    static class TextureFactory
    {
        public static GalImage MakeTexture(NvGpuVmm vmm, long ticPosition)
        {
            int[] tic = ReadWords(vmm, ticPosition, 8);

            GalImageFormat format = GetImageFormat(tic);

            GalTextureTarget textureTarget = (GalTextureTarget)((tic[4] >> 23) & 0xF);

            GalTextureSource xSource = (GalTextureSource)((tic[0] >> 19) & 7);
            GalTextureSource ySource = (GalTextureSource)((tic[0] >> 22) & 7);
            GalTextureSource zSource = (GalTextureSource)((tic[0] >> 25) & 7);
            GalTextureSource wSource = (GalTextureSource)((tic[0] >> 28) & 7);

            TextureSwizzle swizzle = (TextureSwizzle)((tic[2] >> 21) & 7);

            int maxMipmapLevel = (tic[3] >> 28) & 0xF + 1;

            GalMemoryLayout layout;

            if (swizzle == TextureSwizzle.BlockLinear ||
                swizzle == TextureSwizzle.BlockLinearColorKey)
            {
                layout = GalMemoryLayout.BlockLinear;
            }
            else
            {
                layout = GalMemoryLayout.Pitch;
            }

            int gobBlockHeightLog2 = (tic[3] >> 3)  & 7;
            int gobBlockDepthLog2  = (tic[3] >> 6)  & 7;
            int tileWidthLog2      = (tic[3] >> 10) & 7;

            int gobBlockHeight = 1 << gobBlockHeightLog2;
            int gobBlockDepth  = 1 << gobBlockDepthLog2;
            int tileWidth      = 1 << tileWidthLog2;

            int width  = ((tic[4] >> 0)  & 0xffff) + 1;
            int height = ((tic[5] >> 0)  & 0xffff) + 1;
            int depth  = ((tic[5] >> 16) & 0x3fff) + 1;

            int layoutCount = 1;

            // TODO: check this
            if (ImageUtils.IsArray(textureTarget))
            {
                layoutCount = depth;
                depth = 1;
            }

            if (textureTarget == GalTextureTarget.OneD)
            {
                height = 1;
            }

            if (textureTarget == GalTextureTarget.TwoD || textureTarget == GalTextureTarget.OneD)
            {
                depth = 1;
            }
            else if (textureTarget == GalTextureTarget.CubeMap)
            {
                // FIXME: This is a bit hacky but I guess it's fine for now
                layoutCount = 6;
                depth = 1;
            }
            else if (textureTarget == GalTextureTarget.CubeArray)
            {
                // FIXME: This is a really really hacky but I guess it's fine for now
                layoutCount *= 6;
                depth = 1;
            }

            GalImage image = new GalImage(
                width,
                height,
                depth,
                layoutCount,
                tileWidth,
                gobBlockHeight,
                gobBlockDepth,
                layout,
                format,
                textureTarget,
                maxMipmapLevel,
                xSource,
                ySource,
                zSource,
                wSource);

            if (layout == GalMemoryLayout.Pitch)
            {
                image.Pitch = (tic[3] & 0xffff) << 5;
            }

            return image;
        }

        public static GalTextureSampler MakeSampler(NvGpu gpu, NvGpuVmm vmm, long tscPosition)
        {
            int[] tsc = ReadWords(vmm, tscPosition, 8);

            GalTextureWrap addressU = (GalTextureWrap)((tsc[0] >> 0) & 7);
            GalTextureWrap addressV = (GalTextureWrap)((tsc[0] >> 3) & 7);
            GalTextureWrap addressP = (GalTextureWrap)((tsc[0] >> 6) & 7);

            bool depthCompare = ((tsc[0] >> 9) & 1) == 1;

            DepthCompareFunc depthCompareFunc = (DepthCompareFunc)((tsc[0] >> 10) & 7);

            GalTextureFilter    magFilter = (GalTextureFilter)   ((tsc[1] >> 0) & 3);
            GalTextureFilter    minFilter = (GalTextureFilter)   ((tsc[1] >> 4) & 3);
            GalTextureMipFilter mipFilter = (GalTextureMipFilter)((tsc[1] >> 6) & 3);

            GalColorF borderColor = new GalColorF(
                BitConverter.Int32BitsToSingle(tsc[4]),
                BitConverter.Int32BitsToSingle(tsc[5]),
                BitConverter.Int32BitsToSingle(tsc[6]),
                BitConverter.Int32BitsToSingle(tsc[7]));

            return new GalTextureSampler(
                addressU,
                addressV,
                addressP,
                minFilter,
                magFilter,
                mipFilter,
                borderColor,
                depthCompare,
                depthCompareFunc);
        }

        private static GalImageFormat GetImageFormat(int[] tic)
        {
            GalTextureType rType = (GalTextureType)((tic[0] >> 7)  & 7);
            GalTextureType gType = (GalTextureType)((tic[0] >> 10) & 7);
            GalTextureType bType = (GalTextureType)((tic[0] >> 13) & 7);
            GalTextureType aType = (GalTextureType)((tic[0] >> 16) & 7);

            GalTextureFormat format = (GalTextureFormat)(tic[0] & 0x7f);

            bool convSrgb = ((tic[4] >> 22) & 1) != 0;

            return ImageUtils.ConvertTexture(format, rType, gType, bType, aType, convSrgb);
        }

        private static int[] ReadWords(NvGpuVmm vmm, long position, int count)
        {
            int[] words = new int[count];

            for (int index = 0; index < count; index++, position += 4)
            {
                words[index] = vmm.ReadInt32(position);
            }

            return words;
        }
    }
}