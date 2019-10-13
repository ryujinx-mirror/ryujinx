using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class CompareOpConverter
    {
        public static All Convert(this CompareOp op)
        {
            switch (op)
            {
                case CompareOp.Never:          return All.Never;
                case CompareOp.Less:           return All.Less;
                case CompareOp.Equal:          return All.Equal;
                case CompareOp.LessOrEqual:    return All.Lequal;
                case CompareOp.Greater:        return All.Greater;
                case CompareOp.NotEqual:       return All.Notequal;
                case CompareOp.GreaterOrEqual: return All.Gequal;
                case CompareOp.Always:         return All.Always;
            }

            return All.Never;

            throw new ArgumentException($"Invalid compare operation \"{op}\".");
        }
    }
}
