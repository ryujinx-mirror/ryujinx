using OpenTK.Graphics.OpenGL;
using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class TextureCopyUnscaled
    {
        public static void Copy(TextureView src, TextureView dst, int dstLayer, int dstLevel)
        {
            int srcWidth  = src.Width;
            int srcHeight = src.Height;
            int srcDepth  = src.DepthOrLayers;
            int srcLevels = src.Levels;

            int dstWidth  = dst.Width;
            int dstHeight = dst.Height;
            int dstDepth  = dst.DepthOrLayers;
            int dstLevels = dst.Levels;

            dstWidth = Math.Max(1, dstWidth >> dstLevel);
            dstHeight = Math.Max(1, dstHeight >> dstLevel);

            if (dst.Target == Target.Texture3D)
            {
                dstDepth = Math.Max(1, dstDepth >> dstLevel);
            }

            // When copying from a compressed to a non-compressed format,
            // the non-compressed texture will have the size of the texture
            // in blocks (not in texels), so we must adjust that size to
            // match the size in texels of the compressed texture.
            if (!src.IsCompressed && dst.IsCompressed)
            {
                dstWidth  = BitUtils.DivRoundUp(dstWidth,  dst.BlockWidth);
                dstHeight = BitUtils.DivRoundUp(dstHeight, dst.BlockHeight);
            }
            else if (src.IsCompressed && !dst.IsCompressed)
            {
                dstWidth  *= dst.BlockWidth;
                dstHeight *= dst.BlockHeight;
            }

            int width  = Math.Min(srcWidth,  dstWidth);
            int height = Math.Min(srcHeight, dstHeight);
            int depth  = Math.Min(srcDepth,  dstDepth);
            int levels = Math.Min(srcLevels, dstLevels);

            for (int level = 0; level < levels; level++)
            {
                // Stop copy if we are already out of the levels range.
                if (level >= src.Levels || dstLevel + level >= dst.Levels)
                {
                    break;
                }

                GL.CopyImageSubData(
                    src.Handle,
                    src.Target.ConvertToImageTarget(),
                    level,
                    0,
                    0,
                    0,
                    dst.Handle,
                    dst.Target.ConvertToImageTarget(),
                    dstLevel + level,
                    0,
                    0,
                    dstLayer,
                    width,
                    height,
                    depth);

                width  = Math.Max(1, width  >> 1);
                height = Math.Max(1, height >> 1);

                if (src.Target == Target.Texture3D)
                {
                    depth = Math.Max(1, depth >> 1);
                }
            }
        }
    }
}
