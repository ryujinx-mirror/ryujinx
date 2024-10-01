using System;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Graphics.Shader.Translation
{
    [Flags]
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    enum AggregateType
    {
        Invalid,
        Void,
        Bool,
        FP32,
        FP64,
        S32,
        U32,

        ElementTypeMask = 0xff,

        ElementCountShift = 8,
        ElementCountMask = 3 << ElementCountShift,

        Scalar = 0 << ElementCountShift,
        Vector2 = 1 << ElementCountShift,
        Vector3 = 2 << ElementCountShift,
        Vector4 = 3 << ElementCountShift,

        Array = 1 << 10,
    }

    static class AggregateTypeExtensions
    {
        public static int GetSizeInBytes(this AggregateType type)
        {
            int elementSize = (type & AggregateType.ElementTypeMask) switch
            {
                AggregateType.Bool or
                AggregateType.FP32 or
                AggregateType.S32 or
                AggregateType.U32 => 4,
                AggregateType.FP64 => 8,
                _ => 0,
            };

            switch (type & AggregateType.ElementCountMask)
            {
                case AggregateType.Vector2:
                    elementSize *= 2;
                    break;
                case AggregateType.Vector3:
                    elementSize *= 3;
                    break;
                case AggregateType.Vector4:
                    elementSize *= 4;
                    break;
            }

            return elementSize;
        }
    }
}
