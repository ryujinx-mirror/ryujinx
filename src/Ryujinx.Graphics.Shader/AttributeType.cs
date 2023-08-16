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
        Sscaled,
        Uscaled,
    }

    static class AttributeTypeExtensions
    {
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

        public static AggregateType ToAggregateType(this AttributeType type, bool supportsScaledFormats)
        {
            return type switch
            {
                AttributeType.Float => AggregateType.FP32,
                AttributeType.Sint => AggregateType.S32,
                AttributeType.Uint => AggregateType.U32,
                AttributeType.Sscaled => supportsScaledFormats ? AggregateType.FP32 : AggregateType.S32,
                AttributeType.Uscaled => supportsScaledFormats ? AggregateType.FP32 : AggregateType.U32,
                _ => throw new ArgumentException($"Invalid attribute type \"{type}\"."),
            };
        }
    }
}
