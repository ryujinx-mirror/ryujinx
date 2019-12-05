using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.OpenGL
{
    static class CompareOpConverter
    {
        public static All Convert(this CompareOp op)
        {
            switch (op)
            {
                case CompareOp.Never:
                case CompareOp.NeverGl:
                    return All.Never;
                case CompareOp.Less:
                case CompareOp.LessGl:
                    return All.Less;
                case CompareOp.Equal:
                case CompareOp.EqualGl:
                    return All.Equal;
                case CompareOp.LessOrEqual:
                case CompareOp.LessOrEqualGl:
                    return All.Lequal;
                case CompareOp.Greater:
                case CompareOp.GreaterGl:
                    return All.Greater;
                case CompareOp.NotEqual:
                case CompareOp.NotEqualGl:
                    return All.Notequal;
                case CompareOp.GreaterOrEqual:
                case CompareOp.GreaterOrEqualGl:
                    return All.Gequal;
                case CompareOp.Always:
                case CompareOp.AlwaysGl:
                    return All.Always;
            }

            return All.Never;
        }
    }
}
