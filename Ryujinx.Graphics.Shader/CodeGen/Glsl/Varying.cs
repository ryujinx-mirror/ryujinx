using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    public static class Varying
    {
        public static string GetName(int offset)
        {
            offset <<= 2;

            if (offset >= AttributeConsts.UserAttributeBase &&
                offset <  AttributeConsts.UserAttributeEnd)
            {
                offset -= AttributeConsts.UserAttributeBase;

                string name = $"{ DefaultNames.OAttributePrefix}{(offset >> 4)}";

                name += "_" + "xyzw"[(offset >> 2) & 3];

                return name;
            }

            switch (offset)
            {
                case AttributeConsts.PositionX:
                case AttributeConsts.PositionY:
                case AttributeConsts.PositionZ:
                case AttributeConsts.PositionW:
                    return "gl_Position";
                case AttributeConsts.PointSize:
                    return "gl_PointSize";
                case AttributeConsts.ClipDistance0:
                    return "gl_ClipDistance[0]";
                case AttributeConsts.ClipDistance1:
                    return "gl_ClipDistance[1]";
                case AttributeConsts.ClipDistance2:
                    return "gl_ClipDistance[2]";
                case AttributeConsts.ClipDistance3:
                    return "gl_ClipDistance[3]";
                case AttributeConsts.ClipDistance4:
                    return "gl_ClipDistance[4]";
                case AttributeConsts.ClipDistance5:
                    return "gl_ClipDistance[5]";
                case AttributeConsts.ClipDistance6:
                    return "gl_ClipDistance[6]";
                case AttributeConsts.ClipDistance7:
                    return "gl_ClipDistance[7]";
                case AttributeConsts.VertexId:
                    return "gl_VertexID";
            }

            return null;
        }

        public static int GetSize(int offset)
        {
            switch (offset << 2)
            {
                case AttributeConsts.PositionX:
                case AttributeConsts.PositionY:
                case AttributeConsts.PositionZ:
                case AttributeConsts.PositionW:
                    return 4;
            }

            return 1;
        }
    }
}