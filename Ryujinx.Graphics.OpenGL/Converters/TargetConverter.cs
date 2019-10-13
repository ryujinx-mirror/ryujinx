using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Texture;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class TargetConverter
    {
        public static ImageTarget ConvertToImageTarget(this Target target)
        {
            return (ImageTarget)target.Convert();
        }

        public static TextureTarget Convert(this Target target)
        {
            switch (target)
            {
                case Target.Texture1D:            return TextureTarget.Texture1D;
                case Target.Texture2D:            return TextureTarget.Texture2D;
                case Target.Texture3D:            return TextureTarget.Texture3D;
                case Target.Texture1DArray:       return TextureTarget.Texture1DArray;
                case Target.Texture2DArray:       return TextureTarget.Texture2DArray;
                case Target.Texture2DMultisample: return TextureTarget.Texture2DMultisample;
                case Target.Rectangle:            return TextureTarget.TextureRectangle;
                case Target.Cubemap:              return TextureTarget.TextureCubeMap;
                case Target.CubemapArray:         return TextureTarget.TextureCubeMapArray;
                case Target.TextureBuffer:        return TextureTarget.TextureBuffer;
            }

            throw new ArgumentException($"Invalid target \"{target}\".");
        }
    }
}
