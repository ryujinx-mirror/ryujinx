using System;

namespace Ryujinx.Graphics.Shader
{
    public enum AttributeType : byte
    {
        Float,
        Sint,
        Uint
    }

    static class AttributeTypeExtensions
    {
        public static string GetScalarType(this AttributeType type)
        {
            return type switch
            {
                AttributeType.Float => "float",
                AttributeType.Sint => "int",
                AttributeType.Uint => "uint",
                _ => throw new ArgumentException($"Invalid attribute type \"{type}\".")
            };
        }

        public static string GetVec4Type(this AttributeType type)
        {
            return type switch
            {
                AttributeType.Float => "vec4",
                AttributeType.Sint => "ivec4",
                AttributeType.Uint => "uvec4",
                _ => throw new ArgumentException($"Invalid attribute type \"{type}\".")
            };
        }
    }
}