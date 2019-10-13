using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Sampler;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class MagFilterConverter
    {
        public static TextureMagFilter Convert(this MagFilter filter)
        {
            switch (filter)
            {
                case MagFilter.Nearest: return TextureMagFilter.Nearest;
                case MagFilter.Linear:  return TextureMagFilter.Linear;
            }

            throw new ArgumentException($"Invalid filter \"{filter}\".");
        }
    }
}
