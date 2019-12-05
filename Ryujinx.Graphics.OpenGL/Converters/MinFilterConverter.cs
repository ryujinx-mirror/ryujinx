using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Sampler;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class MinFilterConverter
    {
        public static TextureMinFilter Convert(this MinFilter filter)
        {
            switch (filter)
            {
                case MinFilter.Nearest:              return TextureMinFilter.Nearest;
                case MinFilter.Linear:               return TextureMinFilter.Linear;
                case MinFilter.NearestMipmapNearest: return TextureMinFilter.NearestMipmapNearest;
                case MinFilter.LinearMipmapNearest:  return TextureMinFilter.LinearMipmapNearest;
                case MinFilter.NearestMipmapLinear:  return TextureMinFilter.NearestMipmapLinear;
                case MinFilter.LinearMipmapLinear:   return TextureMinFilter.LinearMipmapLinear;
            }

            return TextureMinFilter.Nearest;
        }
    }
}
