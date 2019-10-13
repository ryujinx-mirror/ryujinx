using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class FaceConverter
    {
        public static CullFaceMode Convert(this Face face)
        {
            switch (face)
            {
                case Face.Back:         return CullFaceMode.Back;
                case Face.Front:        return CullFaceMode.Front;
                case Face.FrontAndBack: return CullFaceMode.FrontAndBack;
            }

            return CullFaceMode.FrontAndBack;

            throw new ArgumentException($"Invalid face \"{face}\".");
        }
    }
}
