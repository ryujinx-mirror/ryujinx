using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Sampler;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class CompareModeConverter
    {
        public static TextureCompareMode Convert(this CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.None:               return TextureCompareMode.None;
                case CompareMode.CompareRToTexture:  return TextureCompareMode.CompareRToTexture;
            }

            throw new ArgumentException($"Invalid compare mode \"{mode}\".");
        }
    }
}
