using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class IndexTypeConverter
    {
        public static DrawElementsType Convert(this IndexType type)
        {
            switch (type)
            {
                case IndexType.UByte:  return DrawElementsType.UnsignedByte;
                case IndexType.UShort: return DrawElementsType.UnsignedShort;
                case IndexType.UInt:   return DrawElementsType.UnsignedInt;
            }

            throw new ArgumentException($"Invalid index type \"{type}\".");
        }
    }
}
