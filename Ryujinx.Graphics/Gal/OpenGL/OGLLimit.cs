using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLLimit
    {
        private static Lazy<int> s_MaxUboSize = new Lazy<int>(() => GL.GetInteger(GetPName.MaxUniformBlockSize));

        public static int MaxUboSize => s_MaxUboSize.Value;
    }
}