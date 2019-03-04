using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OglLimit
    {
        private static Lazy<int> _sMaxUboSize = new Lazy<int>(() => GL.GetInteger(GetPName.MaxUniformBlockSize));

        public static int MaxUboSize => _sMaxUboSize.Value;
    }
}