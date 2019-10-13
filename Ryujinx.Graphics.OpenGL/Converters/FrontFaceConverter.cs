using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class FrontFaceConverter
    {
        public static FrontFaceDirection Convert(this FrontFace frontFace)
        {
            switch (frontFace)
            {
                case FrontFace.Clockwise:        return FrontFaceDirection.Cw;
                case FrontFace.CounterClockwise: return FrontFaceDirection.Ccw;
            }

            return FrontFaceDirection.Cw;

            throw new ArgumentException($"Invalid front face \"{frontFace}\".");
        }
    }
}
