using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation
{
    struct AttributeInfo
    {
        private static readonly Dictionary<int, AttributeInfo> _builtInAttributes = new Dictionary<int, AttributeInfo>()
        {
            { AttributeConsts.Layer,         new AttributeInfo(AttributeConsts.Layer,         0, 1, AggregateType.S32) },
            { AttributeConsts.ViewportIndex, new AttributeInfo(AttributeConsts.ViewportIndex, 0, 1, AggregateType.S32) },
            { AttributeConsts.PointSize,     new AttributeInfo(AttributeConsts.PointSize,     0, 1, AggregateType.FP32) },
            { AttributeConsts.PositionX,     new AttributeInfo(AttributeConsts.PositionX,     0, 4, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.PositionY,     new AttributeInfo(AttributeConsts.PositionX,     1, 4, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.PositionZ,     new AttributeInfo(AttributeConsts.PositionX,     2, 4, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.PositionW,     new AttributeInfo(AttributeConsts.PositionX,     3, 4, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.ClipDistance0, new AttributeInfo(AttributeConsts.ClipDistance0, 0, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance1, new AttributeInfo(AttributeConsts.ClipDistance0, 1, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance2, new AttributeInfo(AttributeConsts.ClipDistance0, 2, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance3, new AttributeInfo(AttributeConsts.ClipDistance0, 3, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance4, new AttributeInfo(AttributeConsts.ClipDistance0, 4, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance5, new AttributeInfo(AttributeConsts.ClipDistance0, 5, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance6, new AttributeInfo(AttributeConsts.ClipDistance0, 6, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.ClipDistance7, new AttributeInfo(AttributeConsts.ClipDistance0, 7, 8, AggregateType.Array  | AggregateType.FP32) },
            { AttributeConsts.PointCoordX,   new AttributeInfo(AttributeConsts.PointCoordX,   0, 2, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.PointCoordY,   new AttributeInfo(AttributeConsts.PointCoordX,   1, 2, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.TessCoordX,    new AttributeInfo(AttributeConsts.TessCoordX,    0, 3, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.TessCoordY,    new AttributeInfo(AttributeConsts.TessCoordX,    1, 3, AggregateType.Vector | AggregateType.FP32) },
            { AttributeConsts.InstanceId,    new AttributeInfo(AttributeConsts.InstanceId,    0, 1, AggregateType.S32) },
            { AttributeConsts.VertexId,      new AttributeInfo(AttributeConsts.VertexId,      0, 1, AggregateType.S32) },
            { AttributeConsts.FrontFacing,   new AttributeInfo(AttributeConsts.FrontFacing,   0, 1, AggregateType.Bool) },

            // Special.
            { AttributeConsts.FragmentOutputDepth, new AttributeInfo(AttributeConsts.FragmentOutputDepth, 0, 1, AggregateType.FP32) },
            { AttributeConsts.ThreadKill,          new AttributeInfo(AttributeConsts.ThreadKill,          0, 1, AggregateType.Bool) },
            { AttributeConsts.ThreadIdX,           new AttributeInfo(AttributeConsts.ThreadIdX,           0, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.ThreadIdY,           new AttributeInfo(AttributeConsts.ThreadIdX,           1, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.ThreadIdZ,           new AttributeInfo(AttributeConsts.ThreadIdX,           2, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.CtaIdX,              new AttributeInfo(AttributeConsts.CtaIdX,              0, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.CtaIdY,              new AttributeInfo(AttributeConsts.CtaIdX,              1, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.CtaIdZ,              new AttributeInfo(AttributeConsts.CtaIdX,              2, 3, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.LaneId,              new AttributeInfo(AttributeConsts.LaneId,              0, 1, AggregateType.U32) },
            { AttributeConsts.InvocationId,        new AttributeInfo(AttributeConsts.InvocationId,        0, 1, AggregateType.S32) },
            { AttributeConsts.PrimitiveId,         new AttributeInfo(AttributeConsts.PrimitiveId,         0, 1, AggregateType.S32) },
            { AttributeConsts.PatchVerticesIn,     new AttributeInfo(AttributeConsts.PatchVerticesIn,     0, 1, AggregateType.S32) },
            { AttributeConsts.EqMask,              new AttributeInfo(AttributeConsts.EqMask,              0, 4, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.GeMask,              new AttributeInfo(AttributeConsts.GeMask,              0, 4, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.GtMask,              new AttributeInfo(AttributeConsts.GtMask,              0, 4, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.LeMask,              new AttributeInfo(AttributeConsts.LeMask,              0, 4, AggregateType.Vector | AggregateType.U32) },
            { AttributeConsts.LtMask,              new AttributeInfo(AttributeConsts.LtMask,              0, 4, AggregateType.Vector | AggregateType.U32) },
        };

        private static readonly Dictionary<int, AttributeInfo> _builtInAttributesPerPatch = new Dictionary<int, AttributeInfo>()
        {
            { AttributeConsts.TessLevelOuter0, new AttributeInfo(AttributeConsts.TessLevelOuter0, 0, 4, AggregateType.Array | AggregateType.FP32) },
            { AttributeConsts.TessLevelOuter1, new AttributeInfo(AttributeConsts.TessLevelOuter0, 1, 4, AggregateType.Array | AggregateType.FP32) },
            { AttributeConsts.TessLevelOuter2, new AttributeInfo(AttributeConsts.TessLevelOuter0, 2, 4, AggregateType.Array | AggregateType.FP32) },
            { AttributeConsts.TessLevelOuter3, new AttributeInfo(AttributeConsts.TessLevelOuter0, 3, 4, AggregateType.Array | AggregateType.FP32) },
            { AttributeConsts.TessLevelInner0, new AttributeInfo(AttributeConsts.TessLevelInner0, 0, 2, AggregateType.Array | AggregateType.FP32) },
            { AttributeConsts.TessLevelInner1, new AttributeInfo(AttributeConsts.TessLevelInner0, 1, 2, AggregateType.Array | AggregateType.FP32) },
        };

        public int BaseValue { get; }
        public int Value { get; }
        public int Length { get; }
        public AggregateType Type { get; }
        public bool IsBuiltin { get; }
        public bool IsValid => Type != AggregateType.Invalid;

        public AttributeInfo(int baseValue, int index, int length, AggregateType type, bool isBuiltin = true)
        {
            BaseValue = baseValue;
            Value = baseValue + index * 4;
            Length = length;
            Type = type;
            IsBuiltin = isBuiltin;
        }

        public int GetInnermostIndex()
        {
            return (Value - BaseValue) / 4;
        }

        public static bool Validate(ShaderConfig config, int value, bool isOutAttr, bool perPatch)
        {
            return perPatch ? ValidatePerPatch(config, value, isOutAttr) : Validate(config, value, isOutAttr);
        }

        public static bool Validate(ShaderConfig config, int value, bool isOutAttr)
        {
            if (value == AttributeConsts.ViewportIndex && !config.GpuAccessor.QueryHostSupportsViewportIndex())
            {
                return false;
            }

            return From(config, value, isOutAttr).IsValid;
        }

        public static bool ValidatePerPatch(ShaderConfig config, int value, bool isOutAttr)
        {
            return FromPatch(config, value, isOutAttr).IsValid;
        }

        public static AttributeInfo From(ShaderConfig config, int value, bool isOutAttr)
        {
            value &= ~3;

            if (value >= AttributeConsts.UserAttributeBase && value < AttributeConsts.UserAttributeEnd)
            {
                int location = (value - AttributeConsts.UserAttributeBase) / 16;

                AggregateType elemType;

                if (config.Stage == ShaderStage.Vertex && !isOutAttr)
                {
                    elemType = config.GpuAccessor.QueryAttributeType(location).ToAggregateType();
                }
                else
                {
                    elemType = AggregateType.FP32;
                }

                return new AttributeInfo(value & ~0xf, (value >> 2) & 3, 4, AggregateType.Vector | elemType, false);
            }
            else if (value >= AttributeConsts.FragmentOutputColorBase && value < AttributeConsts.FragmentOutputColorEnd)
            {
                return new AttributeInfo(value & ~0xf, (value >> 2) & 3, 4, AggregateType.Vector | AggregateType.FP32, false);
            }
            else if (value == AttributeConsts.SupportBlockViewInverseX || value == AttributeConsts.SupportBlockViewInverseY)
            {
                return new AttributeInfo(value, 0, 1, AggregateType.FP32);
            }
            else if (_builtInAttributes.TryGetValue(value, out AttributeInfo info))
            {
                return info;
            }

            return new AttributeInfo(value, 0, 0, AggregateType.Invalid);
        }

        public static AttributeInfo FromPatch(ShaderConfig config, int value, bool isOutAttr)
        {
            value &= ~3;

            if (value >= AttributeConsts.UserAttributePerPatchBase && value < AttributeConsts.UserAttributePerPatchEnd)
            {
                int offset = (value - AttributeConsts.UserAttributePerPatchBase) & 0xf;
                return new AttributeInfo(value - offset, offset >> 2, 4, AggregateType.Vector | AggregateType.FP32, false);
            }
            else if (_builtInAttributesPerPatch.TryGetValue(value, out AttributeInfo info))
            {
                return info;
            }

            return new AttributeInfo(value, 0, 0, AggregateType.Invalid);
        }

        public static bool IsArrayBuiltIn(int attr)
        {
            if (attr <= AttributeConsts.TessLevelInner1 ||
                attr == AttributeConsts.TessCoordX ||
                attr == AttributeConsts.TessCoordY)
            {
                return false;
            }

            return (attr & AttributeConsts.SpecialMask) == 0;
        }

        public static bool IsArrayAttributeGlsl(ShaderStage stage, bool isOutAttr)
        {
            if (isOutAttr)
            {
                return stage == ShaderStage.TessellationControl;
            }
            else
            {
                return stage == ShaderStage.TessellationControl ||
                       stage == ShaderStage.TessellationEvaluation ||
                       stage == ShaderStage.Geometry;
            }
        }

        public static bool IsArrayAttributeSpirv(ShaderStage stage, bool isOutAttr)
        {
            if (isOutAttr)
            {
                return false;
            }
            else
            {
                return stage == ShaderStage.TessellationControl ||
                       stage == ShaderStage.TessellationEvaluation ||
                       stage == ShaderStage.Geometry;
            }
        }
    }
}
