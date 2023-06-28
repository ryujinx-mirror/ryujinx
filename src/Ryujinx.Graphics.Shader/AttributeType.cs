using Ryujinx.Graphics.Shader.Translation;
using System;

namespace Ryujinx.Graphics.Shader
{
    public enum AttributeType : byte
    {
        // Generic types.
        Float,
        Sint,
        Uint,
    }

    static class AttributeTypeExtensions
    {
        public static string ToVec4Type(this AttributeType type)
        {
            return type switch
            {
                AttributeType.Float => "vec4",
                AttributeType.Sint => "ivec4",
                AttributeType.Uint => "uvec4",
                _ => throw new ArgumentException($"Invalid attribute type \"{type}\"."),
            };
        }

        public static AggregateType ToAggregateType(this AttributeType type)
        {
            return type switch
            {
                AttributeType.Float => AggregateType.FP32,
                AttributeType.Sint => AggregateType.S32,
                AttributeType.Uint => AggregateType.U32,
                _ => throw new ArgumentException($"Invalid attribute type \"{type}\"."),
            };
        }
    }
}
